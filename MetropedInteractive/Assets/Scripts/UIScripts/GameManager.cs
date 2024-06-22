using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsMovementPaused { get; private set; }

    public GameDataManager gameDataManager;
    public GameObject scenarioMenu;
    public Slider ratingSliderFree; // Reference to the slider UI component
    public Slider ratingSliderPreset;
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
        int rating;
        if (scenarioMenu.activeSelf)
        {
            rating = (int)ratingSliderPreset.value; // Get the rating from the slider

        }
        else
        {
            rating = (int)ratingSliderFree.value; // Get the rating from the slider
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
            rating
            );

        gameDataManager.SaveGameData(newGameData);
    }
}
