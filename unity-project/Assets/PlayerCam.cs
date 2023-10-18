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
}
