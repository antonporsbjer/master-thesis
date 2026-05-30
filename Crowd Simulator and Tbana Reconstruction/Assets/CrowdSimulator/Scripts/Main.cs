using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Main : MonoBehaviour {

	public static Main instance;

	public enum Method{
		uniformSpawn,
		circleSpawn,
		discSpawn,
		continuousSpawn,
		areaSpawn
	}

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



	public GameObject agentPrefabs;
	public GameObject groupAgentPrefabs;
	public Agent shirtColorPrefab;


	public SimulationGrid gridPrefab;
	public MapGen mapGen;
	public Plane plane;
	internal static Vector2 xMinMax;
	internal static Vector2 zMinMax;
	internal MapGen.map roadmap;

	public int cellSize;
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
	private SimulationGrid simulationGrid;

	void Awake()
	{
		instance = this;
	}

	/**
	 * Initialize simulation by taking the user's options into consideration and spawn agents.
	 * Then create the Staggered SimulationGrid along with all cells and velocity nodes.
	**/
	void OnEnable () {
		plane.transform.localScale = new Vector3(planeSizeX, 1.0f, planeSizeZ);
		Vector3 planeLength = plane.getLengths(); //Staggered grid length
		xMinMax = new Vector2(plane.transform.position.x - planeLength.x / 2,
							   plane.transform.position.x + planeLength.x / 2);
		zMinMax = new Vector2(plane.transform.position.z - planeLength.z / 2,
							  plane.transform.position.z + planeLength.z / 2);

		ringDiameter = agentAvoidanceRadius * 2; //Prefered distance between two agents

		//Creates roadmap / pathfinding for agents based on map
		MapGen m = Instantiate (mapGen) as MapGen; 
		roadmap = m.generateRoadMap (roadNodeAmount, xMinMax, zMinMax, visibleMap);

		SimulationGrid grid = Instantiate(gridPrefab) as SimulationGrid;
		grid.showSplattedDensity = showSplattedDensity;
		grid.showSplattedVelocity = showSplattedVelocity;
		grid.cellSize = cellSize;
		grid.agentMaxSpeed = agentMaxSpeed;
		grid.ringDiameter = ringDiameter;
		grid.usePresetGroupDistances = usePresetGroupDistances;
		grid.groupDistances = new float[] { p1p2, p2p3, p3p4 };
		grid.mapGen = mapGen;
		grid.dt = timeStep;
		grid.neighbourBins = neighbourBins;
		grid.solver = solver;
		grid.solverEpsilon = epsilon;
		grid.solverMaxIterations = solverMaxIterations;
		grid.colHandler = handleCollision;
		grid.agentAvoidanceRadius = agentAvoidanceRadius;
		SimulationGrid.instance = grid;
		SimulationGrid.instance.initGrid(xMinMax, zMinMax, alpha, agentAvoidanceRadius);

		for (int i = 0; i < roadmap.spawns.Count; ++i)
		{
			roadmap.spawns[i].spawner.InitializeSpawner(ref roadmap, ref agentList, xMinMax, zMinMax, agentAvoidanceRadius);
		}

		if(customTimeStep)
		{
			Physics.simulationMode = SimulationMode.Script;
			Debug.Log("Simulation mode set to Script");
		}

		simulationGrid = SimulationGrid.instance;

		simulationGrid.solver = solver;
		simulationGrid.solverEpsilon = epsilon;
		simulationGrid.solverMaxIterations = solverMaxIterations;
		//flags
		simulationGrid.showSplattedDensity = showSplattedDensity;
		simulationGrid.showSplattedVelocity = showSplattedVelocity;
		simulationGrid.walkBack = walkBack;
		simulationGrid.skipNodeIfSeeNext = skipNodeIfSeeNext;
		simulationGrid.smoothTurns = smoothTurns;
	}
	

	/**
	 * Main simulation loop which is called every frame
	**/
	void Update () {

		simulationGrid.dt = customTimeStep ? timeStep : Time.deltaTime;

		// Update grid with new density and velocity values
		simulationGrid.updateCellDensity ();
		simulationGrid.updateVelocityNodes ();
		//Solve linear constraint problem
		simulationGrid.PsolveRenormPsolve ();
		//Move agents
		for (int i = agentList.Count - 1; i >= 0; i--)
		{
			Agent agent = agentList[i];

			agent.CheckPositionAndRotation();

			// remove agent if it is outside the bounds of the plane
			if (Mathf.Abs(agent.tr.position.x) > planeSizeX * 5f || Mathf.Abs(agent.tr.position.z) > planeSizeZ * 5f || agent.tr.position.y > 0.5f)
			{
				Debug.LogWarning("Agent outside of bounds, removing");
				agentList.RemoveAt(i);
				Destroy(agent.gameObject);
			}

			if (agent.done)
			{
				Destroy(agent.gameObject);
				agentList.RemoveAt(i);
				continue;
			}
			agent.move(roadmap);
			agent.rbody.velocity = Vector3.zero;
			agent.rbody.angularVelocity = Vector3.zero;
		}
		//Pair-wise collision handling between agents
		simulationGrid.collisionHandling(agentList);


		if(customTimeStep)
		{
			Physics.Simulate(simulationGrid.dt);
		}
	}

	public void AddToAgentList(Agent agent)
	{
		agentList.Add(agent);
	}
}
