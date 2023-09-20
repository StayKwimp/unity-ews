using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bluemogus_bulletscript : MonoBehaviour
{

    [Header("Bullet setup")]
    public LayerMask whatIsEnemies;
    public LayerMask whatIsBullet;

    [Header("Bullet stats")]
    public int bulletDamage;
    public float bulletLifetime;



    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        bulletLifetime -= Time.deltaTime;
        if(bulletLifetime <= 0){
            Destroy(gameObject);
        }
        
    }


private void OnCollisionEnter(Collision collision) {

    if (collision.collider.CompareTag("Player")) {
        collision.gameObject.GetComponentInParent<PlayerMovement>().TakeDamage(bulletDamage);
        }

 Destroy(gameObject);


}


}
