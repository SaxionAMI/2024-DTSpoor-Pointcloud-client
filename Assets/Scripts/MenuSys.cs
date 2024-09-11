using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * MenuSys
 * It listens for the Escape key to toggle the applications pause state and handles the menu UI.
 * 
 * Author: Mikus Vancans
 * Date: 24/06/2024
 */

public class MenuSys : MonoBehaviour
{
    public static bool gamePaused = false;
    [SerializeField] GameObject MenuUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gamePaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        MenuUI.SetActive(false);
        Time.timeScale = 1f;
        gamePaused = false;
    }

    public void Pause()
    {
        MenuUI.SetActive(true);
        Time.timeScale = 0f;
        gamePaused = true;
    }
}