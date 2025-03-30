using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemoveObjects : MonoBehaviour
{

    public GameObject objects;
    public Slider slider;
    
    public void ToggleElement()
    {
        if (objects != null)
        {
            bool isActive = objects.activeSelf;
            objects.SetActive(!isActive);
        }
        if (slider != null)
        {
            slider.value = 0;
        }
    }

}
