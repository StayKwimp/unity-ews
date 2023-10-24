using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerBullet;
using static PlayerMovement;
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
    private bool shooting, readyToShoot;
    public bool reloading;


    // camera en attack reference point
    [Header("Camera")]
    public Camera playerCam;
    public PlayerCam[] camerasForRecoil;

    [Header("References")]
    public Transform attackPoint;
    public PlayerBullet bulletScr;
    public string enemyTag;
    public PlayerMovement playerScr;

    public float bulletMaxTime;


    // graphics
    [Header("Graphics")]
    public GameObject muzzleFlash;
    public TextMeshProUGUI ammoDisplay;

    [Header("FOV")]
    public float mainFOV;
    public float FOVOnADS;
    private int FOVAnimateDirection;
    private float currentFov;


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


    [Header("Grenades")]
    public Vector3 grenadeGunPosition;
    public float hideGunAnimationTime;
    private bool throwingGrenade;
    private float grenadeThrowAnimationProgress;
    private Vector3 gunGrenadeSwitchDirection;
    private bool noADSAnimation = false;

    private bool allowEndGrenadeAnimation;
    private bool allowEndADSAnimation;

    [Header("Recoil")]
    public float recoilOnHipfire;
    public float recoilOnADS;
    public float recoilMultiplierOnCrouching;
    


    [Header("Sounds")]
    public GameObject audioManager;


    [Header("Debug")]
    public bool allowInvoke = true;
    public GameObject raycastHitMarker;


    private void Awake() {
        // vul het magazijn
        bulletsLeft = magazineSize;
        readyToShoot = true;

        currentFov = mainFOV;

        // zet de ads animation progress op een groot getal, zodat de animatie niet gaat spelen bij de game start
        timeBetweenLastADSSwitch = 300f;

        
        
        if (switchGunPosAnimationTime <= 0) {
            Debug.LogError("PlayerGun: switchGunPosAnimationTime may not be 0 or smaller than 0");
            UnityEditor.EditorApplication.isPlaying = false;
            Application.Quit();
        }
    }


    private void Update() {
        // Debug.Log($"Position: {transform.position}, ADSEnabled: {ADSEnabled}, throwingGrenade: {throwingGrenade}");

        // verkrijg info over of de player een granaat aan het gooien is
        throwingGrenade = playerScr.throwingGrenade;
        // en ook voor hoe lang
        grenadeThrowAnimationProgress = playerScr.grenadeAnimationProgress;




        MyInput();

        AnimateGun();


        // update camera fov
        playerCam.fieldOfView = currentFov;

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
        if (Input.GetKeyDown(reloadKey) && bulletsLeft < magazineSize && !reloading && !throwingGrenade) Reload();

        // ga automatisch reloaden als je geen kogels meer over hebt
        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0 && !throwingGrenade) Reload();



        // ADS
        timeBetweenLastADSSwitch += Time.deltaTime;


        var ADSOneFrameEarlier = ADSEnabled;

        ADSEnabled = Input.GetKey(ADSKey);

        // je kan geen ADS gebruiken tijdens reloads of granaten gooien
        if (reloading || throwingGrenade) ADSEnabled = false;
        

        // als de ADS deze frame geswitcht is, zet de time between last ads switch op nul (voor animatie)
        if (ADSOneFrameEarlier != ADSEnabled) {
            timeBetweenLastADSSwitch = 0f;
            originalGunPosition = transform.localPosition;

            // bepaal de ADS gun switch (en fov switch) direction
            // als ADS aan is, is de direction naar de ADS gun position, als ADS uit is, is de direction de andere kant op naar de initial gun position
            if (ADSEnabled) {
                gunSwitchDirection = ADSGunPosition - originalGunPosition;
                FOVAnimateDirection = -1;
            } else {
                gunSwitchDirection = initialGunPosition - originalGunPosition;
                FOVAnimateDirection = 1;
            }

        }
        



        // daadwerkelijk schieten
        // je mag niet schieten tijdens het switchen van aiming mode (hip fire en ADS)
        // en ook niet als je een granaat aan het gooien bent
        // en als de animatie van het grenade terugdoen nog speelt
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0 && !movingGun && !throwingGrenade) {
            bulletsShot = 0;

            Shoot();
        }


        // granaedae
        // aan het begin van de animatie is de direction naar de grenade gun position (dus naar beneden)
        if (throwingGrenade) {
            gunGrenadeSwitchDirection = grenadeGunPosition - transform.localPosition;
        }
        // aan het einde is het juist andersom
        else {
            gunGrenadeSwitchDirection = initialGunPosition - grenadeGunPosition;
        }

        
    }


    private void AnimateGun() {
        // Animation Handler


        // eerst de grenade animatie
        if (grenadeThrowAnimationProgress <= hideGunAnimationTime || (/*deze is voor als de granaat al gegooid is*/ grenadeThrowAnimationProgress > playerScr.animationStopTime && grenadeThrowAnimationProgress <= (hideGunAnimationTime + playerScr.animationStopTime - 0.051f))) {
            movingGun = true;
            // zorg dat er geen beweging komt bij het ADS deel
            noADSAnimation = true;

            // beweeg
            transform.localPosition += gunGrenadeSwitchDirection * (Time.deltaTime / switchGunPosAnimationTime);
            allowEndGrenadeAnimation = true;
        } else if (allowEndGrenadeAnimation) {
            Debug.Log("Ended Grenade Animation");

            movingGun = false;
            // zet de gun op de correcte positie als de animatie afgelopen is
            if (throwingGrenade) transform.localPosition = grenadeGunPosition;
            else transform.localPosition = initialGunPosition;

            allowEndGrenadeAnimation = false;
            noADSAnimation = false;
            
        }


        // daarna ADS
        // check of er zojuist ADS aan of uit gezet is
        // ADS mag geen animatie doen als de grenade animatie al afgespeeld wordt
        if (timeBetweenLastADSSwitch <= switchGunPosAnimationTime - 0.017f && !noADSAnimation) {
            movingGun = true;
            
            // beweeg
            
            transform.localPosition += gunSwitchDirection * (Time.deltaTime / switchGunPosAnimationTime);

            // set the playercam fov
            var fovDelta = mainFOV - FOVOnADS;
            currentFov += fovDelta * (Time.deltaTime / switchGunPosAnimationTime) * FOVAnimateDirection;

            allowEndADSAnimation = true;
        } else if (!noADSAnimation) {
            // als de animatie afgelopen is, zorg dan dat de gun voor één keer op de goede plek wordt gezet
            if (allowEndADSAnimation) {
                Debug.Log("Ended ADS Animation");
                // zet de gun (en de fov) op de correcte positie als de animatie afgelopen is
                movingGun = false;
                if (ADSEnabled) {
                    transform.localPosition = ADSGunPosition;
                    currentFov = FOVOnADS;
                } else {
                    transform.localPosition = initialGunPosition;
                    currentFov = mainFOV;
                }

                allowEndADSAnimation = false;
            }
            
        // om te voorkomen dat de gun ineens naar een positie gaat waar hij niet heen hoort te gaan
        // mag de gun geen ADS animatie afmaken als de grenade animatie loopt
        } else if (noADSAnimation) {
            allowEndADSAnimation = false;
            currentFov = mainFOV;
        }
    }
    


    private void ApplyRecoil() {
        // bepaal het aantal recoil
        var recoilAmount = 0f;
        if (ADSEnabled) recoilAmount = recoilOnADS;
        else recoilAmount = recoilOnHipfire;

        // verander de recoil amount als de player aan het crouchen is
        if (playerScr.state == PlayerMovement.movementState.crouching) recoilAmount *= recoilMultiplierOnCrouching;

        // apply de recoil op elke camera
        for (int i = 0; i < camerasForRecoil.Length; i++) {
            Debug.Log($"Apply recoil amount {recoilAmount} on camera {camerasForRecoil[i]}");
            camerasForRecoil[i].SmoothRotateCameraInit(0f, recoilAmount, 1.08f);
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
            // Debug.DrawRay(playerCam.transform.position, camDirWithSpread, Color.red, 6f);
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


        // invoke de resetShot functie (als dat al niet gebeurd is)
        if (allowInvoke) {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;
        }


        // als er meer dan één bullet per tap is, herhaal deze functie dan (bijv. shotguns hebben meer dan 1 bullet)
        if (bulletsShot < bulletsPerTap && bulletsLeft > 0) Invoke("Shoot", timeBetweenShots);


        // apply recoil
        ApplyRecoil();

        // speel geluid af
        PlaySound("AK fire");
    }


    public void PlaySound(string name) {
        Debug.Log($"Play sound: {name}");
        audioManager.GetComponent<AudioManager>().Play(name);
    }

    private void ResetShot() {
        // allow shooting and invoking again
        readyToShoot = true;
        allowInvoke = true;
    }

    private void Reload() {
        reloading = true;

        
        Invoke("ReloadFinished", reloadTime);

        // speel geluid af
        PlaySound("AK reload");
    }

    private void ReloadFinished() {
        bulletsLeft = magazineSize;
        reloading = false;
    }
}
