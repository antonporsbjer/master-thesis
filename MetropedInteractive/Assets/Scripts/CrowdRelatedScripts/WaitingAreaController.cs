using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingAreaController : MonoBehaviour
{  
    private List<WaitingArea> waitingAreas;
    private List<Agent> waitingAgents;
    private GameObject waitingAgentsContainer;

    void OnEnable()
    {
        waitingAreas = new List<WaitingArea>();
        waitingAgents = new List<Agent>();
        waitingAgentsContainer = GameObject.Find("Waiting Agents");

        foreach(WaitingArea waitingArea in FindObjectsOfType<WaitingArea>())
        {
            waitingAreas.Add(waitingArea);
        }
    }

    public void addAgentToWaitingList(Agent agent)
    {
        waitingAgents.Add(agent);
    }

    public (int,Vector3) getWaitingSpotAndGatewayNode(int startNode)
    {
        return (0, new Vector3(0, 0, 0));
    }

}
