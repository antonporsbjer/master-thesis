using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {

	internal int node;	//The node for this spawner
	protected Main mainScript;

	internal MapGen.map map;
	Vector2 X, Z;

	public GameObject agentEditorContainer = null;
	public CustomNode customGoal = null;
	internal int goal;

	public float timeBetweenSpawns;
	public bool usePoisson = false;
    public Agent agentPrefab;
	private float nextSpawnTimer;
	private CustomNode spawnerNode;

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
		if(customGoal == null)
		{
			Debug.LogWarning("No custom goal set for spawner " + gameObject.name + ", using default goal with index 0.");
			goal = 0;
		}
		else
		{
			goal = customGoal.index;
		}
	}

	public void InitializeSpawner(MapGen.map map, Vector2 X, Vector2 Z) {
		this.map = map;
		this.X = X; this.Z = Z;
		SetGoal();
	}

	void Start()
	{
		mainScript = FindObjectOfType<Main>();
		if(mainScript == null)
		{
			Debug.LogError("Main script not found in the scene.");
			return;
		}

		spawnerNode = GetComponentInChildren<CustomNode>();

		nextSpawnTimer = 0f;
	}

	public void UpdateSpawner()
	{
		if(mainScript.agentList.Count >= mainScript.maxNumberOfAgents)
		{
			return;
		}

		nextSpawnTimer -= SimulationGrid.instance.dt;

		if(nextSpawnTimer <= 0)
		{
			Vector3 startPos = new Vector3 (Random.Range (-0.5f, 0.5f), 0f, Random.Range (-0.5f, 0.5f)); 
			startPos = spawnerNode.transform.TransformPoint (startPos);
			SpawnOneAgent(startPos);

			if(usePoisson)
			{
				nextSpawnTimer = CalculateTimeBetweenSpawns();
			}
			else
			{
				nextSpawnTimer = timeBetweenSpawns;
			}
		}
	}

	public void SpawnOneAgent(Vector3 startPosition)
	{
        Agent agent = Instantiate (agentPrefab);

		agent.InitializeAgent (startPosition, node, goal, map);

		if (agentEditorContainer != null)
			agent.tr.parent = agentEditorContainer.transform;

		mainScript.agentList.Add (agent);
	}

	float CalculateTimeBetweenSpawns()
    {
		float spawnRate = 1f / timeBetweenSpawns;
        float u = Random.value;
        // -ln(1-u)/λ
        return -Mathf.Log(1 - u) / spawnRate;
    }
}
