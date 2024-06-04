using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    private float timer = 0f; 
    private float moveInterval = 30f; 
    private MeshRenderer meshRenderer;
    Vector3 originalPos;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        // originalPos = transform.position;
        originalPos = new Vector3(-40, transform.position.y, transform.position.z);
    }

    void Update()
    {
        if (transform.position.x < 170)
        {
            transform.Translate(Vector3.right * 10 * Time.deltaTime); 
        }
        else
        {
            timer += Time.deltaTime; 

            if (timer >= moveInterval)
            {
                transform.position = originalPos;              
                timer = 0f; 
            }
        }
    }
}