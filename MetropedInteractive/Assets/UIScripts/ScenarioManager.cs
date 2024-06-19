using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using GLTFast.Addons;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Threading;

public class ScenarioPicker : MonoBehaviour
{
    public Slider ExposureSlider;
    public GameObject Pillars;
    public GameObject Walls;
    public GameObject GlassWalls;
    public GameObject Ads;
    public GameObject Benches;
    public GameObject Bins;
    public GameObject VendingMachines;
    public GameObject Crowd;
    public GameObject Trains;
    private bool[] scenariosPicked = new bool[10];
    System.Random random = new System.Random();
    private GameObject[] scenarioObjects;
    public GameObject RatingMenu;
    public GameObject FreeMenu;

    /*
    The preset scenarios are saved in the map below. 
    They are saved as follows:
    {ScenarioId, Lighting, Pillars, Walls, GlassWalls, Ads, Benches, Bins, VendingMachines, Crowd, Trains}
    The lighting has 3 possible values:
        1 - Bright
        2 - Optima
        3 - Dark
    The other elements have 2 possible values:
        1 - On
        0 - Off
    */
    public int[,] presetScenarios = new int[,]          
    {
        {1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1}, //Scenario 1
        {2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0}, //Scenario 2
        {3, 2, 1, 1, 1, 0, 0, 0, 0, 0, 0}, //Scenario 3
        {4, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0}, //Scenario 4
        {5, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}, //Scenario 5
        {6, 2, 1, 1, 0, 1, 1, 1, 1, 1, 1}, //Scenario 6
        {7, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1}, //Scenario 7
        {8, 2, 1, 1, 0, 0, 0, 0, 0, 0, 0}, //Scenario 8
        {9, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0}, //Scenario 9
        {10, 2, 0, 0, 0, 1, 1, 0, 0, 0, 0} //Scenario 10
    };


    void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            scenariosPicked[i] = false;
        }
        scenarioObjects = new GameObject[]
        {
            Pillars, Walls, GlassWalls, Ads, Benches, Bins, VendingMachines, Crowd, Trains
        };
    }
    public bool pickRandScenario()
    {
        int[] falseIndices = scenariosPicked.Select((value, index) => new { value, index })
                                            .Where(x => !x.value)
                                            .Select(x => x.index)
                                            .ToArray();
        
        if(falseIndices.Length == 0)
        {
            Debug.Log("All Scenarios have been picked.");
            if (FreeMenu != null)
            {
                FreeMenu.SetActive(true);
            }
            return false;
        }
        int randomFalseIdx = falseIndices[random.Next(falseIndices.Length)];
        Debug.Log("Random False Index: " + randomFalseIdx);
        scenariosPicked[randomFalseIdx] = true;
        pickSpecificScenario(randomFalseIdx + 1);
        return true;
    }

    public void pickSpecificScenario(int scenarioId)
    {
        int scenarioIdx = scenarioId - 1;
        if (scenarioIdx < 0 || scenarioIdx >= scenariosPicked.Length)
        {
            Debug.LogError("Invalid scenario index " + scenarioIdx);
            return;
        }

        for (int i = 0; i < scenarioObjects.Length; i++)
        {
            int state = presetScenarios[scenarioIdx, i + 2]; //ignores the ScenarioId and the Lighting
            ToggleObject(scenarioObjects[i], state);
        }
        Debug.Log("Set Scenario Id: " + scenarioId);
    }

    private void ToggleObject(GameObject obj, int state)
    {
        if (obj !=null)
        {
            obj.SetActive(state == 1);
        }
    }

    public void startScenario()
    {
        if (FreeMenu != null)
        {
            FreeMenu.SetActive(false);
        }
        if(!pickRandScenario())
        {
            pickSpecificScenario(1);
            return;
        }
        StartCoroutine(TimerCoroutine());
        
    }

    private IEnumerator TimerCoroutine()
    {
        yield return new WaitForSeconds(10);
        if (RatingMenu != null)
        {
            RatingMenu.SetActive(true);
            GameManager.Instance.SetMovementPause(true);
        }
    }
}
