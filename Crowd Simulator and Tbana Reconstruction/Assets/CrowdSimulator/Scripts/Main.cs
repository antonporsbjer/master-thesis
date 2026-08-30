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
	public ExperimentHUD experimentHUD;
	private float simulationStartTimer = 0f;
	private bool simulationStarted = false;

	void Awake()
	{
		instance = this;
	}

	/**
	 * Initialize simulation by taking the user's options into consideration and spawn agents.
	 * Then create the Staggered SimulationGrid along with all cells and velocity nodes.
	**/
	void OnEnable () {
		if (plane != null)
		{
			Vector3 planeLength = plane.getLengths(); // Staggered grid length from Plane mesh/scale
			planeSizeX = plane.transform.localScale.x;
			planeSizeZ = plane.transform.localScale.z;

			xMinMax = new Vector2(plane.transform.position.x - planeLength.x / 2f,
								  plane.transform.position.x + planeLength.x / 2f);
			zMinMax = new Vector2(plane.transform.position.z - planeLength.z / 2f,
								  plane.transform.position.z + planeLength.z / 2f);
		}
		else
		{
			float lengthX = planeSizeX * 10f;
			float lengthZ = planeSizeZ * 10f;
			xMinMax = new Vector2(-lengthX / 2f, lengthX / 2f);
			zMinMax = new Vector2(-lengthZ / 2f, lengthZ / 2f);
		}

		ringDiameter = agentAvoidanceRadius * 2; //Prefered distance between two agents

		//Creates roadmap / pathfinding for agents based on map
		MapGen m = Instantiate (mapGen) as MapGen; 
		roadmap = m.generateRoadMap (roadNodeAmount, xMinMax, zMinMax, visibleMap);

		SimulationGrid grid = Instantiate(gridPrefab) as SimulationGrid;
		grid.showSplattedDensity = showSplattedDensity;
		grid.showSplattedVelocity = showSplattedVelocity;
		grid.cellSize = cellSize <= 0 ? 1 : cellSize;
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
			if (groupAgentPrefabs != null)
			{
				roadmap.spawns[i].spawner.InitializeSpawner(ref groupAgentPrefabs, ref roadmap, ref agentList, xMinMax, zMinMax, agentAvoidanceRadius);
			}
			else
			{
				roadmap.spawns[i].spawner.InitializeSpawner(ref roadmap, ref agentList, xMinMax, zMinMax, agentAvoidanceRadius);
			}
		}

		if(customTimeStep)
		{
			Physics.simulationMode = SimulationMode.Script;
			Debug.Log("Simulation mode set to Script");
		}

		experimentHUD = FindObjectOfType<ExperimentHUD>();
		if (experimentHUD == null)
		{
			Debug.LogWarning("ExperimentHUD not found in scene");
		}

		simulationGrid = SimulationGrid.instance;
		if (simulationGrid != null)
		{
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
	}
	

	/**
	 * Main simulation loop which is called every frame
	**/
	void Update () {

		if(!simulationStarted)
		{
			StartSimulation();
			return;
		}

		if (simulationGrid == null)
		{
			simulationGrid = SimulationGrid.instance;
			if (simulationGrid == null) return;
		}

		// Cap dt at 0.05f (20fps equivalent) to prevent physics explosions and LCP solver breaking if a lag spike occurs
		float dtRaw = customTimeStep ? timeStep : Time.deltaTime;
		simulationGrid.dt = Mathf.Min(dtRaw, 0.05f);
		GridParallelBridge.Instance.BatchAndRunFrameRaycasts(agentList, roadmap, simulationGrid);

		// Update grid with new density and velocity values
		simulationGrid.updateCellDensity ();
		simulationGrid.updateVelocityNodes ();

		GridParallelBridge.Instance.CopyManagedGridToNative(simulationGrid.density, simulationGrid.nCellsX, simulationGrid.nCellsZ);
		GridParallelBridge.Instance.CalculateAgentDensitiesInParallel(agentList, simulationGrid);


		//Solve linear constraint problem
		simulationGrid.PsolveRenormPsolve ();
		//Move agents
		for (int i = agentList.Count - 1; i >= 0; i--)
		{
			Agent agent = agentList[i];

			agent.CheckPositionAndRotation();

			// remove agent if it is outside the bounds of the plane
			float margin = 5f;
			if (agent.tr.position.x < xMinMax.x - margin || agent.tr.position.x > xMinMax.y + margin ||
				agent.tr.position.z < zMinMax.x - margin || agent.tr.position.z > zMinMax.y + margin ||
				agent.tr.position.y > 0.5f)
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
		//simulationGrid.collisionHandling(agentList);

		GridParallelBridge.Instance.RunParallelCollisionAvoidance(agentList, simulationGrid, xMinMax, zMinMax);


		if(customTimeStep)
		{
			Physics.Simulate(simulationGrid.dt);
		}

		if (experimentHUD != null)
		{
			experimentHUD.RegisterSimTick();
		}
	}

	private void StartSimulation()
	{
		simulationStartTimer -= Time.deltaTime;
		if (simulationStartTimer <= 0f)
		{
			simulationStarted = true;
			if (experimentHUD != null)
			{
				experimentHUD.realTimeStart = Time.realtimeSinceStartup;
			}
		}
	}

	public void AddToAgentList(Agent agent)
	{
		agentList.Add(agent);
	}
}
