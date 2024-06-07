using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsMovementPaused { get; private set; }

    public GameDataManager gameDataManager;

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
        GameData newGameData = new GameData(442387, 4);
        gameDataManager.SaveGameData(newGameData);
    }
}

