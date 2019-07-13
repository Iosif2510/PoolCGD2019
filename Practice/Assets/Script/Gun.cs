using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum FireMode {
        Auto, Burst, Single
    };

    public FireMode fireMode;

    public Transform[] projectileSpawn;
    public Projectile projectile;
    public float msBtwShots = 100;
    public float muzzleVelocity = 35;
    public int burstCount;
    public float reloadTime = .3f;

    Projectile[,] bullets;
    Transform[] shells;
    public int bulletAmount = 30;
    int index;

    public Color ownerColor;

    [Header("Recoil")]
    public Vector2 kickMinMax = new Vector2(.05f, 2f);
    public Vector2 recoilAngleMinMax = new Vector2(3,5);
    public float recoilMoveSettleTime = .1f;
    public float recoilRotateSettleTime = .1f;

    [Header("Effect")]

    public Transform shell;
    public Transform shellEjection;
    public AudioClip shootAudio;
    public AudioClip reloadAudio;
    MuzzleFlash muzzleflash;

    float nextShotTime;
    
    bool triggerReleasedSinceLastShot;
    int shotsRemainingInBurst;
    int projectilesPerMag;
    int projectilesRemainingInMag;
    bool isReloading;

    Vector3 recoilSmoothDampVelocity;
    float recoilRotSmoothDampSpeed;
    float recoilAngle;
    
    public void Start() {
        shotsRemainingInBurst = burstCount;
        muzzleflash = GetComponent<MuzzleFlash>();
        int projectileSpawnLength = projectileSpawn.Length;
        projectilesPerMag = bulletAmount * projectileSpawnLength;
        bullets = new Projectile[projectileSpawnLength, bulletAmount];
        shells = new Transform[bulletAmount];
        for (int i = 0; i < bulletAmount; i++) {
            for (int j = 0; j < projectileSpawnLength; j++) {
                bullets[j,i] = Instantiate(projectile) as Projectile;
                bullets[j,i].gameObject.SetActive(false);
                bullets[j,i].SetSpeed(muzzleVelocity);
            }
            shells[i] = Instantiate(shell) as Transform;
            shells[i].gameObject.SetActive(false);
        }
        projectilesRemainingInMag = projectilesPerMag;
        index = 0;
    }

    void LateUpdate() {
        //animate recoil
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, recoilMoveSettleTime);
        recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotSmoothDampSpeed, recoilRotateSettleTime);
        transform.localEulerAngles = transform.localEulerAngles + Vector3.left * recoilAngle;

        if (!isReloading && projectilesRemainingInMag == 0) {
            Reload();
        }
    }

    void Shoot() {
        if (!isReloading && (projectilesRemainingInMag > 0) &&  (Time.time > nextShotTime)) {

            if (fireMode == FireMode.Burst) {
                if (shotsRemainingInBurst == 0) return;
                shotsRemainingInBurst--;
            }
            else if (fireMode == FireMode.Single) {
                if (!triggerReleasedSinceLastShot) return;
            }

            for (int i = 0; i < projectileSpawn.Length; i++) {
                if (!bullets[i, index].gameObject.activeSelf) {
                    if (projectilesRemainingInMag == 0) break;
                    projectilesRemainingInMag--;
                    nextShotTime = Time.time + msBtwShots / 1000;

                    bullets[i, index].transform.position = projectileSpawn[i].position;
                    bullets[i, index].transform.rotation = projectileSpawn[i].rotation;
                    bullets[i, index].SetOwnerColor(ownerColor);
                    bullets[i, index].gameObject.SetActive(true);

                }
            }
            if (!shells[index].gameObject.activeSelf) {
                shells[index].position = shellEjection.position;
                shells[index].rotation = shellEjection.rotation;
                shells[index].gameObject.SetActive(true);
                index = (index + 1) % bulletAmount;

                muzzleflash.Activate();
            }
            
            //recoil
            transform.localPosition -= Vector3.forward * .2f;
            recoilAngle += Random.Range(recoilAngleMinMax.x, recoilAngleMinMax.y);
            recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);

            AudioManager.instance.PlaySound(shootAudio, transform.position);
        }
    }

    public void Reload() {
        if (!isReloading && projectilesRemainingInMag != projectilesPerMag) {
            AudioManager.instance.PlaySound(reloadAudio, transform.position);
            StartCoroutine(AnimateReload());
            }
    }

    IEnumerator AnimateReload() {
        isReloading = true;
        yield return new WaitForSeconds(.2f);

        float reloadSpeed = 1f / reloadTime;
        float percent = 0;
        Vector3 initialRot = transform.localEulerAngles;
        float maxReloadAngle = 30;

        while(percent < 1) {
            percent += Time.deltaTime * reloadSpeed;
            float interpolation = (-Mathf.Pow(percent,2) + percent) * 4;
            float reloadAngle = Mathf.Lerp(0,maxReloadAngle, interpolation);
            transform.localEulerAngles = initialRot + Vector3.left * reloadAngle;

            yield return null;
        }

        isReloading = false;
        projectilesRemainingInMag = projectilesPerMag;
    }

    public void Aim(Vector3 aimPoint) {
        if (!isReloading) transform.LookAt(aimPoint);
    }

    public void OnTriggerHold() {
        Shoot();
        triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerRelease() {
        triggerReleasedSinceLastShot = true;
        shotsRemainingInBurst = burstCount;
    }
}
