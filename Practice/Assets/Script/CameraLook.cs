using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLook : MonoBehaviour
{
    public Transform target;
    float dist = 10f;
    float height = 21f;

    void Update() {
        if (target != null) {
            transform.position = new Vector3(target.position.x, Vector3.up.y * height, target.position.z - Vector3.forward.z * dist);
            transform.LookAt(target);
        }
    }
}
