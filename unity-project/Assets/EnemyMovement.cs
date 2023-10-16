using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static EnemyMovement;


public class EnemyMovement : MonoBehaviour
{
    [Header("Definitions")]
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;
    public int health;


    // patrolling state
    [Header("Patrolling State")]
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;
    public int maxWalkPointReachTime;
    private int walkPointReachTimer = 0;

    
    // attacking state
    [Header("Attacking State")]
    public float timeBetweenAttacks;
    public enum Weapon {Bullets, Grenades};
    public Weapon attackType;
    private bool alreadyAttacked = false;

    [Header("Grenades/Bullets")]
    public GameObject projectile;
    public float[] shootForceMul = {1, 1, 1};
    public Transform bulletSpawnpoint;
    public Vector3 targetingPointOffset; // de y component moet negatief zijn als je wil dat hij omhoog richt en positief als je wil dat hij naar beneden richt.

    private Vector3 bulletOrigin;

    [Header("Bullets")]
    public float spread; 
    public float bulletShootForceMul;
    public float[] playerMovementAdjustment = {1, 1};
    public GameObject muzzleFlash;
    public float bulletMaxTime;


    // states
    [Header("Sight/Attack Range")]
    public float leaveAttackingStateRange;
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange, playerInLeaveAttackRange;
    private bool attacking = false;

    [Header("Reticle Coloring")]
    public GameObject reticleGameObject;


    [Header("Sound")]
    public GameObject audioManager;
    

    private void Awake() {
        // vind de speler
        player = GameObject.Find("PlayerObj").transform;
        agent = GetComponent<NavMeshAgent>();
    }


    void Update()
    {
        // check of de speler in de sight en attack range is
        playerInLeaveAttackRange = Physics.CheckSphere(transform.position, leaveAttackingStateRange, whatIsPlayer);
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        // Debug.Log($"playerInSightRange: {playerInSightRange}, playerInAttackRange: {playerInAttackRange}, playerInLeaveAttackRange: {playerInLeaveAttackRange}, attacking: {attacking}");


        if (attacking) {
            attacking = playerInLeaveAttackRange;
        }

        if (!playerInSightRange) {
            Patrolling();
            attacking = false;
        }
        else if (!playerInAttackRange && !playerInLeaveAttackRange) ChasePlayer();

        if ((playerInAttackRange && playerInSightRange) || attacking) {
            AttackPlayer();
            attacking = true;
        }
        
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
            bulletOrigin = bulletSpawnpoint.position;


            if (attackType == Weapon.Grenades) AttackPlayerWithGrenade();
            else if (attackType == Weapon.Bullets) AttackPlayerWithBullet();

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }


    private void AttackPlayerWithGrenade() {
        
        // shoot les granades (amazingk)
        //hier wordt transform.TransformDirection gebruikt om de positie van de bulletOrigin relatief ten op zichte van de enemy te houden
        // Vector3 bulletOriginPosition = transform.position + transform.TransformDirection(bulletOrigin);

        var rb = Instantiate(projectile, bulletOrigin, transform.rotation).GetComponent<Rigidbody>();
        Vector3 distanceToPlayer = player.position - bulletOrigin;
        Vector3 shootForce = new Vector3(distanceToPlayer.x * shootForceMul[0], distanceToPlayer.y * shootForceMul[1] + 5f, distanceToPlayer.z * shootForceMul[2]);



        //hier wordt transform.TransformDirection gebruikt om de vector relatief ten op zichte van de shootForce vector te houden
        rb.AddForce(shootForce - transform.TransformDirection(targetingPointOffset), ForceMode.Impulse);        
    }



    private void AttackPlayerWithBullet() {
        // eerst de player velocity bepalen, en dan nog de movement van de player voorspellen en die erbij doen

        var playerVelocity = player.GetComponentInParent<Rigidbody>().velocity;
        var distanceToPlayer = player.position - bulletOrigin;
        
        // afstand maal een constante wordt de aiming position (een rare vorm van pythagoras i guess)
        var multiplierXZ = distanceToPlayer.magnitude * playerMovementAdjustment[0];
        var multiplierY = distanceToPlayer.magnitude * playerMovementAdjustment[1];

        // Debug.Log(distanceToPlayer.magnitude);
        
        var aimingPosition = player.position + targetingPointOffset + new Vector3(playerVelocity.x * multiplierXZ, playerVelocity.y * multiplierY, playerVelocity.z * multiplierXZ);
        // var aimingPosition = player.position + targetingPointOffset + new Vector3(playerVelocity.x * playerMovementAdjustment[0], playerVelocity.y * playerMovementAdjustment[1], playerVelocity.z * playerMovementAdjustment[0]);

        // vector AB = position B - position A
        Vector3 directionWithoutSpread = aimingPosition - bulletOrigin;


        Debug.DrawLine(bulletOrigin, aimingPosition, Color.green, 2f);

        

        // bullet spread toevoegen
        float xSpread = Random.Range(-spread, spread);
        float ySpread = Random.Range(-spread, spread);


        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(xSpread, ySpread, 0f);


        // bullet spawnen
        GameObject currentBullet = Instantiate(projectile, bulletOrigin, Quaternion.identity);
        
        // draai de bullet
        currentBullet.transform.forward = directionWithSpread.normalized;

        // zorg dat de bullet daadwerkelijk ergens heen vliegt
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * bulletShootForceMul, ForceMode.Impulse);


        // muzzle flash
        if (muzzleFlash != null) {
            Instantiate(muzzleFlash, bulletOrigin, Quaternion.identity);
        }



        alreadyAttacked = true;
        Invoke(nameof(ResetAttack), timeBetweenAttacks);
    }







    private void ResetAttack() {
        alreadyAttacked = false;
    }
    

    public void TakeDamage(int damage) {
        health -= damage;
        if (health <= 0) Invoke("DestroyEnemy", 0.05f);

        // Debug.Log("health: " + health.ToString());

        
        // werkt nog niet, te lui (en momenteel te insignificant) om te fixen :)
        // audioManager.GetComponent<AudioManager>().Play("bullet hit");
    }

    private void DestroyEnemy() {
        Destroy(gameObject);

        // voeg een kill toe
        GameObject.Find("Player").GetComponent<PlayerMovement>().kills += 1;

        // flash de reticle
        reticleGameObject.GetComponent<ReticleColor>().FlashReticleColor();
    }
}
