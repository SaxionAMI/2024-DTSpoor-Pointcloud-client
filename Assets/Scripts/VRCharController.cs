using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

/**
 * VRCharController
 * A script whcih controls the movement of a VR character based on input from an XR controller.
 * It uses the CharacterController component to move the character in the direction the user is looking.
 * 
 * Adapted from Chase Mitchell, 15/05/2021 https://sneakydaggergames.medium.com/vr-in-unity-how-to-create-a-continuous-movement-system-track-real-space-movement-2bd6fe31df0a
 * Author: Mikus Vancans
 * Date: 27/06/2024
 */

public class VRCharController : MonoBehaviour
{
    Vector2 inputAxis;

    public XRNode input;
    private XROrigin rig;
    private CharacterController character;
    [SerializeField] float speed = 1.0f;

    void Start()
    {
        character = GetComponent<CharacterController>();
        rig = GetComponent<XROrigin>();
    }
    void Update()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(input); // Get the input device for the specified XRNode
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out inputAxis); // Try to get the value of the primary 2D axis (joystick or touchpad) from the input device
    }

    private void FixedUpdate()
    {
        // Calculate the direction to move based on the input axis and the direction the user is looking
        Quaternion headYaw = Quaternion.Euler(x: 0, rig.Camera.transform.eulerAngles.y, z: 0);
        Vector3 dir = headYaw * new Vector3(inputAxis.x, 0, inputAxis.y);
        // Move the character in the calculated direction
        character.Move(dir * speed * Time.deltaTime);
    }
}
