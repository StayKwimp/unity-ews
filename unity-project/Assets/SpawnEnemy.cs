using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpawnEnemy : MonoBehaviour
{
    [Header("Enemies")]
    public GameObject[] enemyPrefabs;

    [Header("Spawning")]
    public float maxSpawnRadius;
    public float maxNavmeshFindRange;
    public float minPlayerDistance = 10f; 

    private GameObject playerObj; 


    private void Start() {
        // define player object
        playerObj = GameObject.Find("Player");
    }





    public bool SpawnEnemyFunc() {
        // check of de player niet te dicht bij de spawner is (zodat enemies niet in je gezicht spawnen)
        Vector3 playerPos = playerObj.transform.position;
        Vector3 vectorDistanceToPlayer = playerPos - transform.position;
        float distanceToPlayer = vectorDistanceToPlayer.magnitude;

        if (distanceToPlayer <= minPlayerDistance) {
            Debug.Log("stopped mogus spawn, player too close");
            return false;
        }



        // pak een random enemy uit de enemy list
        var enemyPrefabsListInt = Random.Range(0, enemyPrefabs.Length);

        var spawnX = transform.position.x + Random.Range(-maxSpawnRadius, maxSpawnRadius);
        var spawnZ = transform.position.z + Random.Range(-maxSpawnRadius, maxSpawnRadius);

        var spawnPos = new Vector3(spawnX, transform.position.y, spawnZ);


        // vind de dichtbijzijnste plek op de navmesh
        NavMeshHit nearestNavmeshPos;
        if (NavMesh.SamplePosition(spawnPos, out nearestNavmeshPos, maxNavmeshFindRange, NavMesh.AllAreas)) {

            // als een dichtbijzijnste plek is gevonden binnen de navmesh find range, zet dan een enemy object op die position neer
            var spawnedObj = Instantiate(enemyPrefabs[enemyPrefabsListInt], nearestNavmeshPos.position, Quaternion.identity);

            // return spawnedObj
            if (spawnedObj != null) return true;
            else return false;
        }
        else return false;
    }



    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, maxNavmeshFindRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxSpawnRadius);
    }
}
