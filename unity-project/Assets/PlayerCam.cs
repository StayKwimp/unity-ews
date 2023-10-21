using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    [Header("Sensitivity")]
    public float sensX;
    public float sensY;

    [Header("ADS")]
    public GameObject playerGun;
    public float sensMultiplierOnADS;
    private bool ADSEnabled;

    public Transform orientation;

    float xRotation;
    float yRotation;

    // smooth camera movement
    private bool smoothlyMoveCamera = false;
    private float smoothDecayDivider;
    private float disableSmoothMovementThreshold = 0.005f;
    private float smoothRotateX = 0f;
    private float smoothRotateY = 0f;


    private void Start(){
        
        // zet de mouse cursor op invisible en locked midden op het scherm
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void Update(){
        // check if gun is on ADS
        ADSEnabled = playerGun.GetComponent<PlayerGun>().ADSEnabled;

        
        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        if (ADSEnabled) {
            mouseX *= sensMultiplierOnADS;
            mouseY *= sensMultiplierOnADS;
        }

        RotateCamera(mouseX, mouseY);


        // smooth camera movement
        if (smoothlyMoveCamera) {
            // gebruikt recursie om de nieuwe waarde te berekenen
            float newSmoothRotateX = smoothRotateX / smoothDecayDivider;
            float newSmoothRotateY = smoothRotateY / smoothDecayDivider;
            
            // pas die nieuwe waarde toe
            smoothRotateX = newSmoothRotateX;
            smoothRotateY = newSmoothRotateY;
            
            // controleer of beide X en Y boven de threshold zitten om te kunnen bewegen
            if (smoothRotateX < disableSmoothMovementThreshold && smoothRotateY < disableSmoothMovementThreshold) {
                // zo nee, stop met de camera proberen te bewegen
                smoothlyMoveCamera = false;
            } else {
                // zo ja, draai de camera
                RotateCamera(smoothRotateX, smoothRotateY);
            }

        }

    }

    // public zodat andere functies (voor o.a. gun recoil) buiten dit script het ook kunnen callen
    public void RotateCamera(float rotateX, float rotateY) {
        // bepaal de rotation
        yRotation += rotateX;

        xRotation -= rotateY;

        // zorg dat xRotation binnen -90 en 90 graden blijft, zodat je niet verder dan dat omhoog en omlaag kan kijken
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // rotate camera
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

        // rotate player
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }


    public bool SmoothRotateCameraInit(float initRotateXVel, float initRotateYVel, float decayDivider) {
        // controleer of decayDivider niet de camera oneindig laat bewegen
        if (decayDivider <= 1f) {
            Debug.LogWarning($"decayDivider cannot be 1 or smaller than 1. decayDivider: {decayDivider}");
            return false;
        }

        // rest van de setup
        smoothlyMoveCamera = true;
        smoothRotateX = initRotateXVel;
        smoothRotateY = initRotateYVel;
        smoothDecayDivider = decayDivider;

        // return true om te laten weten dat de setup correct is
        return true;
    }
}
