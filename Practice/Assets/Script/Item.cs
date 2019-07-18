using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public float rotationSpeed = 40.0f;
    public float timeToDisappear = 5.0f;

    protected virtual void Start()
    {
        Destroy(gameObject, timeToDisappear);
    }

    
    protected virtual void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed);
    }
}
