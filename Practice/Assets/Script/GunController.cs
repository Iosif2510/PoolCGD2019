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
    public event System.Action OnEquipGun;

    void Start() {
        if (allGuns[0] != null) {
            EquipGun(allGuns[0]);
        }
    }

    public void AcquireGun(int gunIndex) {
        acquiredGuns[gunIndex] = true;
        allGuns[gunIndex].AcquireAmmo();
        EquipGun(gunIndex);
    }

    private void EquipGun(Gun gunToEquip) {
        if (equippedGun != null) Destroy(equippedGun.gameObject);
        equippedGun = Instantiate(gunToEquip, weaponHold.position, weaponHold.rotation) as Gun;
        equippedGun.transform.parent = weaponHold;
        OnEquipGun();
    } //! do not use directly

    public void EquipGun(int weaponIndex) {
        currnetGunIndex = weaponIndex;
        EquipGun(allGuns[weaponIndex]);
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
