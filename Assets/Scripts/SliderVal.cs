using FastPoints;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/**
 * SliderVal
 * This very simple script updates a TextMeshProUGUI component to display the current value of a Slider component.
 * It listens to changes in the slider value and updates the text accordingly.
 * 
 * Author: Mikus Vancans
 * Date: 02/07/2024
 */

public class SliderVal : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Slider slider;

    // Update is called once per frame
    void Update()
    {
        text.text = slider.value.ToString();
    }
}
