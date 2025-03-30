using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMovementOnStart : MonoBehaviour
{
    public GameObject Panel;
    public GameObject FreeMenu;
    public GameObject RatingMenu;


    void Start ()
    {
        if (Panel != null)
        {
            Panel.SetActive(true);
            GameManager.Instance.SetMovementPause(true);
        }
        if (FreeMenu != null)
        {
            FreeMenu.SetActive(false);
        }
        if (RatingMenu != null)
        {
            RatingMenu.SetActive(false);
        }
        
    }
}
