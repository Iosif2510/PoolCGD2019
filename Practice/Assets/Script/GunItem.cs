using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunItem : Item
{
    int maxGunNum;
    int gunNum;
    public Texture[] textures;

    protected override void Start()
    {
        base.Start();
    }
    protected override void Update()
    {
        base.Update();
    }
    public void SetGunItem(int _gunNum)
    {
        gunNum = _gunNum;
        Debug.Log(gunNum);
        GetComponent<Renderer>().material.mainTexture = textures[gunNum];
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            GunController gunController = other.GetComponent<GunController>();
            gunController.EquipGun(gunNum, other.GetComponent<Renderer>().material.color);
            AudioManager.instance.PlaySound(gunController.allGuns[gunNum].reloadAudio, transform.position);
            Destroy(gameObject);
        }
    }
}
