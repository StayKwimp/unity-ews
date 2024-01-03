using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class goalSpawner : MonoBehaviour
{

    [Header("Spawnpoints")]
    public GameObject spawnerObject;
    public float respawnDelay;

    // Start is called before the first frame update
    void Start()
    {
        Instantiate(spawnerObject, transform.position, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SpawnGoal()
    {
        //ik weet dat dit heel erg lelijk is.
        Debug.Log("spawngoal works");
        Invoke("SpawnObject", respawnDelay);
    }

    private void SpawnObject()
    {
        Debug.Log("spawn new object works");
        Instantiate(spawnerObject, transform.position, Quaternion.identity);
    }

}