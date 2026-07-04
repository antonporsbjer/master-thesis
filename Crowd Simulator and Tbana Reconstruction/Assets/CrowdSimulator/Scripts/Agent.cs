using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Agent : MonoBehaviour {
	public Vector3 preferredVelocity, continuumVelocity, collisionAvoidanceVelocity;
	public Vector3 velocity;
	public List<int> path;
	internal int pathIndex = 0;
	internal float agentRelXPos, agentRelZPos;
	internal float neighbourXWeight, neighbourZWeight, neighbourXZWeight, selfWeight;
	internal float selfRightVelocityWeight, selfLeftVelocityWeight, selfUpperVelocityWeight, selfLowerVelocityWeight, 
	neighbourRightVelocityWeight, neighbourLeftVelocityWeight, neighbourUpperVelocityWeight, neighbourLowerVelocityWeight;
	internal float densityAtAgentPosition;

	internal bool done = false;
	internal bool noMap = false;
	internal Vector3 noMapGoal;
	internal Animator animator;
	internal Rigidbody rbody;
	internal bool collision = false;
	internal int row,column;
	Vector3 prevPos;
	Vector3 previousDirection;
	public float walkingSpeed;
    public float maxWaitTime = 2f;
	public float currentSpeed;
	private SimulationGrid grid;

	internal Transform tr;
	private float cachedCellSize;
	private float cachedCellSizeSquared;

	private static int agentLayerMask = -1;
	internal float colliderRadius;
	internal Renderer agentRenderer;
	private Main mainScript;
	internal int goal;
	internal Vector3 targetPoint;
	internal bool hasCachedCanSeeNext_0 = false;
	internal bool cachedCanSeeNext_0 = false;
	internal bool hasCachedCanSeeNext_1 = false;
	internal bool cachedCanSeeNext_1 = false;

	void Awake()
	{
		tr = transform;
	}

	internal void CheckPositionAndRotation()
	{
		Vector3 pos = tr.position;
    	Quaternion rot = tr.rotation;
    
		if (pos.y > 0.1f ||
			pos.y < -0.1f ||
			rot.x < -0.1 ||
			rot.x > 0.1 ||
			rot.z > 0.1 ||
			rot.z < -0.1)
			{
				Reset();
			}
	}

	
	internal void Start() {
		animator = tr.gameObject.GetComponent<Animator> ();
		rbody = tr.gameObject.GetComponent<Rigidbody> ();

		if (rbody != null)
		{
			rbody.isKinematic = false;
			rbody.useGravity = false;
		}
		else
		{
			Debug.LogError("No Rigidbody found!");
		}

		CapsuleCollider col = GetComponent<CapsuleCollider>();
		if (col == null)
		{
			Debug.LogError("No CapsuleCollider found!");
		}
		else
		{
			colliderRadius = col.radius;
		}

		//Which cell am i in currently?
		calculateRowAndColumn();
		if (!grid.colHandler && rbody != null) {
			Destroy (rbody);
		}

		mainScript = Main.instance;
		if(this is SubgroupAgent)
		{
			walkingSpeed = mainScript.agentMaxSpeed;
		}
		else
		{
			walkingSpeed = Random.Range(mainScript.agentMinSpeed, mainScript.agentMaxSpeed);
		}
		
	}

	public void InitializeAgent(Vector3 pos, int start, int goal, MapGen.map map)
	{
		tr.position = pos;
		this.goal = goal;
		path = map.shortestPaths[start][goal];

		pathIndex = 1;
		targetPoint = map.allNodes[path[pathIndex]].getTargetPoint(pos, gameObject.GetInstanceID());
		preferredVelocity = (targetPoint - pos).normalized;
		agentRenderer = GetComponentInChildren<Renderer>();

		grid = SimulationGrid.instance;

		cachedCellSize = SimulationGrid.instance.cellSize;
    	cachedCellSizeSquared = cachedCellSize * cachedCellSize;
	}

	public void ApplyMaterials(Material materialColor, ref Dictionary<string, int> skins, Material argMat = null)
	{
		if (tag == "original") {
			if (tr.childCount > 1) {
				//tr.GetChild(1).GetComponent<SkinnedMeshRenderer> ().sharedMaterial = materialColor;
			}
		} else if (tr.childCount > 0) {
			Renderer ss = tr.GetChild (0).GetComponent<Renderer> ();
			if (ss != null)
				ss.material.mainTexture = (Texture)Resources.Load (tag + "-" + Random.Range (1, skins [tag]+1));
			else {
				Renderer ss2 = tr.GetChild (1).GetComponent<Renderer> ();
				if (ss2 != null)
					ss2.material.mainTexture = (Texture)Resources.Load (tag + "-" + Random.Range (1, skins [tag]+1));
			}
		}
	}

	internal void calculateRowAndColumn()
	{
		Vector3 pos = tr.position;
		row = (int)((pos.z - Main.zMinMax.x) / grid.cellSize);
		column = (int)((pos.x - Main.xMinMax.x) / grid.cellSize);

		row = Mathf.Clamp(row, 0, grid.nCellsZ - 1);
    	column = Mathf.Clamp(column, 0, grid.nCellsX - 1);

		Vector3 cellCenter = grid.cellCenters[row, column];
		agentRelXPos = pos.x - cellCenter.x;
		agentRelZPos = pos.z - cellCenter.z;
	}

	/**
	 * Calculate the actual velocity of this agent, based on continuum, preferred and collision avoidance velocities
	 **/ 
	internal void setCorrectedVelocity()
	{
		calculateDensityAtPosition();
		calculateContinuumVelocity();
		//-1 since we subtract this agents density at position

		velocity = preferredVelocity + (densityAtAgentPosition - 1 / Mathf.Pow(grid.cellSize, 2)) / SimulationGrid.maxDensity
		* (continuumVelocity - preferredVelocity);
		velocity.y = 0f;
		if (velocity != Vector3.zero)
		{
			tr.forward = velocity.normalized;
		}
		velocity = velocity + collisionAvoidanceVelocity;
	}

	internal bool canSeeNext(MapGen.map map, int modifier)
	{
		if (modifier == 0 && hasCachedCanSeeNext_0)
		{
			return cachedCanSeeNext_0;
		}
		if (modifier == 1 && hasCachedCanSeeNext_1)
		{
			return cachedCanSeeNext_1;
		}
		
		if (pathIndex + modifier < path.Count && pathIndex + modifier >= 0 && pathIndex + modifier < map.allNodes.Count)
		{
			//Can we see next goal?
			Vector3 pos = tr.position;
			Vector3 next = map.allNodes[path[pathIndex + modifier]].getTargetPoint(pos, gameObject.GetInstanceID());
			Vector3 targetPosition = pos - tr.forward * colliderRadius;
			Vector3 dir = next - targetPosition;
			Vector3 endPosition = targetPosition + (dir.normalized * dir.magnitude);
			if (agentLayerMask == -1)
			{
				agentLayerMask = ~LayerMask.GetMask("Agent");
			}
			if (!Physics.Raycast(targetPosition, dir.normalized, out RaycastHit hit, dir.magnitude, agentLayerMask))
			{
				Debug.DrawLine(targetPosition, endPosition, Color.green);
				return true;
			}
			else
			{
				Debug.DrawLine(targetPosition, hit.point, Color.red);
			}
		}
		return false;
	}
	/**
	 * Calculate the preferred velocity by looking at desired path
	 **/ 
	internal void calculatePreferredVelocityMap(MapGen.map map)
	{
		bool change = false;
		previousDirection = preferredVelocity.normalized;
		Vector3 pos = tr.position;

		if (map.allNodes[path[pathIndex]].IsAgentInsideArea(pos) || (grid.skipNodeIfSeeNext && canSeeNext(map, 1)))
		{
			//New node reached
			collision = false;
			pathIndex += 1;
			if (pathIndex >= path.Count)
			{
				//Done
				done = true;
			}
			else
			{
				targetPoint = map.allNodes[path[pathIndex]].getTargetPoint(pos, gameObject.GetInstanceID());
				Vector3 nextDirection = (targetPoint - pos).normalized;
				if (Vector3.Angle(previousDirection, nextDirection) > 20.0f && grid.smoothTurns)
				{
					preferredVelocity = Vector3.RotateTowards(velocity.normalized, nextDirection, grid.dt * ((35.0f - 400 * grid.dt) * Mathf.PI / 180.0f), 15.0f).normalized;
					change = true;
				}
			}
		}
		else if (pathIndex > 0 && grid.walkBack && !canSeeNext(map, 0))
		{ //Can we see current heading? Are we trapped?
		  //No. We want to go back
			preferredVelocity = (map.allNodes[path[pathIndex - 1]].getTargetPoint(pos, gameObject.GetInstanceID()) - pos).normalized;
			change = false;
		}
		else
		{
			collision = false;
			Vector3 nextDirection = (targetPoint - pos).normalized;
			if (change && Vector3.Angle(previousDirection, nextDirection) > 20.0f && grid.smoothTurns)
			{
				preferredVelocity = Vector3.RotateTowards(velocity.normalized, nextDirection, grid.dt * ((35.0f - 400 * grid.dt) * Mathf.PI / 180.0f), 15.0f).normalized;
			}
			else
			{
				change = false;
				preferredVelocity = (targetPoint - pos).normalized;
			}
		}
		preferredVelocity = preferredVelocity * walkingSpeed;
		preferredVelocity.y = 0f;
	}

	/**
	 * Calculate the preferred velocity of a single uncharted point as a goal 
	 **/
	internal void calculatePreferredVelocityNoMap()
	{
		Vector3 pos = tr.position;
		if ((pos - noMapGoal).magnitude < MapGen.DEFAULT_THRESHOLD)
		{
			//New node reached
			//Done
			done = true;
		}
		else
		{
			preferredVelocity = (noMapGoal - pos).normalized;
		}
		preferredVelocity = preferredVelocity * walkingSpeed;
		preferredVelocity.y = 0f;
	}

	internal virtual void calculatePreferredVelocity(MapGen.map map)
	{
		if (noMap)
		{
			calculatePreferredVelocityNoMap();
		}
		else
		{
			calculatePreferredVelocityMap(map);
		}
	}

	/**
	 * Change the position of the agent and reset variables. 
	 * Do animations.
	 **/
	internal void changePosition(MapGen.map map)
	{
		if (done)
		{
			return; // Don't do anything
		}

		calculatePreferredVelocity(map);
		setCorrectedVelocity();

		prevPos = tr.position;

		Vector3 newPosition = prevPos + velocity * grid.dt;
		newPosition.y = 0.0f;   // Lock Y position
		tr.position = newPosition;

		collisionAvoidanceVelocity = Vector3.zero;

		Animate(prevPos);
	}

	void Animate(Vector3 previousPosition)
	{
		float realSpeed = Vector3.Distance (tr.position, previousPosition) / Mathf.Max(grid.dt, grid.dt);
		if (animator != null) {
	
			if (realSpeed < 0.05f) {
				animator.speed = 0;
			} else {
				animator.speed = realSpeed / walkingSpeed;
			}
		}
	}

	/**
	 * Do a bilinear interpolation of surrounding densities and come up with a density at this agents position.
	 **/
	internal float calculateDensityAtPosition()
	{
		densityAtAgentPosition = 0.0f;
		int xNeighbour = column + (int)Mathf.Sign(neighbourXWeight);    //Column for the neighbour which the agent contributes to
		int zNeighbour = row + (int)Mathf.Sign(neighbourZWeight);       //Row for the neighbour which the agent contributes to

		densityAtAgentPosition += Mathf.Abs(selfWeight) * grid.density[row, column];

		if (xNeighbour >= 0 && xNeighbour < grid.nCellsX)
		{   //As long as the cell exists
			densityAtAgentPosition += Mathf.Abs(neighbourXWeight) * grid.density[row, xNeighbour];
		}

		if (zNeighbour >= 0 && zNeighbour < grid.nCellsZ)
		{           //As long as the cell exists
			densityAtAgentPosition += Mathf.Abs(neighbourZWeight) * grid.density[zNeighbour, column];
		}

		if (zNeighbour >= 0 && zNeighbour < grid.nCellsZ && xNeighbour >= 0 && xNeighbour < grid.nCellsX)
		{   //As long as the cell exists
			densityAtAgentPosition += Mathf.Abs(neighbourXZWeight) * grid.density[zNeighbour, xNeighbour];
		}
		return densityAtAgentPosition;
	}

	/**
	 * Calculate the continuum velocity caused by pressure from the grid
	 **/
	internal void calculateContinuumVelocity()
	{
		Vector3 tempContinuumVelocity = Vector3.zero;

		int xNeighbour = column + (int)Mathf.Sign(neighbourXWeight);    //Column for the neighbour which the agent contributes to
		int zNeighbour = row + (int)Mathf.Sign(neighbourZWeight);       //Row for the neighbour which the agent contributes to

		// Sides in current cell
		tempContinuumVelocity.x += selfLeftVelocityWeight * grid.cellMatrix[row, column].leftVelocityNode.velocity;
		tempContinuumVelocity.x += selfRightVelocityWeight * grid.cellMatrix[row, column].rightVelocityNode.velocity;
		tempContinuumVelocity.z += selfUpperVelocityWeight * grid.cellMatrix[row, column].upperVelocityNode.velocity;
		tempContinuumVelocity.z += selfLowerVelocityWeight * grid.cellMatrix[row, column].lowerVelocityNode.velocity;

		if (zNeighbour >= 0 && zNeighbour < grid.nCellsZ)
		{   //As long as the cell exists
			tempContinuumVelocity.x += neighbourLeftVelocityWeight * grid.cellMatrix[zNeighbour, column].leftVelocityNode.velocity;
			tempContinuumVelocity.x += neighbourRightVelocityWeight * grid.cellMatrix[zNeighbour, column].rightVelocityNode.velocity;
		}

		if (xNeighbour >= 0 && xNeighbour < grid.nCellsX)
		{           //As long as the cell exists
			tempContinuumVelocity.z += neighbourUpperVelocityWeight * grid.cellMatrix[row, xNeighbour].upperVelocityNode.velocity;
			tempContinuumVelocity.z += neighbourLowerVelocityWeight * grid.cellMatrix[row, xNeighbour].lowerVelocityNode.velocity;
		}

		if (float.IsNaN(tempContinuumVelocity.x)) tempContinuumVelocity.x = 0;
		if (float.IsNaN(tempContinuumVelocity.z)) tempContinuumVelocity.z = 0;

		continuumVelocity = tempContinuumVelocity;
	}

	/**
	 * Move command (and all it includes) for this agent.
	 * Recalculate weights and contributions to grid after update.
	 **/
	internal void move(MapGen.map map) {
		changePosition (map);
		calculateRowAndColumn ();
		setWeights ();
		grid.cellMatrix[row, column].addVelocity(this);
		grid.cellMatrix[row, column].addDensity (this);
	}


	/**
	 * Set weight contributions to current cell radius. (Inverse bilinear interpolation)
	 **/
	public void setWeights()
	{
		//An area the size of a cell is surrounded by each point.
		//AgentRelXPos: Side length of supposed area, outside current cell of agent - x direction
		//AgentRelZPos: Side length of supposed area, outside current cell of agent - z direction
		float sideOne = cachedCellSize  - Mathf.Abs(agentRelXPos); //Side length of supposed area of this agents position, x - direction
		float sideTwo = cachedCellSize  - Mathf.Abs(agentRelZPos); //Side length of supposed area of this agents position, z - direction

		// Weights on smaller areas inside and outside current cell
		//Area weight of neighboring cell in..
		neighbourXWeight = sideTwo * agentRelXPos / cachedCellSizeSquared; // x direction
		neighbourZWeight = sideOne * agentRelZPos / cachedCellSizeSquared; //z direction
		neighbourXZWeight = agentRelXPos * agentRelZPos / cachedCellSizeSquared; //both x and z direction (diagonal from this agent's cell)
																	 //Own cell weight
		selfWeight = sideOne * sideTwo / cachedCellSizeSquared;

		//Now checking velocityNodes contribution
		//Offsets from each velocity node's center (also seen as a cell on each node)
		float rightShiftedRelXPos = cachedCellSize / 2 + agentRelXPos;
		float leftShiftedRelXPos = cachedCellSize / 2 - agentRelXPos;
		float upperShiftedRelZPos = cachedCellSize / 2 + agentRelZPos;
		float lowerShiftedRelZPos = cachedCellSize / 2 - agentRelZPos;

		//Weight contributions to different velocityNodes (area / totalCellArea)
		selfRightVelocityWeight = rightShiftedRelXPos * sideTwo / cachedCellSizeSquared;
		selfLeftVelocityWeight = leftShiftedRelXPos * sideTwo / cachedCellSizeSquared;
		selfUpperVelocityWeight = upperShiftedRelZPos * sideOne / cachedCellSizeSquared;
		selfLowerVelocityWeight = lowerShiftedRelZPos * sideOne / cachedCellSizeSquared;

		neighbourRightVelocityWeight = rightShiftedRelXPos * Mathf.Abs(agentRelZPos) / cachedCellSizeSquared;
		neighbourLeftVelocityWeight = leftShiftedRelXPos * Mathf.Abs(agentRelZPos) / cachedCellSizeSquared;
		neighbourUpperVelocityWeight = upperShiftedRelZPos * Mathf.Abs(agentRelXPos) / cachedCellSizeSquared;
		neighbourLowerVelocityWeight = lowerShiftedRelZPos * Mathf.Abs(agentRelXPos) / cachedCellSizeSquared;
	}

	internal void Reset()
	{
		rbody.velocity = Vector3.zero;
		rbody.angularVelocity = Vector3.zero;
		velocity = Vector3.zero;
		preferredVelocity = Vector3.zero;
		continuumVelocity = Vector3.zero;
		collisionAvoidanceVelocity = Vector3.zero;
		Vector3 pos = tr.position;
		tr.position = new Vector3(pos.x, 0f, pos.z);
		tr.rotation = Quaternion.identity;
	}
}
