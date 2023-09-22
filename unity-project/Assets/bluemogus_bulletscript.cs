using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bluemogus_bulletscript : MonoBehaviour
{

    [Header("Bullet setup")]
    public LayerMask whatIsEnemies;
    public LayerMask whatIsBullet;
    public LayerMask whatIsPlayer;

    [Header("Bullet stats")]
    public int bulletDamage;
    public float bulletLifetime;

    private float raycastLength = 1f;


    // called on a fixed interval
    void FixedUpdate() {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, raycastLength, whatIsPlayer)) {
            hit.transform.gameObject.GetComponentInParent<PlayerMovement>().TakeDamage(bulletDamage);

            Invoke(nameof(DestroyBullet), 0.05f);
        }

    }


    void Update()
    {
        bulletLifetime -= Time.deltaTime;
        if(bulletLifetime <= 0){
            Invoke(nameof(DestroyBullet), 0.05f);
        }
        
    }


    private void OnCollisionEnter(Collision collision) {

        // if (collision.collider.CompareTag("Player")) {
        //     collision.gameObject.GetComponentInParent<PlayerMovement>().TakeDamage(bulletDamage);
        // }

        Invoke(nameof(DestroyBullet), 0.05f);
    }

    private void DestroyBullet() {
        // dit om bugs te voorkomen
        Destroy(gameObject);
    }
}
