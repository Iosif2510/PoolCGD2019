using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public Transform weaponHold;
    public Gun[] allGuns;
    public int[] allGunsBullet;
    public int currnetGunIndex;
    public Gun equippedGun;
    public event System.Action OnEquipGun;

    void Start() {
        allGunsBullet = new int[allGuns.Length];
        for (int i = 0; i < allGunsBullet.Length; i++) {
            allGunsBullet[i] = 0;
        }
        if (allGuns[0] != null) {
            EquipGun(0);
        }
    }

    public void AcquireGun(int gunIndex) {
        allGuns[gunIndex].AcquireAmmo();
        allGunsBullet[gunIndex] += allGuns[gunIndex].defaultAmmo;
        EquipGun(gunIndex);
    }

    public void EquipGun(int weaponIndex) {
        currnetGunIndex = weaponIndex;
        if (equippedGun != null) Destroy(equippedGun.gameObject);
        equippedGun = Instantiate(allGuns[weaponIndex], weaponHold.position, weaponHold.rotation) as Gun;
        equippedGun.transform.parent = weaponHold;
        equippedGun.currentAmmo = allGunsBullet[weaponIndex];
        OnEquipGun();
        print("OnEquipGun called");
    }

    public void OnTriggerHold() {
        if (equippedGun != null) equippedGun.OnTriggerHold();
    }

    public void OnTriggerRelease() {
        if (equippedGun != null) equippedGun.OnTriggerRelease();
    }

    public void Aim(Vector3 aimPoint) {
        if (equippedGun != null) equippedGun.Aim(aimPoint);
    }

    public void Reload() {
        if (equippedGun != null) equippedGun.Reload();
    }

    public float gunHeight {
        get {
            return weaponHold.position.y;
        }
    }
}
