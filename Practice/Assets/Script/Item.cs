using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public float rotationSpeed = 40.0f;
    public float timeToDisappear = 10.0f;
    bool disappearance = true;

    public static event System.Action ItemSecure;

    protected virtual void Start()
    {
        if(disappearance)
            Destroy(gameObject, timeToDisappear);
    }

    
    protected virtual void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed);
    }
    
    public void SetDisappearance(bool _disappearance)
    {
        disappearance = _disappearance;
    }

    protected virtual void OnTriggerEnter(Collider col) {
        if (ItemSecure != null) ItemSecure();
    }
}
