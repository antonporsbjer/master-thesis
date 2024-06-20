using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ChangeExposure : MonoBehaviour
{
    public Slider exposureSlider;
    private ColorAdjustments colorAdjustments;
    public Slider slider;
    private float[] exposureValues = {-1.5f, 1.1f, 2.1f}; //Change to whatever is deemed fitting

    void Start ()
    {
        Volume postProcessingVolume = GameObject.FindGameObjectWithTag("PostProcessing").GetComponent<Volume>();
        postProcessingVolume.profile.TryGet(out colorAdjustments);


        exposureSlider.value = colorAdjustments.postExposure.value;
        exposureSlider.onValueChanged.AddListener(HandleSliderValueChanged);
    }
    public void HandleSliderValueChanged (float value)
    {
        int indexValue = Mathf.RoundToInt(value);
        //Debug.Log("Slider value; " + value);
        //Debug.Log("IndexValue: " + indexValue);

        if (indexValue >= 0 && indexValue < exposureValues.Length){
            float exposureValue = exposureValues[indexValue];
            //Debug.Log("Setting exposure to: " + exposureValue);
            colorAdjustments.postExposure.value = exposureValue;
            //Debug.Log("Exposure set to: " + colorAdjustments.postExposure.value);
        }
        else
        {
            Debug.LogError("ExposureSlider value is out of bounds.");
        }
        if (slider != null)
        {
            slider.value = 0;
        }
    }
}
