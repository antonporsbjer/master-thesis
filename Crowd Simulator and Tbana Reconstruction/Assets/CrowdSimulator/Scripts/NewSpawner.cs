using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NewSpawner : MonoBehaviour {

	internal int node;	//The node for this spawner
	protected Main mainScript;

	internal List<Agent> agentList; //Reference to global agentlist
	internal MapGen.map map; //map of available spawns / goals
	Vector2 X, Z; //Information about plane sizes
	internal float agentAvoidanceRadius;

	public GameObject agentEditorContainer = null;
	public CustomNode customGoal = null;
	internal int goal;

	public float spawnRate;
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
		goal = map.goals[0];
		if (customGoal != null) {
			//OPT: Use dictionary in mapgen to get constant time access!
			for(int i = 0; i < map.allNodes.Count; ++i) {
				if (map.allNodes [i].transform.position == customGoal.transform.position) {
					goal = i;
					break;
				}
			}
		}
	}

	public void InitializeSpawner(ref MapGen.map map,  ref List<Agent> agentList, Vector2 X, Vector2 Z, float agentAvoidanceRadius) {
		this.map = map;
		this.X = X; this.Z = Z;
		this.agentAvoidanceRadius = agentAvoidanceRadius;
		this.agentList = agentList;
		SetGoal();
	}

	void Start()
	{
		mainScript = FindObjectOfType<Main>();
        // Disable coroutine call
		// continousSpawn(); 
        
        // Initialize timer
        if (usePoisson) {
            currentSpawnWaitTime = CalculateTimeBetweenSpawns();
        } else {
            currentSpawnWaitTime = spawnRate;
        }
	}
    
    private float spawnTimer = 0f;
    private float currentSpawnWaitTime = 0f;

    void Update() {
        if (mainScript == null) return;
        
        // Scale deltaTime by simulationSpeed
        float dt = Time.deltaTime * mainScript.simulationSpeed;
        spawnTimer += dt;
        
        if (spawnTimer >= currentSpawnWaitTime) {
            spawnTimer -= currentSpawnWaitTime;
            
            if (agentList.Count < mainScript.maxNumberOfAgents) {
                Transform spawnerNode = transform.GetChild(0);
                Vector3 startPos = new Vector3 (Random.Range (-0.5f, 0.5f), 0f, Random.Range (-0.5f, 0.5f)); 
			    startPos = spawnerNode.TransformPoint (startPos);
                spawnOneAgent(startPos);
            }
            
            if (usePoisson) {
                currentSpawnWaitTime = CalculateTimeBetweenSpawns();
            } else {
                currentSpawnWaitTime = spawnRate;
            }
        }
    }

	// CONTINUOUS SPAWN (Legacy/Unused if we use Update)
	public void continousSpawn() {
		// StartCoroutine (spawnContinously(spawnRate));
	}

	internal IEnumerator spawnContinously(float continousSpawnRate) {
        // Deprecated by Update loop
        yield return null;
	}

	// BURST SPAWN (Keep as coroutine? Or refactor?)
    // Burst spawn seems rarely used or manual. If used, it should also speed up? 
    // For now, let's just leave BurstSpawn as is or warn user. 
    // User specifically asked about "spawn rate" which implies continuous spawning.
    // I will focus on continuous.
	public IEnumerator BurstSpawn(int nAgents, float burstRate)
	{
		for (int i = 0; i < nAgents; ++i) {
			Vector3 startPos = new Vector3(transform.position.x + Random.Range(-1.5f, 1.5f), transform.position.y, transform.position.z + Random.Range(-1.5f, 1.5f));
			spawnOneAgent (startPos);
            // This WaitForSeconds will NOT be affected by my custom simulationSpeed unless I change Time.timeScale.
            // If the user wants EVERYTHING fast, maybe I should just set Time.timeScale in Main?
            // But Main does multiple physics steps per frame. Time.timeScale doesn't do that automatically for custom physics loops like this one.
            // So manual scaling is correct.
            // I will update this to wait manually if possible, but IEnumerator makes it hard to loop with custom delta.
            // Leaving as is, assuming Continuous is the main concern.
			yield return new WaitForSeconds (burstRate);
		}

	}

	public void spawnOneAgent(Vector3 startPosition)
	{
        Agent agent;
		agent = Instantiate (agentPrefab.transform.GetChild(Random.Range(0, agentPrefab.transform.childCount)).GetComponent<Agent>());

		agent.InitializeAgent (startPosition, node, goal, ref map);

		if (agentEditorContainer != null)
			agent.transform.parent = agentEditorContainer.transform;

		agentList.Add (agent); 
	}

	float CalculateTimeBetweenSpawns()
    {
        float u = Random.value;
        // -ln(1-u)/λ
        return -Mathf.Log(1 - u) / spawnRate;
    }
}
