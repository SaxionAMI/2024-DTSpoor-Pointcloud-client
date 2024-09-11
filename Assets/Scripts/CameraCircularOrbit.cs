using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * CameraCircularOrbit
 * The script allows the camera to orbit around a target object at a specified distance and speed.
 * The camera can either look at the target or look away from it based on the configuration.
 * (To compare performance when looking at cloud vs. not looking at it)
 * 
 * Author: Mikus Vancans
 * Date: 28/05/2024
 */

public class CameraCircularOrbit : MonoBehaviour
{
    public Transform target; // Target object to orbit around
    public float orbitDistance = 100.0f; // Distance from the target
    public float orbitSpeed = 0.3f; // Speed of the orbit

    public bool lookAway = false; // Determines whether the camera looks away from the target
    public bool orbit = false; // Determines whether the camera should orbit the target

    private void LateUpdate()
    {
        // Check if a target is set for the camera
        if (target == null)
        {
            Debug.Log("<color=red>No target selected for camera in active CameraOrbit Script</color>");
            return; // Exit the function if no target is set
        }
        if (orbit)
        {
            transform.position = CalculatePosition();

            if (lookAway)
            {
                // Look at a fixed point in the distance instead of the target
                transform.LookAt(new Vector3(0, 1000, 0));
            }
            else
            {
                transform.LookAt(target);                // Always look at the target
            }
        }
        else
        {
            // If not orbiting, look at a fixed point in the distance
            transform.LookAt(new Vector3(0, 1000, 0));
        }

    }

    // Calculate the new position of the camera based on the current time, orbit speed, and distance
    private Vector3 CalculatePosition()
    {
        float xPosition = Mathf.Cos(Time.time * orbitSpeed) * orbitDistance;
        float zPosition = Mathf.Sin(Time.time * orbitSpeed) * orbitDistance;
        Vector3 tempVector3 = new Vector3(xPosition, 0, zPosition) + target.position;

        return tempVector3;
    }


    public void SetLookAway(bool state)
    {
        lookAway = state;
    }
    public void SetOrbit(bool state)
    {
        orbit = state;
    }
    public void SetPosition(Vector3 state)
    {
        transform.position = state;
    }
}