using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class goalScript : MonoBehaviour
{

    [Header("Health settings")]
    public int healthAddition;
    public int healthRemoval;
    public float goalDamageTimer;
    public string objectType;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("Lowermaxhealth", goalDamageTimer);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if(GameObject.Find("Player").GetComponent<PlayerMovement>().maxHealth < 200)
            {
                GameObject.Find("Player").GetComponent<PlayerMovement>().maxHealth += healthAddition;
            }
            Destroy(gameObject);
        }
    }
    
    void Lowermaxhealth()
    {
        Debug.Log("lowermaxhealth runs");
         GameObject.Find("Player").GetComponent<PlayerMovement>().maxHealth -= healthRemoval;
         if(objectType == "goal")
         {
            GameObject.Find("GoalSpawner")?.GetComponent<goalSpawner>()?.SpawnGoal();
         }
         else if(objectType == "goal1")
         {
            GameObject.Find("GoalSpawner2")?.GetComponent<goalSpawner>()?.SpawnGoal();
         }
         Destroy(gameObject);
    }
}
