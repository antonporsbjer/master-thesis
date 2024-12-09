using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrainArrival : MonoBehaviour {
public List<Spawner> spawnGroup;
public float trainArrivalInterval = 10f;
private float time = 0f;

    void Update()
    {
        time += Time.deltaTime;
        if (time > trainArrivalInterval)
        {
            foreach (Spawner spawner in spawnGroup)
            {
                spawner.AreaSpawn();
            }
            time = 0f;
        }
    }
}