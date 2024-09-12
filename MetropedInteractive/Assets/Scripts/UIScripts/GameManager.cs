using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsMovementPaused { get; private set; }

    public GameDataManager gameDataManager;
    public ScenarioPicker scenarioManager;
    public GameObject scenarioMenu;
    public Slider ratingSliderFree1; // Reference to the slider UI component
    public Slider ratingSliderFree2;
    public Slider ratingSliderFree3;
    public Slider ratingSliderPreset1;
    public Slider ratingSliderPreset2;
    public Slider ratingSliderPreset3;
    public Slider exposureSlider;
    public GameObject glassWalls;
    public GameObject walls;
    public GameObject advertisements;
    public GameObject crowd;
    public GameObject trains;
    public GameObject pillars;
    public GameObject vendingMachines;
    public GameObject trashCans;
    public GameObject benches;
    private int presetScenarioId;

    void Start()
    {
        gameDataManager = GetComponent<GameDataManager>();
        gameDataManager.readFile();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetMovementPause(bool pause)
    {
        IsMovementPaused = pause;
    }

    
    public void SaveGame()
    {
        string participantId = "00000"; // Example participant ID
        int rating1;
        int rating2;
        int rating3;
        if (scenarioMenu.activeSelf)
        {
            rating1 = (int)ratingSliderPreset1.value; // Get the rating from the slider
            rating2 = (int)ratingSliderPreset2.value;
            rating3 = (int)ratingSliderPreset3.value;
            if (scenarioManager != null)
            {
                presetScenarioId = scenarioManager.GetScenarioId();
            }
        }
        else
        {
            rating1 = (int)ratingSliderFree1.value; // Get the rating from the slider
            rating2 = (int)ratingSliderFree2.value;
            rating3 = (int)ratingSliderFree3.value;
            presetScenarioId = -1;
        }
        float exposure = (float)exposureSlider.value;
        bool glassWallsValue = (bool)glassWalls.activeSelf;
        bool wallsValue = (bool)walls.activeSelf;
        bool advertisementsValue = (bool)advertisements.activeSelf;
        bool crowdValue = (bool)crowd.activeSelf;
        bool trainsValue = (bool)trains.activeSelf;
        bool pillarsValue = (bool)pillars.activeSelf;
        bool vendingMachinesValue = (bool)vendingMachines.activeSelf;
        bool trashCanValue = (bool)trashCans.activeSelf&&pillars.activeSelf;
        bool benchesValue = (bool)benches.activeSelf;

        GameData newGameData = new GameData
            (
            participantId,
            presetScenarioId, 
            wallsValue, 
            glassWallsValue, 
            advertisementsValue, 
            crowdValue, 
            trainsValue,
            pillarsValue,
            vendingMachinesValue, 
            trashCanValue, 
            benchesValue, 
            exposure,
            rating1,
            rating2,
            rating3
            );

        gameDataManager.SaveGameData(newGameData);
    }
}
