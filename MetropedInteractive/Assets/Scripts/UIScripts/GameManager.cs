using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsMovementPaused { get; private set; }

    public GameDataManager gameDataManager;
    public Slider ratingSlider; // Reference to the slider UI component
    public Slider exposureSlider;
    public Toggle glassWalls;
    public Toggle walls;
    public Toggle advertisements;
    public Toggle crowd;
    public Toggle trains;
    public Toggle pillars;
    public Toggle vendingMachines;
    public Toggle signs;
    public Toggle benches;
    public Toggle fireboxes;

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
        int rating = (int)ratingSlider.value; // Get the rating from the slider
        float exposure = (float)exposureSlider.value;
        bool glassWallsValue = (bool)glassWalls.isOn;
        bool wallsValue = (bool)walls.isOn;
        bool advertisementsValue = (bool)advertisements.isOn;
        bool crowdValue = (bool)crowd.isOn;
        bool trainsValue = (bool)trains.isOn;
        bool pillarsValue = (bool)pillars.isOn;
        bool vendingMachinesValue = (bool)vendingMachines.isOn;
        bool signsValue = (bool)signs.isOn;
        bool benchesValue = (bool)benches.isOn;

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
            signsValue, 
            benchesValue, 
            exposure,
            rating
            );

        gameDataManager.SaveGameData(newGameData);
    }
}
