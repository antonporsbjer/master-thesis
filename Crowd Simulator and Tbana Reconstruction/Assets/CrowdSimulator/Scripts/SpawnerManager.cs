using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class SpawnerConfig
{
    [Tooltip("Zone or parent group name (e.g. 'zone1', 'Platform East').")]
    public string zoneName = "";

    [Tooltip("Display label for this spawner in the manager.")]
    public string displayName = "";

    [Tooltip("Reference to the NewSpawner component in the scene.")]
    public NewSpawner spawner;

    [Tooltip("Whether this spawner is enabled and actively spawning agents.")]
    public bool isEnabled = true;

    [Tooltip("Independent spawn rate (agents per second) for this specific spawner.")]
    [Min(0f)]
    public float spawnRate = 1.0f;

    [Tooltip("Whether to use Poisson distribution for time intervals between spawns.")]
    public bool usePoisson = false;

    [Tooltip("Optional custom target goal node for agents spawned from this spawner.")]
    public CustomNode customGoal;

    [Tooltip("If true, this spawner keeps its own spawnRate even when Global Spawn Rate mode is active.")]
    public bool overrideGlobalRate = false;

    public string GetEffectiveName()
    {
        if (!string.IsNullOrEmpty(displayName))
        {
            return displayName;
        }
        if (spawner != null)
        {
            return spawner.gameObject.name;
        }
        return "Unassigned Spawner";
    }

    public string GetFullName()
    {
        string baseName = GetEffectiveName();
        if (!string.IsNullOrEmpty(zoneName))
        {
            return $"{zoneName}/{baseName}";
        }
        if (spawner != null && spawner.transform.parent != null)
        {
            return $"{spawner.transform.parent.name}/{baseName}";
        }
        return baseName;
    }

    public void ApplyToSpawner(float globalRate, bool useGlobal, float multiplier = 1.0f)
    {
        if (spawner == null) return;

        spawner.enabled = isEnabled;
        spawner.usePoisson = usePoisson;
        if (customGoal != null)
        {
            spawner.customGoal = customGoal;
        }

        float effectiveRate = (useGlobal && !overrideGlobalRate) ? globalRate : spawnRate;
        spawner.spawnRate = Mathf.Max(0f, effectiveRate * multiplier);
    }

    public void ReadFromSpawner()
    {
        if (spawner == null) return;
        isEnabled = spawner.enabled;
        spawnRate = spawner.spawnRate;
        usePoisson = spawner.usePoisson;
        customGoal = spawner.customGoal;

        if (spawner.transform.parent != null)
        {
            zoneName = spawner.transform.parent.name;
        }
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = spawner.gameObject.name;
        }
    }
}

public class SpawnerManager : MonoBehaviour
{
    public static SpawnerManager instance;

    [Header("Mode Configuration")]
    [Tooltip("When enabled, all spawners follow the Global Spawn Rate (unless 'Override Global' is checked on a spawner). When disabled, each spawner uses its independent spawn rate.")]
    public bool useGlobalSpawnRate = false;

    [Tooltip("Global spawn rate used when 'Use Global Spawn Rate' is enabled.")]
    [Min(0f)]
    public float globalSpawnRate = 1.0f;

    [Tooltip("Multiplier applied across all spawner rates (both independent and global). Useful for scaling overall crowd density while maintaining relative per-spawner ratios.")]
    [Min(0f)]
    public float spawnRateMultiplier = 1.0f;

    [Header("Independent Spawner Configurations")]
    [Tooltip("List of all managed spawners with independent configuration.")]
    public List<SpawnerConfig> spawnerConfigs = new List<SpawnerConfig>();

    [HideInInspector]
    [Tooltip("Maintained for backwards compatibility with legacy scripts.")]
    public List<NewSpawner> allSpawners = new List<NewSpawner>();

    /// <summary>
    /// Total combined spawn rate of all currently enabled spawners (agents/sec).
    /// </summary>
    public float TotalSpawnRate
    {
        get
        {
            float total = 0f;
            foreach (var cfg in spawnerConfigs)
            {
                if (cfg != null && cfg.spawner != null && cfg.isEnabled)
                {
                    float rate = (useGlobalSpawnRate && !cfg.overrideGlobalRate) ? globalSpawnRate : cfg.spawnRate;
                    total += Mathf.Max(0f, rate * spawnRateMultiplier);
                }
            }
            return total;
        }
    }

