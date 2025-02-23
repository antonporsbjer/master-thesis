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
    private MapGen.map roadmap;

    public void Initialize()
    {
        waitingAreas = new List<WaitingArea>();
        waitingAgents = new List<Agent>();
        waitingAgentsContainer = GameObject.Find("Waiting Agents");

        foreach(WaitingArea waitingArea in FindObjectsOfType<WaitingArea>())
        {
            waitingAreas.Add(waitingArea);
        }

        roadmap = FindObjectOfType<MapGen>().getRoadmap();

        BuildSpawnerWaitingAreaDistances();
    }

    public void addAgentToWaitingList(Agent agent)
    {
        waitingAgents.Add(agent);
    }

    public (int,Vector3) getWaitingAreaSpot(int startNode)
    {
        int assignedWaitingArea;
        Vector3 assignedWaitingSpot;

        foreach (int waitingAreaIndex in spawnerWaitingAreaDistances[startNode])
        {
            (int waitingAreaIndex, Vector3? waitingAreaSpot) areaAndSpot = waitingAreas[waitingAreaIndex].getWaitingSpot();
            if(areaAndSpot.waitingAreaSpot.HasValue)
            {
                assignedWaitingSpot = areaAndSpot.waitingAreaSpot.Value;
                assignedWaitingArea = areaAndSpot.waitingAreaIndex;
                break;
            }
        }
        return (-1,Vector3.zero);
    }

    void BuildSpawnerWaitingAreaDistances()
    {
        spawnerWaitingAreaDistances = new Dictionary<int, List<int>>();

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
