using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public Transform weaponHold;
    public Gun[] allGuns;
    Gun equippedGun;

    void Start() {
        if (allGuns[0] != null) {
            EquipGun(allGuns[3], gameObject.GetComponent<Renderer>().material.color);
        } 
    }

    public void EquipGun(Gun gunToEquip, Color ownerColor) {
        if (equippedGun != null)  Destroy(equippedGun.gameObject);
        equippedGun = Instantiate(gunToEquip, weaponHold.position, weaponHold.rotation) as Gun;
        equippedGun.transform.parent = weaponHold;
        equippedGun.ownerColor = ownerColor;
    }

    public void EquipGun(int weponIndex, Color ownerColor) {
        EquipGun(allGuns[weponIndex], ownerColor);
    }

    public void SetGunOwnerColor(Color c)
    {
        equippedGun.ownerColor = c;
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
