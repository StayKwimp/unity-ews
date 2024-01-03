using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class goalScript : MonoBehaviour
{

    [Header("Health settings")]
    public int healthAddition;
    public int healthRemoval;
    public float goalDamageTimer;
    private float timePassed = 0f;
    public string objectType;

    [Header("TMPro")]
    public string countdownDisplayName;
    private TextMeshProUGUI countdownDisplay;

    private GameObject playerObj;
    private GameObject goalSpawner1;
    private GameObject goalSpawner2;

    
    // Start is called before the first frame update
    void Awake() {
        // playerObj = GameObject.Find("Player").GetComponent<PlayerMovement>();
    }


    void Start()
    {
        countdownDisplay = GameObject.Find(countdownDisplayName).GetComponent<TextMeshProUGUI>();
        Invoke("Lowermaxhealth", goalDamageTimer);
    }

    // Update is called once per frame
    void Update()
    {
        timePassed += Time.deltaTime;

        float timeRemaining = goalDamageTimer - timePassed;
        if (countdownDisplay != null){
            countdownDisplay.SetText($"Time remaining: {(int)timeRemaining}s");
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if(GameObject.Find("Player").GetComponent<PlayerMovement>().maxHealth < 200)
            {
                GameObject.Find("Player").GetComponent<PlayerMovement>().maxHealth += healthAddition;
                if(objectType == "goal")
                    {
                        GameObject.Find("GoalSpawner")?.GetComponent<goalSpawner>()?.SpawnGoal();
                    }
                    else if(objectType == "goal1")
                    {
                        GameObject.Find("GoalSpawner2")?.GetComponent<goalSpawner>()?.SpawnGoal();
                    }
            }
            
            Invoke("DestroySelf", 0.1f);
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
        
        Invoke("DestroySelf", 0.1f);
    }


    private void DestroySelf() {
        Destroy(gameObject);
    }
}
