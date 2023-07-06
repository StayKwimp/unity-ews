using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlashMovement : MonoBehaviour
{
    public Transform gunTransform;
    public float animationDuration;

    private GameObject[] muzzleFlashes;
    private int amount;


    private void Start() {
        // zorg dat er altijd 1 muzzle flash over blijft
        muzzleFlashes = GameObject.FindGameObjectsWithTag("MuzzleFlash");
        amount = muzzleFlashes.Length;
        if (amount > 1) Invoke("DestroyAnimation", animationDuration);
    }

    private void DestroyAnimation() {
        Destroy(gameObject);
    }

    private void Update() {
        transform.rotation = gunTransform.rotation;
        transform.position = gunTransform.position;
    }
}
