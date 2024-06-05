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

    void Start ()
    {
        Volume postProcessingVolume = GameObject.FindGameObjectWithTag("PostProcessing").GetComponent<Volume>();
        postProcessingVolume.profile.TryGet(out colorAdjustments);


        exposureSlider.value = colorAdjustments.postExposure.value;
        exposureSlider.onValueChanged.AddListener(HandleSliderValueChanged);
    }
    public void HandleSliderValueChanged (float value)
    {
        colorAdjustments.postExposure.value = value;
    }
}
