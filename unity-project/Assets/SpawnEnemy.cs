using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpawnEnemy : MonoBehaviour
{
    [Header("Enemies")]
    public GameObject[] enemyPrefabs;

    [Header("Navmesh")]
    public float maxNavmeshFindRange;





    public bool SpawnEnemyFunc() {
        var enemyPrefabsListInt = Random.Range(0, enemyPrefabs.Length);


        // vind de dichtbijzijnste plek op de navmesh
        NavMeshHit nearestNavmeshPos;
        if (NavMesh.SamplePosition(transform.position, out nearestNavmeshPos, maxNavmeshFindRange, NavMesh.AllAreas)) {


            var spawnedObj = Instantiate(enemyPrefabs[enemyPrefabsListInt], nearestNavmeshPos.position, Quaternion.identity);

            if (spawnedObj != null) return true;
            else return false;
        }
        else return false;
    }



    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, maxNavmeshFindRange);
    }
}
