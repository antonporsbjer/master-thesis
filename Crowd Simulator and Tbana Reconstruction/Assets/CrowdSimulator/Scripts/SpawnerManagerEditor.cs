#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(SpawnerManager))]
public class SpawnerManagerEditor : Editor
{
    private SpawnerManager manager;
    private SerializedProperty useGlobalSpawnRate_Prop;
    private SerializedProperty globalSpawnRate_Prop;
    private SerializedProperty spawnRateMultiplier_Prop;
    private SerializedProperty spawnerConfigs_Prop;

    private string searchFilter = "";
    private float bulkRateValue = 1.0f;
    private Dictionary<string, bool> zoneFoldouts = new Dictionary<string, bool>();
    private Dictionary<string, float> zoneBulkRates = new Dictionary<string, float>();

    void OnEnable()
    {
        manager = (SpawnerManager)target;
        useGlobalSpawnRate_Prop = serializedObject.FindProperty("useGlobalSpawnRate");
        globalSpawnRate_Prop = serializedObject.FindProperty("globalSpawnRate");
        spawnRateMultiplier_Prop = serializedObject.FindProperty("spawnRateMultiplier");
        spawnerConfigs_Prop = serializedObject.FindProperty("spawnerConfigs");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 1. Statistics & Overview Panel
        DrawStatisticsHeader();

        EditorGUILayout.Space(8);

        // 2. Mode & Global Controls Panel
        DrawGlobalSettings();

        EditorGUILayout.Space(8);

        // 3. Toolbar / Batch Operations
        DrawToolbar();

        EditorGUILayout.Space(8);

        // 4. Search Filter
        DrawSearchFilter();

        EditorGUILayout.Space(8);

        // 5. Spawners Grouped by Zone
        DrawSpawnerList();

        serializedObject.ApplyModifiedProperties();

        // Real-time synchronization during Play mode
        if (Application.isPlaying && GUI.changed)
        {
            manager.ApplyAllConfigurations();
        }
    }

