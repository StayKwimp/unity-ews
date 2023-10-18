using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerBullet;
using TMPro;

public class PlayerGun : MonoBehaviour
{
    [Header("Bullets")]
    // bullet
    public GameObject bullet;

    // bullet force
    public float shootForce, upwardForce;

    // gun stats
    [Header("Gun Stats")]
    public float timeBetweenShooting, spread, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;

    private int bulletsLeft, bulletsShot;


    // bools
    private bool shooting, readyToShoot, reloading;


    // camera en attack reference point
    [Header("Camera")]
    public Camera playerCam;
    public PlayerCam[] camerasForRecoil;

    [Header("Reference points")]
    public Transform attackPoint;
    public PlayerBullet bulletScr;
    public string enemyTag;

    public float bulletMaxTime;


    // graphics
    [Header("Graphics")]
    public GameObject muzzleFlash;
    public TextMeshProUGUI ammoDisplay;


    [Header("Controls")]
    public KeyCode fireKey = KeyCode.Mouse0;
    public KeyCode ADSKey = KeyCode.Mouse1;
    public KeyCode reloadKey;

    [Header("ADS")]
    public Vector3 initialGunPosition;
    public Vector3 ADSGunPosition;
    public float switchGunPosAnimationTime;
    public float spreadOnADS;

    private Vector3 gunSwitchDirection;
    private Vector3 originalGunPosition;

    public bool ADSEnabled;
    private float timeBetweenLastADSSwitch;
    private bool movingGun;

    [Header("Recoil")]
    public float recoilOnHipfire;
    public float recoilOnADS;
    


    [Header("Sounds")]
    public GameObject audioManager;


    [Header("Debug")]
    // bugfixing (holie shid)
    public bool allowInvoke = true;
    public GameObject raycastHitMarker;


    private void Awake() {
        // vul het magazijn
        bulletsLeft = magazineSize;
        readyToShoot = true;

        
        
        if (switchGunPosAnimationTime <= 0) {
            Debug.LogError("PlayerGun: switchGunPosAnimationTime may not be 0 or smaller than 0");
            UnityEditor.EditorApplication.isPlaying = false;
            Application.Quit();
        }
    }


    private void Update() {
        MyInput();

        AnimateGun();

        // set ammo display if it exists
        if (ammoDisplay != null) {
            if (!reloading) ammoDisplay.SetText(bulletsLeft / bulletsPerTap + " / " + magazineSize / bulletsPerTap);
            else ammoDisplay.SetText("Reloading!");
        }
    }


    private void MyInput() {
        // kijk of je de fire button ingedrukt kan houden om te schieten
        if (allowButtonHold) shooting = Input.GetKey(fireKey);
        else shooting = Input.GetKeyDown(fireKey);

        // reloading
        if (Input.GetKeyDown(reloadKey) && bulletsLeft < magazineSize && !reloading) Reload();

        // ga automatisch reloaden als je geen kogels meer over hebt
        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0) Reload();



        // ADS
        timeBetweenLastADSSwitch += Time.deltaTime;


        var ADSOneFrameEarlier = ADSEnabled;

        ADSEnabled = Input.GetKey(ADSKey);

        // je kan geen ADS gebruiken tijdens reloads
        if (reloading) ADSEnabled = false;
        

        // als de ADS deze frame geswitcht is, zet de time between last ads switch op nul (voor animatie)
        if (ADSOneFrameEarlier != ADSEnabled) {
            timeBetweenLastADSSwitch = 0f;
            originalGunPosition = transform.localPosition;

            // bepaal de ADS gun switch direction
            // als ADS aan is, is de direction naar de ADS gun position, als ADS uit is, is de direction de andere kant op naar de initial gun position
            if (ADSEnabled) gunSwitchDirection = ADSGunPosition - originalGunPosition;
            else gunSwitchDirection = initialGunPosition - originalGunPosition;
        }
        



