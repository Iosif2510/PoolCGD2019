using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public Transform weaponHold;
    public Gun[] allGuns;
    bool[] possessingGuns;
    Gun equippedGun;

    void Start() {
        if (allGuns[0] != null) {
            EquipGun(allGuns[0], gameObject.GetComponent<Renderer>().material.color);
        }

        int gunNum = allGuns.Length;
        possessingGuns = new bool[gunNum];
        possessingGuns[0] = true;
        for (int i = 1; i < gunNum; i++) {
            possessingGuns[i] = false;
        }
    }

    public void AcquireGun(int gunIndex, Color ownerColor) {
        possessingGuns[gunIndex] = true;
        EquipGun(allGuns[gunIndex], ownerColor);
    }

    public void EquipGun(Gun gunToEquip, Color ownerColor) {
        if (equippedGun != null) Destroy(equippedGun.gameObject);
        equippedGun = Instantiate(gunToEquip, weaponHold.position, weaponHold.rotation) as Gun;
        equippedGun.transform.parent = weaponHold;
    }

    public void EquipGun(int weponIndex, Color ownerColor) {
        EquipGun(allGuns[weponIndex], ownerColor);
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
