using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunItem : MonoBehaviour
{

    int gunNum;
    public AudioClip[] reloadSounds;
    public Texture[] textures;
    float rotateSpeed = 20;

    void Start()
    {
        
    }

    
    void Update()
    {
        transform.Rotate(Vector3.up, Time.deltaTime * rotateSpeed);
    }

    void SetGunItem(int _gunNum)
    {
        gunNum = _gunNum;
        gameObject.GetComponent<Renderer>().material.mainTexture = textures[gunNum];
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            other.GetComponent<GunController>().EquipGun(gunNum);
            AudioManager.instance.PlaySound(reloadSounds[gunNum], transform.position);
            Destroy(gameObject);
        }
    }
}
