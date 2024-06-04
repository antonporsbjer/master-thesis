using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveObjects : MonoBehaviour
{

    public GameObject objects;
    public void ToggleElement()
    {
        if (objects != null)
        {
            bool isActive = objects.activeSelf;
            objects.SetActive(!isActive);
        }
    }

}
