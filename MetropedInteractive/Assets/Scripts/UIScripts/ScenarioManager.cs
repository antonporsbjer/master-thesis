using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using GLTFast.Addons;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Threading;
using UnityEngine.XR.Interaction.Toolkit;

public class ScenarioPicker : MonoBehaviour
{
    public Slider ExposureSlider;
    public GameObject CrowdAgents;
    public GameObject Pillars;
    public GameObject Walls;
    public GameObject GlassWalls;
    public GameObject Ads;
    public GameObject Benches;
    public GameObject Bins;
    public GameObject VendingMachines;
    public GameObject Trains;
    private bool[] scenariosPicked = new bool[10];
    System.Random random = new System.Random();
    private GameObject[] scenarioObjects;
    public GameObject RatingMenu;
    public GameObject FreeMenu;
    public CrowdToggle crowdToggle;
    public GameObject player;
    public Transform XRrig;
    public LocomotionSystem locomotionSystem;
    private ChangeExposure changeExposure;
    public Slider presetRatingSlider;
    public int presetScenarioId;
    
    public TeleportCoordinates teleportCoordinates = new TeleportCoordinates
    {
        position = new Vector3(-42.745f, 0.628f, -2.7f),
        rotation = new Vector3(0f, 90f, 0f)
    };
    private TeleportCoordinates VRteleportCoordinates = new TeleportCoordinates
    {
        position = new Vector3(-40.53f, 0f, -3.465f),
        rotation = new Vector3(0f, 90f, 0f)
    };

    /*
    The preset scenarios are saved in the map below. 
    They are saved as follows:
    {ScenarioId, Lighting, Crowd, Pillars, Walls, GlassWalls, Ads, Benches, Bins, VendingMachines, Trains}
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
        {3, 2, 0, 1, 1, 1, 0, 0, 0, 0, 0}, //Scenario 3
        {4, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0}, //Scenario 4
        {5, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}, //Scenario 5
        {6, 2, 1, 1, 1, 0, 1, 1, 1, 1, 1}, //Scenario 6
        {7, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1}, //Scenario 7
        {8, 2, 0, 1, 1, 0, 0, 0, 0, 0, 0}, //Scenario 8
        {9, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0}, //Scenario 9
        {10, 2, 0, 0, 0, 0, 1, 1, 0, 0, 0} //Scenario 10
    };


    void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            scenariosPicked[i] = false;
        }
        scenarioObjects = new GameObject[]
        {
            Pillars, Walls, GlassWalls, Ads, Benches, Bins, VendingMachines, Trains
        };
        changeExposure = FindObjectOfType<ChangeExposure>();

        if (changeExposure == null)
        {
            Debug.LogError("ChangeExposure component not found in scene.");
        }
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
        presetScenarioId = scenarioId;
        if (scenarioIdx < 0 || scenarioIdx >= scenariosPicked.Length)
        {
            Debug.LogError("Invalid scenario index " + scenarioIdx);
            return;
        }

        int exposureSetting = presetScenarios[scenarioIdx, 1];

        if (changeExposure != null)
        {
            if (exposureSetting == 1)
            {
                Debug.Log("Exposure set by index to: 2");
                changeExposure.SetExposureByIndex(2);
            }
            if (exposureSetting == 2)
            {
                Debug.Log("Exposure set by index to: 1");
                changeExposure.SetExposureByIndex(1);

            }
            if(exposureSetting == 3)
            {
                Debug.Log("Exposure set by index to: 0");
                changeExposure.SetExposureByIndex(0);
            }
        }

        if(presetScenarios[scenarioIdx, 2] == 0)
        {
            if (CrowdAgents.activeSelf)
            {
                if(crowdToggle != null)
                {
                    crowdToggle.ToggleElement();
                }
            }
        }
        else
        {
            if (!CrowdAgents.activeSelf)
            {
                if (crowdToggle != null)
                {
                    crowdToggle.ToggleElement();
                }
            }
        }

        for (int i = 0; i < scenarioObjects.Length; i++)
        {
            int state = presetScenarios[scenarioIdx, i + 3]; //ignores the ScenarioId, the Lighting, and the Crowd
            ToggleObject(scenarioObjects[i], state);
        }
        Debug.Log("Set Scenario Id: " + scenarioId);
    }
    
    public int GetScenarioId()
    {
        return presetScenarioId;
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
            pickSpecificScenario(1);    //For when we start the Freeplay
            return;
        }
        StartCoroutine(TimerCoroutine());
        
    }

    public void TeleportPlayer()
    {
        if (player != null && teleportCoordinates != null)
        {
            player.transform.position = teleportCoordinates.position;
            player.transform.rotation = Quaternion.Euler(teleportCoordinates.rotation);
            
            LookControl lookControl = FindObjectOfType<LookControl>();
            if (lookControl != null)
            {
                lookControl.SetInitialRotation();
            }
        }
        StartCoroutine(TeleportVRCoroutine());
    }

    private IEnumerator TeleportVRCoroutine()
    {
        if (XRrig != null && VRteleportCoordinates != null)
        {
            if (locomotionSystem != null)
            {
                locomotionSystem.enabled = false;
                Debug.Log("Locomotion disabled");
            }

            yield return new WaitForEndOfFrame();

            XRrig.transform.position = VRteleportCoordinates.position;
            XRrig.transform.rotation = Quaternion.Euler(VRteleportCoordinates.rotation);
            Debug.Log("updated VR coordinates: " + VRteleportCoordinates.position);
            
            yield return new WaitForEndOfFrame();

            if (locomotionSystem != null)
            {
                locomotionSystem.enabled = true;
            }
        }
    }



    private IEnumerator TimerCoroutine()
    {
        yield return new WaitForSeconds(10);
        if (RatingMenu != null)
        {
            RatingMenu.SetActive(true);
            presetRatingSlider.value = 0;
            GameManager.Instance.SetMovementPause(true);
        }
    }
}
