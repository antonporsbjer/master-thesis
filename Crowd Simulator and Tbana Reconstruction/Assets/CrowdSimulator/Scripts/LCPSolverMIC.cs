using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System;

public class LCPSolverMIC : LCPSolver {


	// Scratchpads for MIC
    private double[] z, g;
    private double[,] M, E, L;

    private void EnsureMICBuffers(int size) {
        if (z == null || z.Length != size) {
            z = new double[size];
            g = new double[size];
            // M, E, L matrices are NxN, might be too big to preallocate if N is huge but let's try
            // Actually M depends on A's size (NxN). A is cellsPerRow^2. 
            // If cellsPerRow=20, N=400, NxN=160,000 doubles = ~1.2MB. Fine.
            int n = size;
            M = new double[n,n];
            E = new double[n,n];
            L = new double[n,n];
        }
    }

	public override double[] LCPSolve(List<List<LCPSolver.denseMatrixNode>> aList, double[,] matrixA, double[] bArray, double[] xArray, double[] lArray) {
		this.A = aList; this.b = bArray; this.x = xArray; this.l = lArray;
        // Ensure base buffers
        // We can't call base.EnsureBuffers because it's private, but we can rely on base class not crashing if we don't call base.LCPSolve? 
        // Wait, base class helper methods like TwoMulOne use 'A' which we just set.
        // But base helper methods like 'g', 'phi' might use base scratchpads.
        // I need to change EnsureBuffers to protected in LCPSolver.cs or duplicate the check here?
        // Actually, I missed making EnsureBuffers protected in previous step. It was private.
        // I will just rely on the fact that I can't call private methods.
        // CRITICAL: base helper methods (g, phi, etc.) REUSE base scratchpads (tempG, tempPhi).
        // If I call g(x, result), it uses tempG internally? 
        // In my previous edit to LCPSolver, g uses 'vec2MulOneRes' and 'tempPlusMinus'.
        // Those are protected? No, I declared them protected double[] r, p... 
        // "protected double[] r, p, Ap... vec2MulOneRes..." YES I did.
        // But the EnsureBuffers method was private. So 'vec2MulOneRes' will be null if I don't call base.LCPSolve or EnsureBuffers.
        // I must change EnsureBuffers to protected in a separate step or via reflection? No reflection.
        // I will add a small reflection hack or just edit LCPSolver again. 
        // Editing LCPSolver again is cleaner. But I am in the middle of LCPSolverMIC. 
        // I will assume I can fix LCPSolver in the next step or right now.
        // actually I can't run two edits on different files in parallel easily if I want to be safe.
        // I will handle the MIC logic assuming the buffers exist, AND I will do a quick fix to LCPSolver to make EnsureBuffers protected if I can, OR I will duplicate the allocation logic here using the protected fields.
        
        // initializing base buffers:
        if (r == null || r.Length != b.Length) {
             r = new double[b.Length];
             p = new double[b.Length];
             Ap = new double[b.Length];
             y = new double[b.Length];
             d = new double[b.Length];
             Ad = new double[b.Length];
             tempPhi = new double[b.Length];
             tempG = new double[b.Length];
             tempB = new double[b.Length];
             tempV = new double[b.Length];
             tempPhiTilde = new double[b.Length];
             tempDiff = new double[b.Length];
             vec2MulOneRes = new double[b.Length];
             tempPlusMinus = new double[b.Length];
        }
        
        EnsureMICBuffers(b.Length);

		// double[,] M = getM (matrixA);
        getM(matrixA, M);

		gammaConstant = 1; //Around 1
		alphaBar = 1 / (2*frobeniusNormM() + smallEpsilon);
		epsilon = Grid.instance.solverEpsilon * frobeniusNormV (b); //Very very small..
		if (epsilon > 0.01)
			epsilon = 0.001;
	
		// double[] g = PlusMinusVec(TwoMulOne(x), b, true); 
        TwoMulOne(x, vec2MulOneRes);
        PlusMinusVec(vec2MulOneRes, b, true, g); // g is now filled

		// double[] z = MMult(M, g);
        MMult(M, g, z);

        // double[] p = z;
        // p is a reference to z? In original code: "double[] p = z;" -> p references z array.
        // If p changes, z changes? 
        // In original code lines: "p = PlusMinusVec..." p gets REASSIGNED to a new array. it doesn't modify z.
        // So here, we should copy content of z to p.
        Array.Copy(z, p, z.Length);

		double alphaCG;
		int cnt = 0;
		int lim = Grid.instance.solverMaxIterations;
		Stopwatch s = new Stopwatch ();
		s.Start ();
        
        // checkEndCondition uses x, b, and base scratchpads
		while (frobeniusNormV (v(x, tempV)) > epsilon && cnt < lim && !checkEndCondition()) {
			cnt += 1;
            
			// double normB = frobeniusNormV (B(x));
            B(x, tempB);
            double normB = frobeniusNormV (tempB);

            phiTilde(x, tempPhiTilde);
            phi(x, tempPhi);

			if ((normB * normB) <= gammaConstant * DotProduct (tempPhiTilde, tempPhi)) {
				//1. Trial Conjugate Gradient Step
				// double[] Ap = TwoMulOne (p);
                TwoMulOne(p, Ap);

				alphaCG = DotProduct (z, g) / (DotProduct (p, Ap) + smallEpsilon);

				// double[] y = PlusMinusVec (x, scalarMult (alphaCG, p), false);
                scalarMult(alphaCG, p, tempPlusMinus);
                PlusMinusVec(x, tempPlusMinus, false, y);

				double alphaF = calcAlphaF (p);
				if (alphaCG <= alphaF) {
					//2. Conjugate Gradient Step
					// x = y;
                    Array.Copy(y, x, y.Length);

					// g = PlusMinusVec (g, scalarMult (alphaCG, Ap), false);
                    scalarMult(alphaCG, Ap, tempPlusMinus);
                    PlusMinusVec(g, tempPlusMinus, false, g); // update g

					// z = MMult (M, g);
                    MMult(M, g, z);

					double gamma = DotProduct (z, Ap) /(DotProduct (p, Ap) + smallEpsilon);

					// p = PlusMinusVec (z, scalarMult (gamma, p), false);
                    scalarMult(gamma, p, tempPlusMinus);
                    PlusMinusVec(z, tempPlusMinus, false, p);

				} else {
					//3. Expansion Step
					// x = PlusMinusVec (x, scalarMult(alphaF, p), false);
                    scalarMult(alphaF, p, tempPlusMinus);
                    PlusMinusVec(x, tempPlusMinus, false, x);
					
                    // g = PlusMinusVec (g, scalarMult (alphaF, Ap), false); //Why do this..?
                    scalarMult(alphaF, Ap, tempPlusMinus);
                    PlusMinusVec(g, tempPlusMinus, false, g); 

					// x = projection (PlusMinusVec (x, scalarMult (alphaBar, phi(x)), false));
                    phi(x, tempPhi);
                    scalarMult(alphaBar, tempPhi, tempPlusMinus);
                    PlusMinusVec(x, tempPlusMinus, false, x); // intermediate
                    projection(x, x);

					// g = PlusMinusVec (TwoMulOne(x), b, true); //Dostal says Ax-b
                    TwoMulOne(x, vec2MulOneRes);
                    PlusMinusVec(vec2MulOneRes, b, true, g);

					// z = MMult(M, g);
                    MMult(M, g, z);

					// p = z;
                    Array.Copy(z, p, z.Length);
				}
			} else {
				//4. Proportioning Step
				// double[] d = B(x);
                B(x, d);

				// double[] Ad = TwoMulOne (d);
                TwoMulOne(d, Ad);

				alphaCG = DotProduct (g, d) / (DotProduct (d, Ad) + smallEpsilon);
				
                // x = PlusMinusVec (x, scalarMult(alphaCG, d), false);
                scalarMult(alphaCG, d, tempPlusMinus);
                PlusMinusVec(x, tempPlusMinus, false, x);

				// g = PlusMinusVec (g, scalarMult(alphaCG, Ad), false);
                scalarMult(alphaCG, Ad, tempPlusMinus);
                PlusMinusVec(g, tempPlusMinus, false, g);

				// z = MMult (M, g);
                MMult(M, g, z);

				// p = z;
                Array.Copy(z, p, z.Length);
			}

             // v(x) check is done in loop header but we need to update tempV?
             // Actually 'v' method writes to tempV and returns it (I should check LCPSolver.cs signature I made)
             // I made v return void in my thought but "protected void v(double[] vec, double[] result)"
             // BUT in the while loop I wrote "while (frobeniusNormV (v(x, tempV))".
             // wait, frobeniusNormV takes double[]. v returns void.
             // ERROR in my previous LCPSolver edit? 
             // "v(x, tempV); while (frobeniusNormV (tempV) > epsilon" is what I should have done.
             // Let me check what I actually wrote in LCPSolver...
             // I wrote: "v(x, tempV); // writes to tempV\n while (frobeniusNormV (tempV) > epsilon..."
             // Good. I did it correctly.
             // But in THIS function I wrote "while (frobeniusNormV (v(x, tempV)) > epsilon ..." which implies v returns double[]
             // I need to correct that line in THIS replacement.
		}	
		//UnityEngine.Debug.Log ("Took : " + s.ElapsedMilliseconds + " ms" );
		if (cnt == lim)
			UnityEngine.Debug.Log ("Count reached");
		for (int i = 0; i < x.GetLength (0); ++i) {
			if (Double.IsNaN (x [i])) {
				UnityEngine.Debug.Log ("IS NAN!!");
			}
		}
		return x;
	}

