using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{

    Vector3 initialPosition;
    public event System.Action NextWave;

    void OnEnable() {
        initialPosition = transform.position;
        StartCoroutine(PeekUp());
    }

    void OnDisable() {
        transform.position = initialPosition;
    }

    void OnTriggerEnter(Collider col) {
        if (col.gameObject.tag == "Player") {
            Debug.Log("nextwave");
            NextWave();
        }
    }

    IEnumerator PeekUp() {
        while (transform.position.y < 1.2f) {
            transform.Translate(Vector3.up * 2f * Time.deltaTime);
            yield return null;
        }
    }
}
