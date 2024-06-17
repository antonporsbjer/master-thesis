using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//This script is important! The normal RemoveObjects script doesn't work on the crowd as it permanently disables the spawning of new agents when toggled.
public class CrowdToggle : MonoBehaviour
{
    public GameObject objects;
    public Slider slider;
    public GameObject mainObject;

    public void ToggleElement()
    {
        if (objects != null)
        {
            bool isActive = objects.activeSelf;
            objects.SetActive(!isActive);

            if (!isActive && mainObject != null)
            {
                StartCoroutine(ReenableMainObject());
            }
        }

        if (slider != null)
        {
            slider.value = 0;
        }
    }

    private IEnumerator ReenableMainObject()
    {
        mainObject.SetActive(false);
        yield return null; // Wait for a frame
        mainObject.SetActive(true);
    }
}
