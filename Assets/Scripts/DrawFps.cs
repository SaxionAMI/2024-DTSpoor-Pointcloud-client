using TMPro;
using UnityEngine;
using UnityEngine.UI;

/**
 * DrawFps
 * This script calculates and displays the current frames per second (FPS) on the screen.
 * It shows the FPS both in the upper-left corner of the screen and in a TextMeshProUGUI component.
 * 
 * Author: Mikus Vancans
 * Date: 11/06/2024
 */

public class DrawFps : MonoBehaviour
{
    float deltaTime = 0.0f;
    [SerializeField] TextMeshProUGUI textAttribute;

    void Update()
    {
        // Calculate the time it took to complete the last frame and smooth it out using an exponential moving average
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        // Set up the rectangle for displaying FPS
        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100; // Set the font size as 2% of screen height
        style.normal.textColor = Color.green; // Set the text color to green

        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);

        // Display the FPS text in the upper-left corner of the screen
        GUI.Label(rect, text, style);

        //Display the FPS on the Menu screen
        textAttribute.text = text;
    }
}