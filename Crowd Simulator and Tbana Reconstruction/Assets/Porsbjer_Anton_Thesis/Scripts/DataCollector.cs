using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataCollector : MonoBehaviour
{
    private int visibleSignCount;

    private void SaveToJSON()
    {
        string visibilityData = JsonUtility.ToJson(this, true);
        string filePath = Application.persistentDataPath + "/data.json";
        Debug.Log("Data saved to: " + filePath);
        System.IO.File.WriteAllText(filePath, visibilityData);
        Debug.Log("Data saved: " + visibilityData);
    }

    public void IncrementVisibleSignCount()
    {
        visibleSignCount++;
        Debug.Log("Visible sign count: " + visibleSignCount);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
           SaveToJSON(); // Save data to JSON when 'S' is pressed
        }
    }
}
