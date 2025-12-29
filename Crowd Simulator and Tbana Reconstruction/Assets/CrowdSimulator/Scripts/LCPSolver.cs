using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System;

public class LCPSolver {


	public struct denseMatrixNode{
		public int colIndex;
		public double value;
		public denseMatrixNode(int c, double v) {
			colIndex = c; value = v;
		}
	}
	protected double alphaBar, gammaConstant, epsilon;
	protected double[] b, x, l;
    
    // Scratchpad arrays for zero-allocation
    protected double[] r, p, Ap, y, d, Ad, tempPhi, tempG, tempB, tempV, tempPhiTilde, tempDiff, vec2MulOneRes, oneMulOneRes, tempPlusMinus;

	protected List<List<denseMatrixNode>> A;
	internal double smallEpsilon = 0.000000000000001;

    private void EnsureBuffers(int size) {
        if (r == null || r.Length != size) {
            r = new double[size];
            p = new double[size];
            Ap = new double[size];
            y = new double[size];
            d = new double[size];
            Ad = new double[size];
            tempPhi = new double[size];
            tempG = new double[size];
            tempB = new double[size];
            tempV = new double[size];
            tempPhiTilde = new double[size];
            tempDiff = new double[size];
            vec2MulOneRes = new double[size];
            tempPlusMinus = new double[size];
        }
    }

	internal bool checkEndCondition() {
        TwoMulOne(x, vec2MulOneRes); // writes to vec2MulOneRes
		PlusMinusVec (vec2MulOneRes, b, true, tempPlusMinus); // writes to tempPlusMinus
        double[] z = tempPlusMinus;

		//z >= 0
		bool conditionA = true;
		//x >= 0
		bool conditionB = true;
		//zTx = 0
		bool conditionC = false;
		double sum = 0.0;

		for (int i = 0; i < z.GetLength (0); ++i) {
			if (z [i] < 0)
				conditionA = false;
			if (x [i] < 0)
				conditionB = false;
			sum += z [i] * x [i];
		}
		if (Math.Abs (sum) < epsilon) {
			conditionC = true;
		}
		return conditionA && conditionB && conditionC;
	}


	public virtual double[] LCPSolve(List<List<denseMatrixNode>> aList, double[,] aMatrix, double[] bArray, double[] xArray, double[] lArray) {
		this.A = aList; this.b = bArray; this.x = xArray; this.l = lArray;
        EnsureBuffers(b.Length);

		gammaConstant = 1; //Around 1
		alphaBar = 1 / (2*frobeniusNormM() + smallEpsilon);
		epsilon = Grid.instance.solverEpsilon * frobeniusNormV (b); //Very very small..
		if (epsilon > 0.01)
			epsilon = 0.001;
			//		UnityEngine.Debug.Log ("Epsilon: " + epsilon);
		
        // double[] r = PlusMinusVec(TwoMulOne(x), b, true); 
        TwoMulOne(x, vec2MulOneRes);
        PlusMinusVec(vec2MulOneRes, b, true, r);

        // double[] p = phi(x);
        phi(x, p);

		double alphaCG;
		int cnt = 0;
		int lim = Grid.instance.solverMaxIterations;
		Stopwatch s = new Stopwatch ();
		s.Start ();
        
        // frobeniusNormV (v(x)) > epsilon
        v(x, tempV); // writes to tempV
		while (frobeniusNormV (tempV) > epsilon && cnt < lim && !checkEndCondition()  ) {
			cnt += 1;
            
            // double normB = frobeniusNormV (B(x));
            B(x, tempB);
		    double normB = frobeniusNormV (tempB);
            
            // DotProduct (phiTilde(x), phi(x))
            phiTilde(x, tempPhiTilde);
            phi(x, tempPhi);

		    if ((normB * normB) <= gammaConstant * DotProduct (tempPhiTilde, tempPhi)) {
				//1. Trial Conjugate Gradient Step
				// double[] Ap = TwoMulOne (p);
                TwoMulOne(p, Ap);

				alphaCG = DotProduct (r, p) / (DotProduct (p, Ap)  + smallEpsilon);
				
                // double[] y = PlusMinusVec (x, scalarMult (alphaCG, p), false);
                scalarMult(alphaCG, p, tempPlusMinus); // writes to tempPlusMinus
                PlusMinusVec(x, tempPlusMinus, false, y); 

				double alphaF = calcAlphaF (p);
				if (alphaCG <= alphaF) {
					//2. Conjugate Gradient Step
						for (int i = 0; i < y.GetLength (0); ++i)
								x [i] = y [i];
	//				x = y;
                    // r = PlusMinusVec (r, scalarMult (alphaCG, Ap), false);
                    scalarMult(alphaCG, Ap, tempPlusMinus);
					PlusMinusVec (r, tempPlusMinus, false, r); // update r
					
                    // double gamma = DotProduct (phi(y), Ap) / (DotProduct (p, Ap)  + smallEpsilon);
                    phi(y, tempPhi);
                    double gamma = DotProduct (tempPhi, Ap) / (DotProduct (p, Ap)  + smallEpsilon);
					
                    // p = PlusMinusVec (phi(y), scalarMult (gamma, p), false);
                    scalarMult(gamma, p, tempPlusMinus);
                    PlusMinusVec(tempPhi, tempPlusMinus, false, p);

				} else {
						//3. Expansion Step
                        // x = PlusMinusVec (x, scalarMult(alphaF, p), false);
						scalarMult(alphaF, p, tempPlusMinus);
                        PlusMinusVec(x, tempPlusMinus, false, x); // update x

						// r = PlusMinusVec (r, scalarMult (alphaF, Ap), false); //Why do this..?
                        scalarMult(alphaF, Ap, tempPlusMinus);
                        PlusMinusVec(r, tempPlusMinus, false, r);

                        // x = projection (PlusMinusVec (x, scalarMult (alphaBar, phi(x)), false));
                        phi(x, tempPhi);
                        scalarMult(alphaBar, tempPhi, tempPlusMinus);
                        PlusMinusVec(x, tempPlusMinus, false, x); // writes to x temporarily
                        projection(x, x); // projects in-place

                        // r = PlusMinusVec (TwoMulOne(x), b, true); //Dostal says Ax-b
                        TwoMulOne(x, vec2MulOneRes);
						PlusMinusVec (vec2MulOneRes, b, true, r);
                        
                        // p = phi(x);
						phi(x, p);
				}
			} else {
					//4. Proportioning Step
					//double[] d = B(x);
                    B(x, d);

					// double[] Ad = TwoMulOne (d);
                    TwoMulOne(d, Ad);

					alphaCG = DotProduct (r, d) / (DotProduct (d, Ad)+ smallEpsilon);
					
                    // x = PlusMinusVec (x, scalarMult(alphaCG, d), false);
                    scalarMult(alphaCG, d, tempPlusMinus);
                    PlusMinusVec(x, tempPlusMinus, false, x);

                    // r = PlusMinusVec (r, scalarMult(alphaCG, Ad), false);
                    scalarMult(alphaCG, Ad, tempPlusMinus);
					PlusMinusVec (r, tempPlusMinus, false, r);
					
                    // p = phi(x);
                    phi(x, p);
			}

            // Re-calculate v(x) for loop condition
            v(x, tempV);
		}	
		//UnityEngine.Debug.Log ("Took : " + s.ElapsedMilliseconds + " ms" );
		if (cnt == lim)
			UnityEngine.Debug.Log ("Yep..");
	//	UnityEngine.Debug.Log ("Iterations: " + cnt);
		return x;
		}