    /// <summary>
    /// Count of currently enabled spawners.
    /// </summary>
    public int ActiveSpawnerCount
    {
        get
        {
            int count = 0;
            foreach (var cfg in spawnerConfigs)
            {
                if (cfg != null && cfg.spawner != null && cfg.isEnabled) count++;
            }
            return count;
        }
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (spawnerConfigs == null || spawnerConfigs.Count == 0)
        {
            PopulateFromScene();
        }
        else
        {
            SyncWithSceneSpawners();
        }

        ApplyAllConfigurations();
    }

    /// <summary>
    /// Scans the scene and builds spawnerConfigs, preserving any existing configurations.
    /// </summary>
    public void PopulateFromScene()
    {
        NewSpawner[] sceneSpawners = FindObjectsOfType<NewSpawner>();

        // Sort by parent name (zone) and then gameobject name for clean ordering
        System.Array.Sort(sceneSpawners, (a, b) =>
        {
            string parentA = a.transform.parent != null ? a.transform.parent.name : "";
            string parentB = b.transform.parent != null ? b.transform.parent.name : "";
            int parentComp = string.Compare(parentA, parentB, System.StringComparison.Ordinal);
            if (parentComp != 0) return parentComp;
            return string.Compare(a.gameObject.name, b.gameObject.name, System.StringComparison.Ordinal);
        });

        // Map existing configs by spawner reference
        Dictionary<NewSpawner, SpawnerConfig> existing = new Dictionary<NewSpawner, SpawnerConfig>();
        if (spawnerConfigs != null)
        {
            foreach (var cfg in spawnerConfigs)
            {
                if (cfg != null && cfg.spawner != null && !existing.ContainsKey(cfg.spawner))
                {
                    existing[cfg.spawner] = cfg;
                }
            }
        }

        List<SpawnerConfig> newList = new List<SpawnerConfig>();
        allSpawners.Clear();

        foreach (var s in sceneSpawners)
        {
            if (s == null) continue;
            allSpawners.Add(s);

            if (existing.TryGetValue(s, out SpawnerConfig cfg))
            {
                // Update parent / zone name if changed
                if (s.transform.parent != null)
                {
                    cfg.zoneName = s.transform.parent.name;
                }
                newList.Add(cfg);
            }
            else
            {
                SpawnerConfig newCfg = new SpawnerConfig { spawner = s };
                newCfg.ReadFromSpawner();
                newList.Add(newCfg);
            }
        }

        spawnerConfigs = newList;
    }

    /// <summary>
    /// Synchronizes the list with scene spawners: keeps existing entries and adds any newly added spawners.
    /// </summary>
    public void SyncWithSceneSpawners()
    {
        NewSpawner[] sceneSpawners = FindObjectsOfType<NewSpawner>();
        HashSet<NewSpawner> currentSet = new HashSet<NewSpawner>();
        foreach (var cfg in spawnerConfigs)
        {
            if (cfg != null && cfg.spawner != null)
            {
                currentSet.Add(cfg.spawner);
            }
        }

        bool addedNew = false;
        foreach (var s in sceneSpawners)
        {
            if (s != null && !currentSet.Contains(s))
            {
                SpawnerConfig newCfg = new SpawnerConfig { spawner = s };
                newCfg.ReadFromSpawner();
                spawnerConfigs.Add(newCfg);
                addedNew = true;
            }
        }

        // Clean up nulls
        spawnerConfigs.RemoveAll(cfg => cfg == null || cfg.spawner == null);

        // Update allSpawners list
        allSpawners.Clear();
        foreach (var cfg in spawnerConfigs)
        {
            allSpawners.Add(cfg.spawner);
        }

        if (addedNew)
        {
            // Re-sort if new spawners were added
            spawnerConfigs = spawnerConfigs.OrderBy(c => c.zoneName).ThenBy(c => c.GetEffectiveName()).ToList();
        }
    }

    /// <summary>
    /// Pushes all settings from SpawnerManager to the NewSpawner components in the scene.
    /// </summary>
    public void ApplyAllConfigurations()
    {
        allSpawners.Clear();
        foreach (var cfg in spawnerConfigs)
        {
            if (cfg == null || cfg.spawner == null) continue;
            allSpawners.Add(cfg.spawner);
            cfg.ApplyToSpawner(globalSpawnRate, useGlobalSpawnRate, spawnRateMultiplier);
        }

        Debug.Log($"[SpawnerManager] Applied configurations to {spawnerConfigs.Count} spawners (Active: {ActiveSpawnerCount}, Total Rate: {TotalSpawnRate:F2} agents/s)");
    }

