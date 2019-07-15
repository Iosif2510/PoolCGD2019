using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunItem : MonoBehaviour
{
    int maxGunNum;
    int gunNum;
    public AudioClip[] reloadSounds;
    public Texture[] textures;
    float rotateSpeed = 20;

    void Update()
    {
        transform.Rotate(Vector3.up, Time.deltaTime * rotateSpeed);
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
            other.GetComponent<GunController>().EquipGun(gunNum);
            AudioManager.instance.PlaySound(reloadSounds[gunNum], transform.position);
            Destroy(gameObject);
        }
    }
}
