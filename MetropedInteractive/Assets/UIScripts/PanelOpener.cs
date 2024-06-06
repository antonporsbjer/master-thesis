using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelOpener : MonoBehaviour
{
    public GameObject Panel;
    public Slider slider;

    void Start ()
    {
        Panel.SetActive(false);
    }

    public void OpenPanel()
    {
        if (Panel != null)
        {
            bool isActive = Panel.activeSelf;
            Panel.SetActive(!isActive);
            GameManager.Instance.SetMovementPause(!isActive);
        }
        if (slider != null)
        {
            slider.value = 0;
        }
    }

}
