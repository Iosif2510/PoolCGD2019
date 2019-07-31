using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum FireMode {
        Auto, Burst, Single
    };

    public FireMode fireMode;

    public static event System.Action OnShoot;
    public static event System.Action OnReload;

    [Header("Ammo")]
    public Transform[] projectileSpawn;
    public Projectile projectile;
    public float msBtwShots = 100;
    public float muzzleVelocity = 35;
    public int burstCount;
    public float reloadTime = .3f;
    public float knockbackForce = 0f;
    
    public int maxAmmo; //!
    public int currentAmmo;
    public int defaultAmmo;
    public int projectilesPerMag;
    public int projectilesRemainingInMag;
    
    public bool penetration = false;

    Projectile[,] bullets;
    Transform[] shells;
    public int bulletAmount = 30;
    int index;
    float nextShotTime;
    
    bool triggerReleasedSinceLastShot;
    int shotsRemainingInBurst;
    bool isReloading;


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

    Vector3 recoilSmoothDampVelocity;
    float recoilRotSmoothDampSpeed;
    float recoilAngle;
    
    public void Awake() {
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
                bullets[j,i].SetKnockbackForce(knockbackForce);
                bullets[j,i].SetPenetration(penetration);
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
                    if (maxAmmo >= 0) currentAmmo--;
                    nextShotTime = Time.time + msBtwShots / 1000;

                    bullets[i, index].transform.position = projectileSpawn[i].position;
                    bullets[i, index].transform.rotation = projectileSpawn[i].rotation;
                    bullets[i, index].GameObjectEnable();
                }
            }

            if (!shells[index].gameObject.activeSelf) {
                shells[index].position = shellEjection.position;
                shells[index].rotation = shellEjection.rotation;
                shells[index].gameObject.SetActive(true);
                index = (index + 1) % bulletAmount;

                muzzleflash.Activate();
            }
            
            OnShoot();
            //recoil
            transform.localPosition -= Vector3.forward * .2f;
            recoilAngle += Random.Range(recoilAngleMinMax.x, recoilAngleMinMax.y);
            recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);

            AudioManager.Instance.PlaySound(shootAudio, transform.position);
        }
    }

    public void Reload() {
        if (!isReloading && projectilesRemainingInMag != projectilesPerMag && ((currentAmmo > 0) || (maxAmmo < 0))) {
            AudioManager.Instance.PlaySound(reloadAudio, transform.position);
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

        // * ReloadFunction
        isReloading = false;
        if ((currentAmmo >= projectilesPerMag) || (maxAmmo == -1)) {
            projectilesRemainingInMag = projectilesPerMag;
        }
        else {
            projectilesRemainingInMag = currentAmmo;
        }
        OnReload();
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

    public void AcquireAmmo() {
        currentAmmo = (currentAmmo + defaultAmmo > maxAmmo) ? maxAmmo : currentAmmo + defaultAmmo;
    }
    public void AcquireAmmo(int ammo) {
        currentAmmo = (currentAmmo + ammo > maxAmmo) ? maxAmmo : currentAmmo + ammo;
    }
}
