using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelOpener : MonoBehaviour
{
    public GameObject Panel;
    public Slider slider1;
    public Slider slider2;
    public Slider slider3;

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
        if (slider1 != null && slider2 != null && slider3 != null)
        {
            slider1.value = 0;
            slider2.value = 0;
            slider3.value = 0;
        }
    }

}
