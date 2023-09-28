using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReticleColor : MonoBehaviour
{

    [Header("Reticle Coloring")]
    public float reticleColorChangeTime;
    public Color baseReticleColor;
    public Color reticleColorUponDeath;
    


    
    public void Start()
    {
        gameObject.GetComponent<SpriteRenderer>().color = baseReticleColor;
    }

    public void FlashReticleColor() {
        gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        Invoke(nameof(ResetReticleColor), reticleColorChangeTime);
    }

    private void ResetReticleColor() {
        gameObject.GetComponent<SpriteRenderer>().color = baseReticleColor;
    }
}
