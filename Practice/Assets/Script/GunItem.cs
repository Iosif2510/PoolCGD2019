using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunItem : Item
{
    int maxGunNum;
    public int gunNum;
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
        //Debug.Log(gunNum);
        GetComponent<Renderer>().material.mainTexture = textures[gunNum];
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if(other.gameObject.tag == "Player")
        {
            GunController gunController = other.GetComponent<GunController>();
            gunController.AcquireGun(gunNum);
            AudioManager.Instance.PlaySound(gunController.allGuns[gunNum].reloadAudio, transform.position);
            Destroy(gameObject);
        }
    }
}
