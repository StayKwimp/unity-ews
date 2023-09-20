using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerGun : MonoBehaviour
{
    // bullet
    public GameObject bullet;

    // bullet force
    public float shootForce, upwardForce;

    // gun stats
    public float timeBetweenShooting, spread, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;

    private int bulletsLeft, bulletsShot;


    // bools
    private bool shooting, readyToShoot, reloading;


    // camera en attack reference point
    public Camera playerCam;
    public Transform attackPoint;

    public float bulletMaxTime;


    // graphics
    [Header("Graphics")]
    public GameObject muzzleFlash;
    public TextMeshProUGUI ammoDisplay;




    [Header("Controls")]
    public KeyCode fireKey = KeyCode.Mouse0;
    public KeyCode reloadKey;


    [Header("Sounds")]
    public GameObject audioManager;



    // bugfixing (holie shid)
    public bool allowInvoke = true;


    private void Awake() {
        // vul het magazijn
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }


    private void Update() {
        MyInput();

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

        // daadwerkelijk schieten
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0) {
            bulletsShot = 0;

            Shoot();
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
        float xSpread = Random.Range(-spread, spread);
        float ySpread = Random.Range(-spread, spread);


        // nieuwe direction met spread
        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(xSpread, ySpread, 0);



        // spawn lè bullet au your mom
        // currentBullet is hier ook gelijk een pointer naar de gespawnde bullet
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
    }


    public void PlaySound(string name) {
        // geluid werkt nog niet, i dunno why
        //audioManager.GetComponent<AudioManager>().Play(name);
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
