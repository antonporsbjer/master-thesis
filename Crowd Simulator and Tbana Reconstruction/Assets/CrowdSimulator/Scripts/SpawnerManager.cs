using UnityEngine;
using System.Collections.Generic;

public class SpawnerManager : MonoBehaviour
{
    public static SpawnerManager instance;

    [Tooltip("Automatically finds all NewSpawner components in the scene on Start.")]
    public List<NewSpawner> allSpawners = new List<NewSpawner>();

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Find all spawners if not manually assigned
        if (allSpawners == null || allSpawners.Count == 0)
        {
            allSpawners = new List<NewSpawner>(FindObjectsOfType<NewSpawner>());
        }
    }

    /// <summary>
    /// Updates the spawn rate of all managed spawners.
    /// Note: This updates the spawnRate variable. If the spawner is currently yielding in a coroutine, 
    /// the new rate takes effect on the next iteration.
    /// </summary>
    /// <param name="newSpawnRate">The new spawn rate to set</param>
    public void SetGlobalSpawnRate(float newSpawnRate)
    {
        if (allSpawners == null || allSpawners.Count == 0)
        {
            allSpawners = new List<NewSpawner>(FindObjectsOfType<NewSpawner>());
        }

        foreach (NewSpawner spawner in allSpawners)
        {
            if (spawner != null)
            {
                spawner.spawnRate = newSpawnRate;
            }
        }
        
        Debug.Log($"[SpawnerManager] Updated {allSpawners.Count} spawners to a new spawn rate of {newSpawnRate}");
    }
}
