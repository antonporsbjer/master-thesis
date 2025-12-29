using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Data.Common;

public class Main : MonoBehaviour {

	public enum LCPSolutioner {
		mprgp,
		mprgpmic0,
		psor
	}
	public float epsilon;
	public int solverMaxIterations;
	public LCPSolutioner solver;
	

	public float planeSizeX;
	public float planeSizeZ;
	

	
	public float agentAvoidanceRadius;
	public float agentMaxSpeed;
	public float agentMinSpeed;
	public bool usePresetGroupDistances;
	public float p1p2, p2p3, p3p4;



	public Grid gridPrefab;
	public NewSpawner spawnerPrefab;
	public MapGen mapGen;
	public Plane plane;
	internal static Vector2 xMinMax;
	internal static Vector2 zMinMax;
	internal MapGen.map roadmap;

	public int cellsPerRow;
	public int neighbourBins;
	public int roadNodeAmount; // Number of nodes that are placed automatically
	public bool visibleMap; // Show or hide the nodes in the world
	internal float ringDiameter;

	public bool customTimeStep;
	public float timeStep; 

	[Range(0.01f, 1f)]
	public float alpha; 

	internal List<Agent> agentList = new List<Agent>();
	public int maxNumberOfAgents = 1000; // Maximum number of agents when spawning continuously

	public bool showSplattedDensity = false;
	public bool showSplattedVelocity = false;
	public bool walkBack = false;
	public bool skipNodeIfSeeNext = false;
	public bool smoothTurns = false;
	public bool handleCollision = false;

	/**
	 * Initialize simulation by taking the user's options into consideration and spawn agents.
	 * Then create the Staggered Grid along with all cells and velocity nodes.
	**/
	void OnEnable () {
		bool error = false; 
		if (error)
			return;
		
		plane.transform.localScale = new Vector3 (planeSizeX, 1.0f, planeSizeZ);
		Vector3 planeLength = plane.getLengths (); //Staggered grid length
		xMinMax = new Vector2 (plane.transform.position.x - planeLength.x / 2, 
			                   plane.transform.position.x + planeLength.x / 2);
		zMinMax = new Vector2 (plane.transform.position.z - planeLength.z / 2, 
							  plane.transform.position.z + planeLength.z / 2);

		ringDiameter = agentAvoidanceRadius * 2; //Prefered distance between two agents

		//Creates roadmap / pathfinding for agents based on map
		MapGen m = Instantiate (mapGen) as MapGen; 
		roadmap = m.generateRoadMap (roadNodeAmount, xMinMax, zMinMax, visibleMap);

		Grid grid = Instantiate (gridPrefab) as Grid;
		grid.showSplattedDensity = showSplattedDensity;
		grid.showSplattedVelocity = showSplattedVelocity;
		grid.cellsPerRow = cellsPerRow;
		grid.agentMaxSpeed = agentMaxSpeed;
		grid.ringDiameter = ringDiameter;
		grid.usePresetGroupDistances = usePresetGroupDistances;
		grid.groupDistances = new float[] {p1p2, p2p3, p3p4};
		grid.mapGen = mapGen;
		grid.dt = timeStep; 
		grid.neighbourBins = neighbourBins;
		grid.solver = solver;
		grid.solverEpsilon = epsilon;
		grid.solverMaxIterations = solverMaxIterations;
		grid.colHandler = handleCollision;
		grid.agentAvoidanceRadius = agentAvoidanceRadius;
		Grid.instance = grid;
		Grid.instance.initGrid (xMinMax, zMinMax, alpha, agentAvoidanceRadius);

		for (int i = 0; i < roadmap.spawns.Count; ++i)
		{
			//roadmap.spawns[i].spawner.InitializeSpawner (ref agentPrefabs, ref groupAgentPrefabs, ref shirtColorPrefab, ref roadmap, 
			//								 ref agentList, xMinMax, zMinMax, agentAvoidanceRadius);
			roadmap.spawns[i].spawner.InitializeSpawner(ref roadmap, 
											 ref agentList, xMinMax, zMinMax, agentAvoidanceRadius);
		}
	}


