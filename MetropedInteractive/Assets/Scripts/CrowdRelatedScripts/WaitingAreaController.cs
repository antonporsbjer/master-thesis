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

    void addAgentToWaitingList(Agent agent)
    {
        waitingAgents.Add(agent);
    }

}
