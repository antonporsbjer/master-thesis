using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RunManager : MonoBehaviour
{
    public int totalRuns = 10;
    public int agentsPerRun = 100;
    public float extraWaitAfterSpawnSeconds = 5f; // let agents act before saving
    public DataCollector dataCollector; // optional, will FindObjectOfType if null

    private static int currentRun = 0;
    private static RunManager instance;

    void Awake()
    {
        // singleton to persist across reloads
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
      if (dataCollector == null)
      {
        dataCollector = FindObjectOfType<DataCollector>();    
      }

      StartCoroutine(RunLoop());
    }

    IEnumerator RunLoop()
    {
        while (currentRun < totalRuns)
        {
            // wait for scene to be loaded and DataCollector to exist
            yield return new WaitUntil(() => (dataCollector != null) || (FindObjectOfType<DataCollector>() != null));
            if (dataCollector == null)
                dataCollector = FindObjectOfType<DataCollector>();

            // wait until the requested number of agents have been created/registered
            yield return new WaitUntil(() => dataCollector.dataRecord != null && dataCollector.dataRecord.agents.Count >= agentsPerRun);

            // allow some extra time for agents to behave (optional)
            if (extraWaitAfterSpawnSeconds > 0f)
                yield return new WaitForSeconds(extraWaitAfterSpawnSeconds);

            // save current run
            dataCollector.SaveRun(currentRun + 1);
            currentRun++;

            // reset data (so next run starts clean)
            dataCollector.ResetForNextRun();

            // reload scene to reset simulation (this will keep RunManager alive)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

            // after reload the coroutine will continue in the surviving RunManager instance
            yield return null;
        }

        Debug.Log($"RunManager: Completed {totalRuns} runs.");
    }
}