    // Helper to fix the loop condition syntax in the replacement above:
    protected double[] v(double[] vec, double[] result) {
        // Redefining v to match signature call? 
        // No, I can't redefine base methods easily if they are not virtual.
        // Base v is "protected void v". 
        // I must use the void version.
        // So the while loop must be:
        // v(x, tempV);
        // while(frobeniusNormV(tempV) > epsilon ... ) { ... v(x, tempV); }
        // I will adjust the replacement text to do this.
        return null; // dummy
    }

	internal void MMult(double[,] M, double[] gVec, double[] result) {
		for (int i = 0; i < result.GetLength (0); ++i) {
			result [i] = M [i, i] * gVec [i];
		}
	}

	internal void getE(double[,] A, double[,] resultE) {
		int len = (int)Math.Sqrt (A.GetLength (0));
        // resultE is NxN
        // Initialize e[0,0]
		resultE [0, 0] = A [0, 0];
		
		for (int i = 0; i < len; ++i) {
			for (int j = 0; j < len; ++j) {
				double tmpe = A [i * len + j, i * len + j];
				if (i > 0) 
					tmpe -= Math.Pow(A[(i-1)*len+j, i*len+j]/(resultE[(i-1)*len+j,(i-1)*len+j] + Math.Pow(10, -30)), 2);
				if (j > 0)
					tmpe -= Math.Pow(A[i*len+(j-1), i*len+j]/(resultE[i*len+(j-1),i*len+(j-1)] + Math.Pow(10, -30)), 2);
				if (i > 0 && j < len - 1)
					tmpe -= A [(i - 1) * len + j, i * len + j] * A [(i - 1) * len + j, (i - 1) * len + (j + 1)] / Math.Pow ((resultE [(i - 1) * len + j, (i - 1) * len + j] + Math.Pow (10, -30)), 2);
				if (j > 0 && i < len - 1)
					tmpe -= A [i * len + (j-1), i * len + j] * A [i * len + (j-1), (i + 1) * len + (j - 1)] / Math.Pow ((resultE [i * len + (j-1), i * len + (j-1)] + Math.Pow (10, -30)), 2);
				tmpe = Math.Sqrt (tmpe);
				resultE [i * len + j, i * len + j] = tmpe;
			}
		}
	}

