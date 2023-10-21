using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrenade : MonoBehaviour
{

    [Header("Grenade setup")]
    public Rigidbody rb;
    public GameObject explosion;
    public LayerMask whatIsEnemies;
    public LayerMask whatIsPlayer;
    public LayerMask whatIsGround;

    [Header("Properties")]
    [Range(0f, 1f)]
    public float bounciness;
    public bool useGravity = true;
    public float groundDrag = 0f;
    public float grenadeHeight;
    private bool grounded;

    [Header("Grenade stats")]
    public int explosionDamage;
    public float highDamageExplosionRange;
    public float explosionRange;
    public float explosionForce;

    public bool explodeOnTouch = false;

    private int collisions;
    private PhysicMaterial physicsMat;

    private bool alreadyExploding = false;


    void Start() {
        // maak een nieuwe physic material
        physicsMat = new PhysicMaterial();
        physicsMat.bounciness = bounciness;
        physicsMat.frictionCombine = PhysicMaterialCombine.Minimum;
        physicsMat.bounceCombine = PhysicMaterialCombine.Maximum;

        // assign die aan de bullet collider
        GetComponent<SphereCollider>().material = physicsMat;


        rb.useGravity = useGravity;
    }

    void Update() {
        // controleer of de granaat de grond aanraakt
        grounded = Physics.Raycast(transform.position, Vector3.down, grenadeHeight * 0.5f + 0.2f, whatIsGround);


        // zo ja, zet de drag gelijk aan de ground drag (waardoor de granaat verlangzaamt als het de grond aanraakt)
        if (grounded) {
            rb.drag = groundDrag;
        } else {
            rb.drag = 0;
        }
    }


    void Explode() {
        if (!alreadyExploding) {
            // spawn een explosion object
            if (explosion != null) Instantiate(explosion, transform.position, Quaternion.identity);


            // check for enemies in de range van de explosion
            Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRange, whatIsEnemies);

            for (int i = 0; i < enemies.Length; i++) {
                
                
                try {

                    // check of de enemy binnen de high damage exposion range zit, om zo meer damage te doen
                    var distanceBetweenEnemy = Vector3.Distance(transform.position, enemies[i].transform.position);
                    
                    if (distanceBetweenEnemy <= highDamageExplosionRange) {
                        // verkrijg de script component van de enemy en voer de functie TakeDamage erop uit
                        enemies[i].GetComponent<EnemyMovement>().TakeDamage(explosionDamage);
                    } else {
                        // doe hetzelfde, maar dan nu met 1/3 van de normale damage
                        enemies[i].GetComponent<EnemyMovement>().TakeDamage((int)Mathf.Round(explosionDamage * 0.33f));
                    }

                    
                } catch (System.Exception error) {
                    Debug.LogWarning($"Affected enemy '{enemies[i]}' inside explosion range didn't take any damage. \nError: {error}");
                }

                // lè explosion force (enemies hebben voor nu geen rigidbody dus dit doet momenteel niks)
                if (enemies[i].GetComponent<Rigidbody>()) {
                    enemies[i].GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRange, explosionForce * 0.1f);
                }
            }







            // check for players in de range van de explosion
            Collider[] players = Physics.OverlapSphere(transform.position, explosionRange, whatIsPlayer);

            for (int i = 0; i < players.Length; i++) {
                
                
                try {

                    var distanceBetweenPlayer = Vector3.Distance(transform.position, players[i].transform.position);

                    if (distanceBetweenPlayer <= highDamageExplosionRange) {
                        // verkrijg de script component van de player en voer de functie TakeDamage erop uit
                        players[i].GetComponentInParent<PlayerMovement>().TakeDamage(explosionDamage);
                    } else {
                        // hetzelfde, maar dan nu met 1/3 van de damage
                        players[i].GetComponentInParent<PlayerMovement>().TakeDamage((int)Mathf.Round(explosionDamage * 0.33f));
                    }
                    
                } catch (System.Exception error) {
                    Debug.LogWarning($"Player {players[i]} didn't take damage from a grenade. \nError: {error}");
                }

                
                if (players[i].GetComponent<Rigidbody>()) {
                    players[i].GetComponent<Rigidbody>().AddExplosionForce(explosionForce * 0.5f, transform.position, explosionRange, explosionForce * 0.1f);
                }
            }


            alreadyExploding = true;

            // sloop de bullet wat later om bugs te voorkomen
            Invoke("DestroyGrenade", 0.05f);
        }
    }

    public void StartFuse(float fuseLength) {
        // explode na een paar seconden
        Invoke(nameof(Explode), fuseLength);
    }


    public void DestroyGrenade() {
        Destroy(gameObject);
    }



    // laat de explosion ranges zien
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, highDamageExplosionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}
