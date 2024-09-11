using FastPoints;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/**
 * visiblePointCount
 * A simple script to update a TextMeshProUGUI component to display the current visible point count from the PointCloudRenderer.
 * It listens to changes in the visible point count and updates the text accordingly.
 * 
 * Author: Mikus Vancans
 * Date: 02/07/2024
 */

public class visiblePointCount : MonoBehaviour
{
    [SerializeField] PointCloudRenderer renderer;
    [SerializeField] TextMeshProUGUI text;

    void Update()
    {
        text.text = renderer.visiblePointCount.ToString(); 
    }
}
