using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WaitingAreaController : MonoBehaviour
{  
    public List<WaitingArea> waitingAreas;
    private List<Agent> waitingAgents;
    private GameObject waitingAgentsContainer;
    public Dictionary<int, List<int>> spawnerWaitingAreaDistances;

    public void Initialize()
    {
        waitingAreas = new List<WaitingArea>();
        waitingAgents = new List<Agent>();
        waitingAgentsContainer = GameObject.Find("Waiting Agents");

        foreach(WaitingArea waitingArea in FindObjectsOfType<WaitingArea>())
        {
            waitingAreas.Add(waitingArea);
        }

        BuildSpawnerWaitingAreaDistances();
    }

    public void addAgentToWaitingList(Agent agent)
    {
        waitingAgents.Add(agent);
    }

    public (int,Vector3) getWaitingSpotAndGatewayNode(int startNode)
    {
        return (0, new Vector3(0, 0, 0));
    }

    void BuildSpawnerWaitingAreaDistances()
    {
        spawnerWaitingAreaDistances = new Dictionary<int, List<int>>();

        MapGen.map roadmap = FindObjectOfType<MapGen>().getRoadmap();

        foreach(MapGen.spawnNode spawner in roadmap.spawns)
        {
            List<(int index,float distance)> distances = new List<(int, float)>();
            for (int areaIndex = 0; areaIndex < waitingAreas.Count; areaIndex++)
            {
                float distance = Vector3.Distance(roadmap.allNodes[spawner.node].transform.position, waitingAreas[areaIndex].transform.position);
                distances.Add((areaIndex, distance));
            }

            List<int> sortedWaitingAreaIndexes = distances.OrderBy(pair => pair.distance)
                                                            .Select(pair => pair.index)
                                                            .ToList();

            spawnerWaitingAreaDistances.Add(spawner.node, sortedWaitingAreaIndexes);
        }
    }

}