    // VOID methods with result param
	protected void g(double[] vec, double[] result) {
        TwoMulOne(vec, vec2MulOneRes);
		PlusMinusVec(vec2MulOneRes, b, true, result); //Ax + b (dostal has - instead)
	}

	protected void phi(double[] vec, double[] result) {
        g(vec, tempG);
		for (int i = 0; i < vec.GetLength (0); ++i) {
			if (vec [i].Equals (l[i])) {
				result [i] = 0.0;
			} else {
				result [i] = tempG [i];
			}
		}
	}

	protected void B(double[] vec, double[] result) {
        g(vec, tempG);
		for (int i = 0; i < vec.GetLength (0); ++i) {
			if (vec [i].Equals (l [i])) {
				result[i] = Math.Min(tempG [i], 0.0);
			} else {
				result[i] = 0.0;
			}
		}
	}

	protected void v(double[] vec, double[] result) {
        phi(vec, tempPhi);
        B(vec, tempB);
		PlusMinusVec (tempPhi, tempB, true, result);
	}

	protected void phiTilde(double[] vec, double[] result) {
        phi(vec, tempPhi);
		for (int i = 0; i < vec.GetLength(0); ++i) {
			result[i] = Math.Min((vec [i] - l [i]) / alphaBar, tempPhi[i]);
		}
	}
		
	protected void projection(double[] vec, double[] result) {
		PlusMinusVec (vec, l, false, tempDiff);
		for (int i = 0; i < tempDiff.GetLength (0); ++i) {
			tempDiff [i] = Math.Max (tempDiff[i], 0.0);
		} 
		PlusMinusVec (l, tempDiff, true, result); 
	}

	protected double calcAlphaF(double[] p) {
		double alphaF = 100000f; //Large nr(double)(Mathf.Infinity);
		for (int i = 0; i < x.GetLength(0); i++) {
			if (p[i] > 0) {
				double temp = (x[i] - l[i])/p[i];
				if (temp < alphaF) {
					alphaF = temp;
				}
			}
		}
		return alphaF;
	}

	protected double frobeniusNormV(double[] vec) {
		double norm = 0.0;
		for (int i = 0; i < vec.GetLength (0); ++i) {
			norm += Math.Pow(vec[i], 2);
		}
		return Math.Sqrt (norm);
	}

	protected double frobeniusNormM() {
		double norm = 0.0;
		for (int i = 0; i < A.Count; ++i) {
				for (int j = 0; j < A[i].Count; ++j) {
						norm += Math.Pow(A[i][j].value, 2);
				}
		}
		return Math.Sqrt (norm);
	}
		
	protected double DotProduct(double[] vec1, double[] vec2){
		double tVal = 0;
		for (int x = 0; x < vec1.Length; x++){
			tVal += vec1[x] * vec2[x];
		}
		return tVal;
	}

	protected void TwoMulOne(double[] vec, double[] result) {
		for (int i = 0; i < A.Count; ++i) {
			result [i] = OneMultOne (i, vec);
		}
	}

	protected double OneMultOne(int i, double[] vec) {
		double res = 0;
		for(int j = 0; j < A[i].Count; ++j) {
			res += A[i][j].value * vec[A[i][j].colIndex];
		}
		return res;
	}
	protected void scalarMult(double scalar, double[] vec, double[] result) {
		for (int i = 0; i < vec.GetLength (0); ++i) {
			result [i] = vec[i] * scalar;
		}
	}

	protected void PlusMinusVec(double[] a, double[] b, bool plus, double[] result) {
		double op = 1.0;
		if (!plus)
			op = -1.0;
		for (int i = 0; i < a.GetLength (0); ++i) {
			result [i] = a [i] + op*b [i];
		}
	}
}
