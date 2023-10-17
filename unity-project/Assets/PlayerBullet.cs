using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [Header("Bullet setup")]
    public Rigidbody rb;
    public LayerMask whatIsEnemies;


    [Header("Properties")]
    [Range(0f, 1f)]
    public float bounciness;
    public bool useGravity;

    [Header("Bullet stats")]
    public int damage;

    public int maxCollisions;
    public bool destroyOnTouch = true;

    private int collisions;
    private PhysicMaterial physics_mat;


    private void Start() {
        Setup();
    }


    private void Update() {
        // destroy als je meer collisions hebt dan de max collisions
        if (collisions > maxCollisions) {
            DestroyBullet();
        }
    }

    private void DestroyBullet() {
        Destroy(gameObject);
    }

    public void DestroyBulletTimed(float timeToDestroy) {
        Invoke("DestroyBullet", timeToDestroy);
    }

    // deze functie wordt uitgevoerd als de bullet met iets gaat colliden
    private void OnCollisionEnter(Collision collision) {
        collisions++;

        // destroy de bullet als het een enemy raakt
        if (collision.collider.CompareTag("Enemy")) {
            // collision.collider.GetComponent<EnemyMovement>().TakeDamage(damage);
            Invoke("DestroyBullet", 0.02f);
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
}
