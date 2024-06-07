using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsMovementPaused { get; private set; }

    public GameDataManager gameDataManager;
    public Slider ratingSlider; // Reference to the slider UI component

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
        int playerId = 442387; // Example player ID
        int rating = (int)ratingSlider.value; // Get the rating from the slider
        GameData newGameData = new GameData(playerId, rating);
        gameDataManager.SaveGameData(newGameData);
    }
}