    /**
	 * Main simulation loop which is called every frame
	**/
	[Range(0.1f, 10f)]
    public float simulationSpeed = 1.0f;
    
    // Global simulation time tracker
    public static float SimulationTime = 0f;

    private float accumulatedTime = 0f;

    void StepSimulation(float dt) {
        SimulationTime += dt;
        Grid.instance.dt = dt;
        
        // Update grid with new density and velocity values
		Grid.instance.updateCellDensity ();
		Grid.instance.updateVelocityNodes ();
		//Solve linear constraint problem
		Grid.instance.PsolveRenormPsolve ();
		//Move agents
		for (int i = agentList.Count - 1; i >= 0; i--)
		{
			Agent agent = agentList[i];

			if (agent.transform.position.y > 0.1f ||
			agent.transform.position.y < -0.1f ||
			agent.transform.rotation.x < -0.1 ||
			agent.transform.rotation.x > 0.1 ||
			agent.transform.rotation.z > 0.1 ||
			agent.transform.rotation.z < -0.1)
			{
				//Debug.Log(transform.position.y + " " + transform.rotation.x + " " + transform.rotation.z);
				agent.Reset();
				//Debug.DrawLine(agent.transform.position, agent.transform.position + Vector3.up * 5f, Color.red, 2f);
			}

			// remove agent if it is outside the bounds of the plane
			if (Mathf.Abs(agent.transform.position.x) > planeSizeX * 5f || Mathf.Abs(agent.transform.position.z) > planeSizeZ * 5f || agent.transform.position.y > 0.5f)
			{
				Debug.Log("Agent outside of bounds, removing");
				agentList.RemoveAt(i);
				Destroy(agent.gameObject);
			}

			if (agent.done)
			{
				agentList.RemoveAt(i);
				Destroy(agent.gameObject);
				continue;
			}
			agent.move(ref roadmap);
            if (agent.rbody != null) {
			    agent.rbody.velocity = Vector3.zero;
			    agent.rbody.angularVelocity = Vector3.zero;
            }
			
		}
		//Pair-wise collision handling between agents
		Grid.instance.collisionHandling(ref agentList);

		//flags
		Grid.instance.showSplattedDensity = showSplattedDensity;
		Grid.instance.showSplattedVelocity = showSplattedVelocity;
		Grid.instance.walkBack = walkBack;
		Grid.instance.skipNodeIfSeeNext = skipNodeIfSeeNext;
		Grid.instance.smoothTurns = smoothTurns;
    }

    void Update() {
        Grid.instance.solver = solver;
		Grid.instance.solverEpsilon = epsilon;
		Grid.instance.solverMaxIterations = solverMaxIterations;

        float baseStep = customTimeStep ? timeStep : Time.deltaTime;
        if (customTimeStep) {
            // If custom time step is used, we just run once with that step? 
            // Or do we want to scale it? 
            // Assuming simulationSpeed helps multiply the effectiveness or frequency:
            // Let's treat simulationSpeed as a multiplier for wall-clock time accumulation.
            
            accumulatedTime += Time.deltaTime * simulationSpeed;
            while (accumulatedTime >= timeStep) {
                StepSimulation(timeStep);
                accumulatedTime -= timeStep;
            }
        } else {
            // Using Time.deltaTime variable steps.
            // If speed is 2.0, we pass 2*dt ? Or run update twice? 
            // Running update twice is more stable for physics than passing a huge dt.
            
            // Current CrowdSim implementation uses `Grid.instance.dt` in calculations.
            // If dt is too large, LCP might become unstable.
            // Safe approach: accumulate time and step with a fixed maximum dt (e.g. 0.02s)
            
            accumulatedTime += Time.deltaTime * simulationSpeed;
            float maxStep = 0.033f; // 30 fps min
            
            while(accumulatedTime > 0) {
                 float step = Mathf.Min(accumulatedTime, maxStep);
                 StepSimulation(step);
                 accumulatedTime -= step;
            }
        }
    }
	public void AddToAgentList(Agent agent)
	{
		agentList.Add(agent);
	}

}
