using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMovementOnStart : MonoBehaviour
{
    public GameObject Panel;
    public GameObject objects;


    void Start ()
    {
        if (Panel != null)
        {
            Panel.SetActive(true);
            GameManager.Instance.SetMovementPause(true);
        }
        if (objects != null)
        {
            objects.SetActive(false);
        }

        
    }
}
