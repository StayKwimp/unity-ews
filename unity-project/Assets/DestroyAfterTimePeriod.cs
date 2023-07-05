using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTimePeriod : MonoBehaviour
{
    public GameObject objectToDestroy;
    public float destroyDelay;

    void Start() {
        Invoke("DestroyObject", destroyDelay); 
    }

    

    private void DestroyObject() {
        Destroy(objectToDestroy);
    }
}
