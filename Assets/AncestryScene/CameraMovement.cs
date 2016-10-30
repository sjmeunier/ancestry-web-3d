﻿using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{

    public float turnSpeed = 6.0f;      // Speed of camera turning when mouse moves in along an axis
    public float panSpeed = 6.0f * Settings.scaleFactor;       // Speed of the camera when being panned
    public float zoomSpeed = 6.0f * Settings.scaleFactor;      // Speed of the camera going back and forth

    public float keyZoomSpeed = 16.0f * Settings.scaleFactor;
    public float keyPanSpeed = 16.0f * Settings.scaleFactor;

    private Vector3 mouseOrigin;    // Position of cursor when mouse dragging starts
    private bool isPanning;     // Is the camera being panned?
    private bool isRotating;    // Is the camera being rotated?
    private bool isZooming;     // Is the camera zooming?

    //
    // UPDATE
    //

    void Update()
    {
        // Get the left mouse button
        if (Input.GetMouseButtonDown(0))
        {
            // Get mouse origin
            mouseOrigin = Input.mousePosition;
            isRotating = true;
        }

        // Get the right mouse button
        if (Input.GetMouseButtonDown(1))
        {
            // Get mouse origin
            mouseOrigin = Input.mousePosition;
            isPanning = true;
        }

        // Get the middle mouse button
        if (Input.GetMouseButtonDown(2))
        {
            // Get mouse origin
            mouseOrigin = Input.mousePosition;
            isZooming = true;
        }

        // Disable movements on button release
        if (!Input.GetMouseButton(0)) isRotating = false;
        if (!Input.GetMouseButton(1)) isPanning = false;
        if (!Input.GetMouseButton(2)) isZooming = false;

        // Rotate camera along X and Y axis
        if (isRotating)
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

            transform.RotateAround(transform.position, transform.right, -pos.y * turnSpeed);
            transform.RotateAround(transform.position, Vector3.up, pos.x * turnSpeed);

            foreach (GameObject individualSphere in GameObject.FindGameObjectsWithTag("Individuals"))
                individualSphere.transform.LookAt(Camera.main.transform);
        }

        // Move the camera on it's XY plane
        if (isPanning)
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

            Vector3 move = new Vector3(pos.x * panSpeed, pos.y * panSpeed, 0);
            transform.Translate(move, Space.Self);
        }

        // Move the camera linearly along Z axis
        if (isZooming)
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

            Vector3 move = pos.y * zoomSpeed * transform.forward;
            transform.Translate(move, Space.World);
        }


        //Key movement
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Translate(new Vector3(keyPanSpeed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate(new Vector3(-keyPanSpeed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.Translate(new Vector3(0, 0, -keyZoomSpeed * Time.deltaTime));
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.Translate(new Vector3(0, 0, keyZoomSpeed * Time.deltaTime));
        }
    }
}
