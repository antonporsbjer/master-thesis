using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NewSpawner : MonoBehaviour {

	public enum Method {
		uniformSpawn,
		circleSpawn,
		discSpawn,
		continuousSpawn,
		areaSpawn
	}

	public Method spawnMethod = Method.continuousSpawn;
	public int numberOfAgents = 10;

	// Continuous spawn & Grouping
	public bool useGroupedAgents = false;
	public float individualAgents = 0.5f;
	public float percentOfTwoInGroup = 0.3f;
	public float percentOfThreeInGroup = 0.15f;
	public float percentOfFourInGroup = 0.05f;
	public bool useSimpleAgents = false;

	// Circle and disc spawn
	public float circleRadius = 5f;
	public int numberOfDiscRows = 3;

	// Area spawn
	public int rows = 5;
	public int rowLength = 5;

	// Common items
	internal int node;	// The node for this spawner
	protected Main mainScript;

	internal List<Agent> agentList; // Reference to global agentlist
	internal MapGen.map map; // Map of available spawns / goals
	Vector2 X, Z; // Information about plane sizes
	internal float agentAvoidanceRadius;

	public GameObject subgroupModels = null;
	internal List<int> subgroupModelsParentIndex;
	internal int subgroupTag = 0;

	public GameObject agentEditorContainer = null;
	public CustomNode customGoal = null;
	internal int goal;

	public float spawnRate = 1.0f;
	public bool usePoisson = false;
    public GameObject agentPrefab;

	// Set the node index for this spawner's node
	public void SetNode(int node)
	{
		this.node = node;
	}

	/**
	* Set the goal for the agents of this spawner.
	* If there is no custom goal set in the editor, the goal will be the goal node with index 0.
	*/
	private void SetGoal()
	{
		if (map.goals != null && map.goals.Count > 0)
		{
			goal = map.goals[0];
		}
		if (customGoal != null && map.allNodes != null) {
			for(int i = 0; i < map.allNodes.Count; ++i) {
				if (map.allNodes[i].transform.position == customGoal.transform.position) {
					goal = i;
					break;
				}
			}
		}
	}

	public void InitializeSpawner(ref MapGen.map map, ref List<Agent> agentList, Vector2 X, Vector2 Z, float agentAvoidanceRadius) {
		this.map = map;
		this.X = X; this.Z = Z;
		this.agentAvoidanceRadius = agentAvoidanceRadius;
		this.agentList = agentList;
		SetupSubgroupIndices();
		SetGoal();
	}

	public void InitializeSpawner(ref GameObject subgroupModels, ref MapGen.map map, ref List<Agent> agentList, Vector2 X, Vector2 Z, float agentAvoidanceRadius) {
		this.subgroupModels = subgroupModels;
		this.map = map;
		this.X = X; this.Z = Z;
		this.agentAvoidanceRadius = agentAvoidanceRadius;
		this.agentList = agentList;
		SetupSubgroupIndices();
		SetGoal();
	}

	private void SetupSubgroupIndices()
	{
		if (subgroupModels != null) {
			subgroupModelsParentIndex = new List<int>();
			for (int i = 0; i < subgroupModels.transform.childCount; ++i) {
				string childTag = subgroupModels.transform.GetChild(i).tag;
				if (childTag == "female" || childTag == "male") {
					subgroupModelsParentIndex.Add(i);
				}
			}
		}
	}

	void Start()
	{
		mainScript = FindObjectOfType<Main>();

		switch (spawnMethod) {
		case Method.uniformSpawn:
			if (agentList != null)
				agentList.AddRange(spawnRandomAgents(numberOfAgents));
			break;
		case Method.areaSpawn:
			spawnAreaAgents(rows, rowLength, node);
			break;
		case Method.circleSpawn:
			if (agentList != null && mainScript != null)
				agentList.AddRange(circleSpawn(numberOfAgents, circleRadius, mainScript.planeSizeX, mainScript.planeSizeZ));
			break;
		case Method.discSpawn:
			if (agentList != null && mainScript != null)
				agentList.AddRange(discSpawn(mainScript.planeSizeX, mainScript.planeSizeZ, circleRadius, numberOfDiscRows));
			break;
		case Method.continuousSpawn:
			continousSpawn(); 
			break;
		default:
			if (agentList == null)
				agentList = new List<Agent>(); 
			break;
		}
	}

	// UNIFORM SPAWN
	public List<Agent> spawnRandomAgents(int numberOfAgents) {
		List<Agent> agents = new List<Agent>();
		if (map.allNodes == null || map.allNodes.Count == 0) return agents;

		SetGoal();

		for (int i = 0; i < numberOfAgents; ++i) {
			Vector3 pos = new Vector3(Random.Range(X.x, X.y), 10.0f, Random.Range(Z.x, Z.y));
			int maxTries = 50;
			int tries = 0;
			while (Physics.Raycast(pos, new Vector3(0.0f, -1.0f, 0.0f), 20f) && tries < maxTries) {
				pos.x = Random.Range(X.x, X.y);
				pos.z = Random.Range(Z.x, Z.y);
				tries++;
			}
			pos.y = 0.0f;

			Agent a = null;
			if (agentPrefab != null)
			{
				if (agentPrefab.transform.childCount > 0)
					a = Instantiate(agentPrefab.transform.GetChild(Random.Range(0, agentPrefab.transform.childCount)).GetComponent<Agent>());
				else
					a = Instantiate(agentPrefab.GetComponent<Agent>());
			}
			if (a == null) continue;

			a.transform.position = pos;
			float closest = -1;
			int start = -1;
			bool init = false;
			for (int j = 0; j < map.allNodes.Count; ++j) {
				if (!Physics.Raycast(pos, (map.allNodes[j].transform.position - pos).normalized, (map.allNodes[j].transform.position - pos).magnitude)) {
					if (map.allNodes[j].transform.position != transform.position && (!init || (map.allNodes[j].transform.position - pos).magnitude < closest)) {
						closest = (map.allNodes[j].transform.position - pos).magnitude;
						start = j;
						init = true;
					} 
				}
			}
			if (start < 0 || goal < 0) {
				Debug.LogWarning("Insufficient goal or start nodes for agent at " + a.transform.position);
				Destroy(a.gameObject);
				continue;
			}

			a.InitializeAgent(pos, start, goal, map);
			if (agentEditorContainer != null)
				a.transform.parent = agentEditorContainer.transform;

			agents.Add(a);
		}
		return agents;
	}

	// AREA SPAWN
	public void AreaSpawn()
	{
		spawnAreaAgents(rows, rowLength, node);
	}

	void spawnAreaAgents(int rows, int rowLength, int startNode) {
		Vector3 startPos = transform.position - (transform.right * rowLength / 2);
		
		for (int i = 0; i < rows; ++i) {
			for (int j = 0; j < rowLength; ++j) {
				Vector3 posVector = startPos + (transform.right * j) + (transform.right * i);
				posVector.x += 1.5f * j; posVector.z += 1.5f * i; posVector.y = 0.0f;
				spawnOneAgent(posVector);
			}
		}
	}

	// CIRCLE SPAWN
	internal List<Agent> circleSpawn(int numberOfAgents, float r, float planeScaleX, float planeScaleZ){
		Vector3 agentPos = new Vector3(0f, 0f, 0f);
		float planeScale = Mathf.Min(planeScaleX, planeScaleZ);
		if (r > planeScale * 5 - agentAvoidanceRadius) 
		{
			r = planeScale * 5 - agentAvoidanceRadius;
		}

		agentPos.Set(r, 0.5f, 0f);
		float phi = 360f / (float)numberOfAgents;
		List<Agent> li = new List<Agent>();
		for (int n = 0; n < numberOfAgents; n++) {
			Agent a = null;
			if (agentPrefab != null)
			{
				a = Instantiate(agentPrefab.GetComponent<Agent>());
			}
			if (a == null) continue;

			a.transform.position = agentPos;
			a.transform.RotateAround(new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f), n * phi);
			a.noMap = true;
			a.noMapGoal = new Vector3(-a.transform.position.x, a.transform.position.y, -a.transform.position.z);
			li.Add(a);
		}
		return li;
	}

	// DISC SPAWN
	internal List<Agent> discSpawn(float planeScaleX, float planeScaleZ, float startRadius, int numberOfRows) {
		float r;
		int numberOfAgentsInRing;
		float d = 0.4f + agentAvoidanceRadius * 2f;
		List<Agent> li = new List<Agent>();
		for (int n = 0; n < numberOfRows; n++) {
			r = startRadius + n * agentAvoidanceRadius * 2f;
			numberOfAgentsInRing = (int)((2 * Mathf.PI * r) / d);
			if (numberOfAgentsInRing > 0)
			{
				li.AddRange(circleSpawn(numberOfAgentsInRing, r, planeScaleX, planeScaleZ));
			}
		}
		return li;
	}

	// CONTINUOUS SPAWN
	public void continousSpawn() {
		StartCoroutine(spawnContinously(spawnRate));
	}

	internal IEnumerator spawnContinously(float continousSpawnRate) {
		Transform spawnerNode = transform.childCount > 0 ? transform.GetChild(0) : transform;
		
		float timeBetweenSpawn = usePoisson ? CalculateTimeBetweenSpawns() : (continousSpawnRate > 0f ? 1f / continousSpawnRate : float.MaxValue);
		
		float waitTimer = timeBetweenSpawn;
		while (waitTimer > 0) {
			waitTimer -= SimulationGrid.instance != null ? SimulationGrid.instance.dt : Time.deltaTime;
			yield return null;
		}
		
		if (agentList != null && mainScript != null && agentList.Count < mainScript.maxNumberOfAgents) 
		{
			Vector3 startPos = new Vector3(Random.Range(-1.0f, 1.0f), 0f, Random.Range(-1.0f, 1.0f)); 
			startPos = spawnerNode.TransformPoint(startPos);

			float randomRange = Random.Range(0.0f, 1.0f);
			if (!useGroupedAgents || randomRange < individualAgents) {
				spawnOneAgent(startPos);
			} else {
				int groupSize;
				if (randomRange - individualAgents < percentOfTwoInGroup) {
					groupSize = 2;
				} else if (randomRange - individualAgents - percentOfTwoInGroup < percentOfThreeInGroup) {
					groupSize = 3;
				} else {
					groupSize = 4;
				}
				List<SubgroupAgent> liA = InitGroupAgent(groupSize, startPos, node, goal);
				for (int i = 0; i < liA.Count; ++i) {
					agentList.Add((Agent)liA[i]);
				}
			}
		}
		
		StartCoroutine(spawnContinously(continousSpawnRate));
	}

	// BURST SPAWN
	public IEnumerator BurstSpawn(int nAgents, float burstRate)
	{
		for (int i = 0; i < nAgents; ++i) {
			Vector3 startPos = new Vector3(transform.position.x + Random.Range(-1.5f, 1.5f), transform.position.y, transform.position.z + Random.Range(-1.5f, 1.5f));
			spawnOneAgent(startPos);
			yield return new WaitForSeconds(burstRate);
		}
	}

	public void spawnOneAgent(Vector3 startPosition)
	{
		Agent agent = null;
		if (agentPrefab != null) 
		{
			if (agentPrefab.transform.childCount > 0)
			{
				agent = Instantiate(agentPrefab.transform.GetChild(Random.Range(0, agentPrefab.transform.childCount)).GetComponent<Agent>());
			}
			else
			{
				agent = Instantiate(agentPrefab.GetComponent<Agent>());
			}
		}

		if (agent == null)
		{
			Debug.LogWarning("Could not instantiate agent at " + startPosition + " - missing agentPrefab");
			return;
		}

		agent.InitializeAgent(startPosition, node, goal, map);

		if (agentEditorContainer != null)
			agent.transform.parent = agentEditorContainer.transform;

		if (agentList != null)
			agentList.Add(agent);
	}

	// GROUPS
	private SubgroupAgent getGroupModel(bool fixedParent, bool leader) {
		SubgroupAgent model = null;
		if (subgroupModels != null && subgroupModels.transform.childCount > 0) {
			if (subgroupModelsParentIndex != null && subgroupModelsParentIndex.Count > 0 && fixedParent && leader) {
				model = subgroupModels.transform.GetChild(subgroupModelsParentIndex[Random.Range(0, subgroupModelsParentIndex.Count)]).GetComponent<SubgroupAgent>();
			} else {
				model = subgroupModels.transform.GetChild(Random.Range(0, subgroupModels.transform.childCount)).GetComponent<SubgroupAgent>();
			}
		}
		else if (agentPrefab != null) {
			model = agentPrefab.GetComponent<SubgroupAgent>();
		}
		return model;
	}

	private List<SubgroupAgent> InitGroupAgent(int groupSize, Vector3 pos, int start, int goal) {
		bool fixedParent = true;
		List<SubgroupAgent> gr = new List<SubgroupAgent>();

		SubgroupAgent groupModel = getGroupModel(fixedParent, true);
		if (groupModel == null)
		{
			Debug.LogWarning("SubgroupAgent model not found. Spawning single agent instead.");
			spawnOneAgent(pos);
			return gr;
		}

		SubgroupAgent leader = Instantiate(groupModel);
		leader.isLeader = true; 
		leader.transform.position = pos;
		List<Vector3> followerPositions = new List<Vector3>(3); 
		followerPositions.Add(pos);
		float usedValue = agentAvoidanceRadius > 0f ? agentAvoidanceRadius * 2f : 0.6f;
		followerPositions.Add(leader.transform.TransformPoint(0.0f, 0.0f, usedValue));
		followerPositions.Add(leader.transform.TransformPoint(0.0f, 0.0f, -usedValue));	
		followerPositions.Add(leader.transform.TransformPoint(0.0f, 0.0f, 2 * usedValue));
		followerPositions.Add(leader.transform.TransformPoint(0.0f, 0.0f, -2 * usedValue));
		gr.Add(leader);

		for (int i = 0; i < groupSize - 1; ++i) {
			SubgroupAgent followerModel = getGroupModel(fixedParent, false);
			if (followerModel != null)
			{
				SubgroupAgent follower = Instantiate(followerModel);
				gr.Add(follower);
			}
		}

		SubgroupAgent.companions comp = new SubgroupAgent.companions(gr, 0, transform.gameObject.name + subgroupTag.ToString());
		subgroupTag++;
		for (int i = 0; i < gr.Count; ++i) {
			gr[i].groupMemberNumber = i;
			gr[i].number = i;
			gr[i].c = comp;
			Agent sa = gr[i];
			sa.InitializeAgent(followerPositions[i], start, goal, map);
			if (agentEditorContainer != null)
				sa.transform.parent = agentEditorContainer.transform;
		}
		return gr;
	}

	float CalculateTimeBetweenSpawns()
    {
        float u = Random.value;
        // -ln(1-u)/λ
        return -Mathf.Log(1 - u) / (spawnRate > 0f ? spawnRate : 1f);
    }
}
