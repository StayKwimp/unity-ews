using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation;
    float yRotation;

    private void Start(){
        
        // zet de mouse cursor op invisible en locked midden op het scherm
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void Update(){
        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        // niet vragen, i dont understan
        yRotation += mouseX;

        xRotation -= mouseY;


        // zorg dat xRotation binnen -90 en 90 graden blijft, zodat je niet verder dan dat omhoog en omlaag kan kijken
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // rotate camera
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

        // rotate player
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

    }
}
