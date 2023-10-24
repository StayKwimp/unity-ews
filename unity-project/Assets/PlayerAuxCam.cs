using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAuxCam : MonoBehaviour
{
    // [Header("Sensitivity")]
    // public float sensX;
    // public float sensY;

    // [Header("ADS")]
    // public GameObject playerGun;
    // public float sensMultiplierOnADS;
    // private bool ADSEnabled;

    // public Transform orientation;

    // float xRotation;
    // float yRotation;

    // // smooth camera movement
    // private bool smoothlyMoveCamera = false;
    // private float smoothDecayDivider;
    // private float disableSmoothMovementThreshold = 0.005f;
    // private float smoothRotateX = 0f;
    // private float smoothRotateY = 0f;

    // this camera inherts all properties from the main player camera

    [Header("Camera Setup")]
    public Camera playerCam;
    private Camera thisCamera;

    private void Awake() {
        // setup this camera
        thisCamera = gameObject.GetComponent<Camera>();
    }
    private void Update() {
        transform.position = playerCam.transform.position;
        transform.rotation = playerCam.transform.rotation;
        thisCamera.fieldOfView = playerCam.fieldOfView;
    }
}
