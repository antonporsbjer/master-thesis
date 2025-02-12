using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitZoneController : MonoBehaviour
{
    public float allowWalkInterval = 5f;
    public float waitTimer;
    public bool walkingAllowed = false;
    // Start is called before the first frame update
    void Start()
    {
        waitTimer = allowWalkInterval;
    }

    // Update is called once per frame
    void Update()
    {
        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0)
        {
            waitTimer = allowWalkInterval;
            walkingAllowed = !walkingAllowed;
        }
    }
}
