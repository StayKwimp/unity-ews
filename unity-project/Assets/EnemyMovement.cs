using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;


    // patrolling state
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;
    public int maxWalkPointReachTime;
    private int walkPointReachTimer = 0;

    
    // attacking state
    public float timeBetweenAttacks;
    bool alreadyAttacked = false;


    // states
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    // dit is een github test

    private void Awake() {
        // vind de speler
        player = GameObject.Find("PlayerObj").transform;
        agent = GetComponent<NavMeshAgent>();
    }


    void Update()
    {
        // check of de speler in de sight en attack range is
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patrolling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();


        //Debug.Log(string.Format("PlayerInSightRange: {0}, PlayerInAttackRange: {1}", playerInSightRange, playerInAttackRange));
    }



    // state functions


    private void Patrolling() {
        // als er geen punt is waar de enemy heen moet lopen, zoek er dan naar eentje
        if (!walkPointSet) {
            SearchForWalkPoint();
            walkPointReachTimer = maxWalkPointReachTime;
        }

        // als dat wel zo is, loop er heen
        else agent.SetDestination(walkPoint);


        // controleer of je bij de destination bent aangekomen
        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f) walkPointSet = false;



        // als de enemy niet binnen een bepaalde tijd bij de walkpoint is, zoek dan een nieuwe walkpoint
        walkPointReachTimer--;
        if (walkPointReachTimer <= 0) walkPointSet = false;
    }



    private void SearchForWalkPoint() {
        // neem een willekeurige plek binnen een bepaalde range om heen te lopen
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        // controleer of walkpoint binnen de map is
        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround)) walkPointSet = true;
    }




    private void ChasePlayer() {
        // loop naar de speler toe
        agent.SetDestination(player.position);
    }


    private void AttackPlayer() {
        // zorg dat de enemy niet gaat gebewegen door de destination op zijn current position te zetten
        agent.SetDestination(transform.position);

        // we willen niet dat de enemy model naar boven of beneden draait. daarom laten we de y rotation staan op die van de enemy zelf
        // LookAt neemt de twee verschillende positions van de Vector3 en de transform,
        // en returnt daartussen een rotation waarop de enemy naar de speler kijkt.
        // dit doen we niet voor de y rotation (want die is in playerXZPos en transform hetzelfde, dus nul)
        var playerXZPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(playerXZPos);


        if (!alreadyAttacked) {
            // voeg hier nog code toe om de speler aan te vallen


            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack() {
        alreadyAttacked = false;
    }
    
}
