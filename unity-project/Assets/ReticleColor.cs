using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReticleColor : MonoBehaviour
{

    [Header("Reticle Coloring")]
    public GameObject reticle;
    public float reticleColorChangeTime;
    public Color baseReticleColor;
    public Color reticleColorUponDeath;
    


    
    public void Start()
    {
        reticle.GetComponent<SpriteRenderer>().color = baseReticleColor;
    }

    public void FlashReticleColor() {
        reticle.GetComponent<SpriteRenderer>().color = reticleColorUponDeath;
        Invoke(nameof(ResetReticleColor), reticleColorChangeTime);
    }

    private void ResetReticleColor() {
        reticle.GetComponent<SpriteRenderer>().color = baseReticleColor;
    }
}
