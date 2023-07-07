using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MogusExplosiveBullet : MonoBehaviour
{
    [Header("Bullet setup")]
    public Rigidbody rb;
    public GameObject explosion;
    public LayerMask whatIsEnemies;
    public LayerMask whatIsPlayer;


    [Header("Properties")]
    [Range(0f, 1f)]
    public float bounciness;
    public bool useGravity;

    [Header("Bullet stats")]
    public int explosionDamage;
    public float explosionRange;
    public float explosionForce;

    public int maxCollisions;
    public float maxLifetime;
    public bool explodeOnTouch = true;

    private int collisions;
    private PhysicMaterial physics_mat;

    private bool alreadyExploding = false;


    private void Start() {
        Setup();
    }


    private void Update() {
        // explodeer als je meer collisions hebt dan de max collisions
        if (collisions > maxCollisions) Explode();

        // ... of als je lifetime om is
        maxLifetime -= Time.deltaTime;
        if (maxLifetime <= 0) Explode();

        // Debug.Log(string.Format("Bullet velocity: x: {0}, y: {1}, z: {2}", rb.velocity.x, rb.velocity.y, rb.velocity.z));
    }

    private void Explode() {
        if (!alreadyExploding) {
            if (explosion != null) Instantiate(explosion, transform.position, Quaternion.identity);


            // check for enemies in de range van de explosion
            Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRange, whatIsEnemies);

            for (int i = 0; i < enemies.Length; i++) {
                // verkrijg de script component van de enemy en voer de functie TakeDamage erop uit
                // Debug.Log(enemies[i].ToString());
                
                try {
                    enemies[i].GetComponent<EnemyMovement>().TakeDamage(explosionDamage);
                } catch (System.Exception error) {
                    Debug.LogWarning(string.Format("{0} Ignoring...", error));
                }

                // lè explosion force (enemies hebben voor nu geen rigidbody dus dit doet momenteel niks)
                if (enemies[i].GetComponent<Rigidbody>()) {
                    enemies[i].GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRange, explosionForce * 0.1f);
                }
            }







            // check for players in de range van de explosion
            Collider[] players = Physics.OverlapSphere(transform.position, explosionRange, whatIsPlayer);

            for (int i = 0; i < players.Length; i++) {
                // verkrijg de script component van de player en voer de functie TakeDamage erop uit
                // Debug.Log(players[i].ToString());
                
                try {
                    players[i].GetComponentInParent<PlayerMovement>().TakeDamage(explosionDamage);
                } catch (System.Exception error) {
                    Debug.LogWarning(string.Format("{0} Ignoring...", error));
                }

                
                if (players[i].GetComponent<Rigidbody>()) {
                    players[i].GetComponent<Rigidbody>().AddExplosionForce(explosionForce * 0.5f, transform.position, explosionRange, explosionForce * 0.1f);
                }
            }


            alreadyExploding = true;

            // sloop de bullet wat later om bugs te voorkomen
            Invoke("DestroyBullet", 0.05f);
        }
    }


    private void DestroyBullet() {
        Destroy(gameObject);
    }

    // deze functie wordt uitgevoerd als de bullet met iets gaat colliden
    private void OnCollisionEnter(Collision collision) {
        collisions++;

        // explode als de bullet de player raakt
        if (collision.collider.CompareTag("Player") && explodeOnTouch) {
            Explode();
        }
    }

    private void Setup() {
        // maak een nieuwe physic material
        physics_mat = new PhysicMaterial();
        physics_mat.bounciness = bounciness;
        physics_mat.frictionCombine = PhysicMaterialCombine.Minimum;
        physics_mat.bounceCombine = PhysicMaterialCombine.Maximum;

        // assign die aan de bullet collider
        GetComponent<SphereCollider>().material = physics_mat;


        rb.useGravity = useGravity;
    }



    // tijdelijk, laat de explosion range zien van de bullet als je m selecteert in unity
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}