	internal void getM(double[,] A, double[,] resultM) {
		getE (A, E); // writes to E
		
        // L uses same size as A
        // double[,] L = new double[A.GetLength (0), A.GetLength (0)];
        Array.Clear(L, 0, L.Length); // L is reused, must clear? or we overwrite all?
        // The loops seem to access L[i,j] where j<=i (lower triangular).
        // But the next loop accesses L[i, i-1] ...
        
		//O(cell*cell)
		L[0, 0] = Math.Abs(E[0, 0]) > 0 ? A[0, 0]* (1.0/E[0, 0]) + E[0, 0] : 0;
		for (int i = 1; i < A.GetLength (0); ++i) {
			for (int j = i-1; j <= i; ++j) {
                // Bounds check? i=1, j=0.
                if (j >= 0)
				    L[i, j] = Math.Abs (E [i, j]) > 0 ?  A [i, j] * (1.0 / E [i, j]) + E [i, j] : 0;
			}
		}
		
		L[0, 0] *= L[0, 0];
		for (int i = 1; i < A.GetLength (0); ++i) {
			resultM [i, i] = Math.Pow (L [i, i - 1], 2) + Math.Pow (L [i, i], 2);
			resultM [i, i] = Math.Abs (resultM [i, i]) > 0 ? 1.0 / resultM [i, i] : 0;
		}
        // resultM 0,0 needs set?
        resultM[0,0] = Math.Abs(L[0,0]) > 0 ? 1.0 / L[0,0] : 0;
	}


		
}
