using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunItem : MonoBehaviour
{
    int maxGunNum;
    int gunNum;
    public Texture[] textures;
    public float rotateSpeed = 20;
    public float timeToDisappear = 5.0f;

    void Start()
    {
        Destroy(gameObject, timeToDisappear);
    }
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
            GunController gunController = other.GetComponent<GunController>();
            gunController.EquipGun(gunNum, other.GetComponent<Renderer>().material.color);
            AudioManager.instance.PlaySound(gunController.allGuns[gunNum].reloadAudio, transform.position);
            Destroy(gameObject);
        }
    }
}