        // daadwerkelijk schieten
        // je mag niet schieten tijdens het switchen van aiming mode (hip fire en ADS)
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0 && !movingGun) {
            bulletsShot = 0;

            Shoot();
        }

        
    }


    private void AnimateGun() {
        // ADS animation handler

        // check of zojuist ADS uit of aan is gezet
        if (timeBetweenLastADSSwitch <= switchGunPosAnimationTime) {
            movingGun = true;
            var animationProgressPct = timeBetweenLastADSSwitch / switchGunPosAnimationTime;

            // beweeg de gun, de direction wordt al bepaald in MyInput()
            transform.localPosition = originalGunPosition + (gunSwitchDirection * animationProgressPct);
        } else {
            movingGun = false;
            if (ADSEnabled) transform.localPosition = ADSGunPosition;
            else transform.localPosition = initialGunPosition;
        }

    }
    

    private void ApplyRecoil() {
        // bepaal het aantal recoil
        var recoilAmount = 0f;
        if (ADSEnabled) recoilAmount = recoilOnADS;
        else recoilAmount = recoilOnHipfire;

        // apply de recoil op elke camera
        for (int i = 0; i < camerasForRecoil.Length; i++) {
            // Debug.Log($"Apply recoil amount {recoilAmount} on camera {camerasForRecoil[i]}");
            camerasForRecoil[i].RotateCamera(0f, recoilAmount);
        }
    }


    private void Shoot() {
        readyToShoot = false;


        // vind de plaats waar de kogel iets gaat raken dmv een raycast
        // de raycast gaat door het midden van de camera view en gaat dan met een loodrechte lijn op het oppervlak van de camera
        // (dus parallel aan jouw pov)
        Ray ray = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // controleer of de ray iets raakt
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit)) targetPoint = hit.point;
        // als je niks raakt schiet je in de lucht
        else targetPoint = ray.GetPoint(75);




        // de direction van attackpoint naar targetpoint
        // de vector van A naar B is positie B - positie A
        Vector3 directionWithoutSpread = targetPoint - attackPoint.position;

        // bullet spread
        float xSpread = 0f;
        float ySpread = 0f;
        if (ADSEnabled) {
            // gebruik een andere spread bij ADS
            xSpread = UnityEngine.Random.Range(-spreadOnADS, spreadOnADS);
            ySpread = UnityEngine.Random.Range(-spreadOnADS, spreadOnADS);
        } else {
            xSpread = UnityEngine.Random.Range(-spread, spread);
            ySpread = UnityEngine.Random.Range(-spread, spread);
        }


        // nieuwe direction met spread
        Vector3 totalSpread = new Vector3(xSpread, ySpread, 0);
        Vector3 directionWithSpread = directionWithoutSpread + totalSpread;


        // nu hetzelfde met de camera
        Vector3 camDirWithoutSpread = targetPoint - playerCam.transform.position;
        Vector3 camDirWithSpread = camDirWithoutSpread + totalSpread;


        // raycast die met spread vanuit de camera position naar de enemy gaat
        RaycastHit enemyHit;
        if (Physics.Raycast(playerCam.transform.position, camDirWithSpread, out enemyHit, Mathf.Infinity)) {

        

            // debug
            Debug.DrawRay(playerCam.transform.position, camDirWithSpread, Color.red, 6f);
            Instantiate(raycastHitMarker, enemyHit.point, Quaternion.identity);

            try {
                if (enemyHit.collider.CompareTag(enemyTag)) {
                    var bulletDamage = bulletScr.damage;
                    enemyHit.collider.GetComponentInParent<EnemyMovement>().TakeDamage(bulletDamage);
                }
                
            } catch (Exception e) {
                Debug.LogWarning($"Fired gun but an error occured! Most likely the raycast hit something that doesn't have a collider. (Collider: {enemyHit.collider}) \nIf not, the collider with tag '{enemyTag}' it hit doesn't have the EnemyMovement component in either itself or its parent(s). \nException: {e}");
            }
        }



        // spawn lè bullet au your mom
        GameObject currentBullet = Instantiate(bullet, attackPoint.position, Quaternion.identity);

        // rotate de bullet
        currentBullet.transform.forward = directionWithSpread.normalized;

        // voeg krachten toe aan de bullet
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootForce, ForceMode.Impulse);
        // nog upward forces toevoegen (alleen voor grenades)
        currentBullet.GetComponent<Rigidbody>().AddForce(playerCam.transform.up * upwardForce, ForceMode.Impulse);


        currentBullet.GetComponent<PlayerBullet>().DestroyBulletTimed(bulletMaxTime);



        // funny muzzle flash
        if (muzzleFlash != null) {
            Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);
        }


        bulletsLeft--;
        bulletsShot++;

        // speel geluid af
        PlaySound("AK fire");


        // invoke de resetShot functie (als dat al niet gebeurd is)
        if (allowInvoke) {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;
        }


        // als er meer dan één bullet per tap is, herhaal deze functie dan (bijv. shotguns hebben meer dan 1 bullet)
        if (bulletsShot < bulletsPerTap && bulletsLeft > 0) Invoke("Shoot", timeBetweenShots);


        // apply recoil
        ApplyRecoil();
    }


    public void PlaySound(string name) {
        // Debug.Log($"Play sound: {name}");
        audioManager.GetComponent<AudioManager>().Play(name);
    }

    private void ResetShot() {
        // allow shooting and invoking again
        readyToShoot = true;
        allowInvoke = true;
    }

    private void Reload() {
        reloading = true;

        // speel geluid af
        PlaySound("AK reload");
        Invoke("ReloadFinished", reloadTime);
    }

    private void ReloadFinished() {
        bulletsLeft = magazineSize;
        reloading = false;
    }
}