    /// <summary>
    /// Reads current values from all NewSpawner components in the scene into SpawnerManager configs.
    /// </summary>
    public void ReadAllFromScene()
    {
        foreach (var cfg in spawnerConfigs)
        {
            if (cfg != null && cfg.spawner != null)
            {
                cfg.ReadFromSpawner();
            }
        }
    }

    /// <summary>
    /// Sets the global spawn rate and applies it.
    /// Backwards compatible with existing scripts.
    /// </summary>
    public void SetGlobalSpawnRate(float newSpawnRate)
    {
        globalSpawnRate = newSpawnRate;
        if (useGlobalSpawnRate)
        {
            foreach (var cfg in spawnerConfigs)
            {
                if (cfg != null && cfg.spawner != null && !cfg.overrideGlobalRate)
                {
                    cfg.spawner.spawnRate = Mathf.Max(0f, newSpawnRate * spawnRateMultiplier);
                }
            }
        }
        else
        {
            ApplyAllConfigurations();
        }
        Debug.Log($"[SpawnerManager] Updated global spawn rate to {newSpawnRate}");
    }

    /// <summary>
    /// Sets a specific spawner's rate and applies it.
    /// </summary>
    public void SetSpawnerRate(NewSpawner spawner, float rate)
    {
        if (spawner == null) return;
        var cfg = spawnerConfigs.Find(c => c.spawner == spawner);
        if (cfg != null)
        {
            cfg.spawnRate = rate;
            cfg.ApplyToSpawner(globalSpawnRate, useGlobalSpawnRate, spawnRateMultiplier);
        }
        else
        {
            spawner.spawnRate = rate;
        }
    }

    /// <summary>
    /// Sets a specific spawner's enabled state and applies it.
    /// </summary>
    public void SetSpawnerEnabled(NewSpawner spawner, bool isEnabled)
    {
        if (spawner == null) return;
        var cfg = spawnerConfigs.Find(c => c.spawner == spawner);
        if (cfg != null)
        {
            cfg.isEnabled = isEnabled;
            cfg.ApplyToSpawner(globalSpawnRate, useGlobalSpawnRate, spawnRateMultiplier);
        }
        else
        {
            spawner.enabled = isEnabled;
        }
    }

    /// <summary>
    /// Sets the spawn rate for all spawners within a given zone.
    /// </summary>
    public void SetZoneRate(string zoneName, float rate)
    {
        foreach (var cfg in spawnerConfigs)
        {
            if (cfg != null && cfg.zoneName == zoneName)
            {
                cfg.spawnRate = rate;
                cfg.ApplyToSpawner(globalSpawnRate, useGlobalSpawnRate, spawnRateMultiplier);
            }
        }
    }

    /// <summary>
    /// Enables or disables all spawners within a given zone.
    /// </summary>
    public void SetZoneEnabled(string zoneName, bool isEnabled)
    {
        foreach (var cfg in spawnerConfigs)
        {
            if (cfg != null && cfg.zoneName == zoneName)
            {
                cfg.isEnabled = isEnabled;
                cfg.ApplyToSpawner(globalSpawnRate, useGlobalSpawnRate, spawnRateMultiplier);
            }
        }
    }

    /// <summary>
    /// Enables or disables all managed spawners.
    /// </summary>
    public void SetAllEnabled(bool isEnabled)
    {
        foreach (var cfg in spawnerConfigs)
        {
            if (cfg != null)
            {
                cfg.isEnabled = isEnabled;
                cfg.ApplyToSpawner(globalSpawnRate, useGlobalSpawnRate, spawnRateMultiplier);
            }
        }
    }

    /// <summary>
    /// Sets all individual spawn rates to a uniform value.
    /// </summary>
    public void SetAllRates(float rate)
    {
        foreach (var cfg in spawnerConfigs)
        {
            if (cfg != null)
            {
                cfg.spawnRate = rate;
                cfg.ApplyToSpawner(globalSpawnRate, useGlobalSpawnRate, spawnRateMultiplier);
            }
        }
    }

    /// <summary>
    /// Called by RunManager to scale or set the crowd density for a run.
    /// In global mode: sets globalSpawnRate to density.
    /// In independent mode: applies density as the spawnRateMultiplier, preserving per-spawner relative ratios.
    /// </summary>
    public void ApplyCrowdDensity(float density)
    {
        if (useGlobalSpawnRate)
        {
            globalSpawnRate = density;
            spawnRateMultiplier = 1.0f;
        }
        else
        {
            spawnRateMultiplier = density;
        }
        ApplyAllConfigurations();
    }
}
