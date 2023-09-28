using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnController : MonoBehaviour
{
    [Header("Spawnpoints")]
    public GameObject[] spawnerObjects;
    public int maxEnemyAmount;
    public int enemySpawnTimer;

    [Header("Enemies")]
    public string enemyTag;
    public int instancesToSpawnOnGameStart;


    private int enemyCount = 0;
    private List<float> spawnQueueTimers = new List<float> {};


    void Start()
    {
        for (var i = 0; i < instancesToSpawnOnGameStart; i++) {
            SpawnEnemy();
        }
    }

    
    void Update()
    {
        enemyCount = GetEnemyCount();

        var enemiesMissing = maxEnemyAmount - enemyCount - spawnQueueTimers.Count;


        if (enemiesMissing > 0) {
            // als het aantal enemies kleiner is dan de enemies op het speelveld plus degene in de spawn queue
            
            var spawnSpeed = Mathf.Min(enemySpawnTimer / (0.4f * (enemiesMissing + 1f)), enemySpawnTimer);
            spawnQueueTimers.Add(spawnSpeed);
        }



        for (var i = 0; i < spawnQueueTimers.Count; i++) {
            spawnQueueTimers[i] -= Time.deltaTime;

            // spawn een enemy als de tijd om is
            if (spawnQueueTimers[i] <= 0) {

                var success = SpawnEnemy();
                
                // verwijder de timer als de enemy succesvol gespawned is
                if (success) spawnQueueTimers.RemoveAt(i);
            }
        }
        
    }


    private int GetEnemyCount() {
        // vind alle game objects met de tag enemyTag
        var enemyArr = GameObject.FindGameObjectsWithTag(enemyTag);
        // return de lengte van de array
        return enemyArr.Length;
    }

    private bool SpawnEnemy() {
        // kies een willekeurige spawner en spawn daar een enemy
        var spawnerListInt = Random.Range(0, spawnerObjects.Length);

        var success = spawnerObjects[spawnerListInt].GetComponent<SpawnEnemy>().SpawnEnemyFunc();
        return success;
    }
}
