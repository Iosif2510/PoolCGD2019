using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public Transform weaponHold;
    public Gun[] allGuns;
    public bool[] acquiredGuns;
    public int currnetGunIndex;
    public Gun equippedGun;

    private Gun[] allGunsObj;

    public event System.Action OnEquipGun;

    void Start() {
        allGunsObj = new Gun[allGuns.Length];
        acquiredGuns = new bool[allGuns.Length];
        for (int i = 0; i < allGuns.Length; i++) {
            allGunsObj[i] = null;
            acquiredGuns[i] = false;
        }
        for (int i = 0; i < allGuns.Length; i++) {
            allGunsObj[i] = Instantiate(allGuns[i], weaponHold.position, weaponHold.rotation, weaponHold) as Gun;
            allGunsObj[i].gameObject.SetActive(false);
        }
        if (allGunsObj[0] != null) {
            EquipGun(0);
        }
    }

    public void AcquireGun(int gunIndex) {
        allGunsObj[gunIndex].AcquireAmmo();
        EquipGun(gunIndex);
    }

    public void EquipGun(int weaponIndex) {
        currnetGunIndex = weaponIndex;
        if (equippedGun != null) {
            equippedGun.gameObject.SetActive(false);
            }
        allGunsObj[weaponIndex].gameObject.SetActive(true);
        equippedGun = allGunsObj[weaponIndex];
        acquiredGuns[weaponIndex] = true;
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