    private void DrawStatisticsHeader()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 13,
            alignment = TextAnchor.MiddleLeft
        };

        EditorGUILayout.LabelField("Spawner Manager Control Panel", titleStyle);
        EditorGUILayout.Space(2);

        int totalCount = manager.spawnerConfigs != null ? manager.spawnerConfigs.Count : 0;
        int activeCount = manager.ActiveSpawnerCount;
        float totalRate = manager.TotalSpawnRate;
        string modeStr = manager.useGlobalSpawnRate ? "Global Mode" : "Independent Mode";

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Mode: <b>{modeStr}</b>", new GUIStyle(EditorStyles.label) { richText = true });
        EditorGUILayout.LabelField($"Spawners: <b>{activeCount} / {totalCount} active</b>", new GUIStyle(EditorStyles.label) { richText = true });
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Combined Rate: <b>{totalRate:F2} agents/sec</b>", new GUIStyle(EditorStyles.label) { richText = true });
        if (manager.spawnRateMultiplier != 1.0f)
        {
            EditorGUILayout.LabelField($"Multiplier: <b>{manager.spawnRateMultiplier:F2}x</b>", new GUIStyle(EditorStyles.label) { richText = true });
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawGlobalSettings()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Global Configuration", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(useGlobalSpawnRate_Prop, new GUIContent("Use Global Spawn Rate", "When enabled, all spawners follow the global rate unless explicitly set to Override."));

        if (useGlobalSpawnRate_Prop.boolValue)
        {
            EditorGUILayout.PropertyField(globalSpawnRate_Prop, new GUIContent("Global Spawn Rate (ag/s)"));
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(spawnRateMultiplier_Prop, new GUIContent("Spawn Rate Multiplier", "Scales all spawner rates proportionally. Useful for testing crowd density load."));
        if (spawnRateMultiplier_Prop.floatValue != 1.0f)
        {
            if (GUILayout.Button("Reset 1x", GUILayout.Width(65)))
            {
                spawnRateMultiplier_Prop.floatValue = 1.0f;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Batch Actions", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Find All in Scene", GUILayout.Height(24)))
        {
            Undo.RecordObject(manager, "Find All Spawners");
            manager.PopulateFromScene();
            EditorUtility.SetDirty(manager);
        }
        if (GUILayout.Button("Apply to Spawners", GUILayout.Height(24)))
        {
            manager.ApplyAllConfigurations();
        }
        if (GUILayout.Button("Read from Spawners", GUILayout.Height(24)))
        {
            Undo.RecordObject(manager, "Read from Spawners");
            manager.ReadAllFromScene();
            EditorUtility.SetDirty(manager);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Enable All"))
        {
            Undo.RecordObject(manager, "Enable All Spawners");
            manager.SetAllEnabled(true);
            EditorUtility.SetDirty(manager);
        }
        if (GUILayout.Button("Disable All"))
        {
            Undo.RecordObject(manager, "Disable All Spawners");
            manager.SetAllEnabled(false);
            EditorUtility.SetDirty(manager);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Set All Rates:", GUILayout.Width(85));
        bulkRateValue = EditorGUILayout.FloatField(bulkRateValue, GUILayout.Width(60));
        if (GUILayout.Button("Apply to All Rates"))
        {
            Undo.RecordObject(manager, "Set All Rates");
            manager.SetAllRates(bulkRateValue);
            EditorUtility.SetDirty(manager);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawSearchFilter()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Filter:", GUILayout.Width(45));
        searchFilter = EditorGUILayout.TextField(searchFilter);
        if (!string.IsNullOrEmpty(searchFilter))
        {
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                searchFilter = "";
                GUI.FocusControl(null);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawSpawnerList()
    {
        if (spawnerConfigs_Prop.arraySize == 0)
        {
            EditorGUILayout.HelpBox("No spawners configured. Click 'Find All in Scene' to automatically discover all spawners.", MessageType.Info);
            return;
        }

        // Group indices by zone
        Dictionary<string, List<int>> zoneMap = new Dictionary<string, List<int>>();
        string filterLower = searchFilter.Trim().ToLowerInvariant();

        for (int i = 0; i < spawnerConfigs_Prop.arraySize; i++)
        {
            SerializedProperty configProp = spawnerConfigs_Prop.GetArrayElementAtIndex(i);
            SerializedProperty zoneProp = configProp.FindPropertyRelative("zoneName");
            SerializedProperty nameProp = configProp.FindPropertyRelative("displayName");
            SerializedProperty spawnerProp = configProp.FindPropertyRelative("spawner");

            string zone = !string.IsNullOrEmpty(zoneProp.stringValue) ? zoneProp.stringValue : "Ungrouped";
            string displayName = nameProp.stringValue;
            string spawnerName = spawnerProp.objectReferenceValue != null ? spawnerProp.objectReferenceValue.name : "";

            // Check filter
            if (!string.IsNullOrEmpty(filterLower))
            {
                bool matches = zone.ToLowerInvariant().Contains(filterLower) ||
                               displayName.ToLowerInvariant().Contains(filterLower) ||
                               spawnerName.ToLowerInvariant().Contains(filterLower);
                if (!matches) continue;
            }

            if (!zoneMap.ContainsKey(zone))
            {
                zoneMap[zone] = new List<int>();
            }
            zoneMap[zone].Add(i);
        }

        if (zoneMap.Count == 0)
        {
            EditorGUILayout.HelpBox($"No spawners match filter '{searchFilter}'.", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField("Spawners", EditorStyles.boldLabel);

        foreach (var pair in zoneMap)
        {
            string zone = pair.Key;
            List<int> indices = pair.Value;

            if (!zoneFoldouts.ContainsKey(zone))
            {
                zoneFoldouts[zone] = true;
            }
            if (!zoneBulkRates.ContainsKey(zone))
            {
                zoneBulkRates[zone] = 1.0f;
            }

            // Zone Header Box
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Calculate zone stats
            int zoneActive = 0;
            float zoneTotalRate = 0f;
            foreach (int idx in indices)
            {
                SerializedProperty cProp = spawnerConfigs_Prop.GetArrayElementAtIndex(idx);
                bool enabled = cProp.FindPropertyRelative("isEnabled").boolValue;
                float rate = cProp.FindPropertyRelative("spawnRate").floatValue;
                if (enabled)
                {
                    zoneActive++;
                    zoneTotalRate += rate * manager.spawnRateMultiplier;
                }
            }

            EditorGUILayout.BeginHorizontal();
            zoneFoldouts[zone] = EditorGUILayout.Foldout(zoneFoldouts[zone], $"<b>{zone}</b>  ({zoneActive}/{indices.Count} active, {zoneTotalRate:F2} ag/s)", true, new GUIStyle(EditorStyles.foldout) { richText = true });
            
            // Quick zone action buttons
            if (GUILayout.Button("Enable", EditorStyles.miniButtonLeft, GUILayout.Width(50)))
            {
                Undo.RecordObject(manager, $"Enable Zone {zone}");
                manager.SetZoneEnabled(zone, true);
                EditorUtility.SetDirty(manager);
            }
            if (GUILayout.Button("Disable", EditorStyles.miniButtonMid, GUILayout.Width(50)))
            {
                Undo.RecordObject(manager, $"Disable Zone {zone}");
                manager.SetZoneEnabled(zone, false);
                EditorUtility.SetDirty(manager);
            }
            EditorGUILayout.EndHorizontal();

            // Zone bulk rate row
            if (zoneFoldouts[zone])
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Zone Rate:", GUILayout.Width(70));
                zoneBulkRates[zone] = EditorGUILayout.FloatField(zoneBulkRates[zone], GUILayout.Width(50));
                if (GUILayout.Button("Set All in Zone", EditorStyles.miniButton, GUILayout.Width(100)))
                {
                    Undo.RecordObject(manager, $"Set Zone {zone} Rate");
                    manager.SetZoneRate(zone, zoneBulkRates[zone]);
                    EditorUtility.SetDirty(manager);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(4);

                // Draw each spawner in this zone
                foreach (int idx in indices)
                {
                    DrawSingleSpawner(idx);
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }
    }

    private void DrawSingleSpawner(int index)
    {
        SerializedProperty configProp = spawnerConfigs_Prop.GetArrayElementAtIndex(index);
        SerializedProperty spawnerProp = configProp.FindPropertyRelative("spawner");
        SerializedProperty isEnabledProp = configProp.FindPropertyRelative("isEnabled");
        SerializedProperty displayNameProp = configProp.FindPropertyRelative("displayName");
        SerializedProperty spawnRateProp = configProp.FindPropertyRelative("spawnRate");
        SerializedProperty usePoissonProp = configProp.FindPropertyRelative("usePoisson");
        SerializedProperty customGoalProp = configProp.FindPropertyRelative("customGoal");
        SerializedProperty overrideGlobalProp = configProp.FindPropertyRelative("overrideGlobalRate");

        // Subtle row shading
        Color origColor = GUI.backgroundColor;
        if (!isEnabledProp.boolValue)
        {
            GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
        }

        EditorGUILayout.BeginVertical(GUI.skin.box);
        GUI.backgroundColor = origColor;

        // Line 1: [Enabled] [Spawner Object] [DisplayName]
        EditorGUILayout.BeginHorizontal();
        isEnabledProp.boolValue = EditorGUILayout.Toggle(isEnabledProp.boolValue, GUILayout.Width(20));
        
        EditorGUILayout.PropertyField(spawnerProp, GUIContent.none, GUILayout.Width(130));

        if (string.IsNullOrEmpty(displayNameProp.stringValue) && spawnerProp.objectReferenceValue != null)
        {
            displayNameProp.stringValue = spawnerProp.objectReferenceValue.name;
        }
        displayNameProp.stringValue = EditorGUILayout.TextField(displayNameProp.stringValue);
        EditorGUILayout.EndHorizontal();

        // Line 2: Rate, Poisson, Goal, Override
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Rate:", GUILayout.Width(35));
        spawnRateProp.floatValue = Mathf.Max(0f, EditorGUILayout.FloatField(spawnRateProp.floatValue, GUILayout.Width(50)));

        EditorGUILayout.LabelField("ag/s", GUILayout.Width(30));

        usePoissonProp.boolValue = EditorGUILayout.ToggleLeft("Poisson", usePoissonProp.boolValue, GUILayout.Width(65));

        if (useGlobalSpawnRate_Prop.boolValue)
        {
            overrideGlobalProp.boolValue = EditorGUILayout.ToggleLeft("Override Global", overrideGlobalProp.boolValue, GUILayout.Width(110));
        }

        EditorGUILayout.LabelField("Goal:", GUILayout.Width(35));
        EditorGUILayout.PropertyField(customGoalProp, GUIContent.none);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }
}
#endif
