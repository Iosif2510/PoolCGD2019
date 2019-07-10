using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{

    public Rigidbody myRigidBody;
    public float forceMin;
    public float forceMax;

    Material mat;
    Color initialColor;

    float lifetime = 2;
    float fadetime = 2;

    void Awake() {
        mat = GetComponent<Renderer>().material;
        initialColor = mat.color;
    }

    void OnEnable()
    {
        mat.color = initialColor;
        float force = Random.Range(forceMin, forceMax);
        myRigidBody.AddForce(transform.right * force);
        myRigidBody.AddTorque(Random.insideUnitSphere * force);

        StartCoroutine(Fade());
    }

    void OnDisable() {

    }

    IEnumerator Fade() {
        yield return new WaitForSeconds(lifetime);

        float percent = 0;
        float fadeSpeed = 1 / fadetime;

        while (percent < 1) {
            percent += Time.deltaTime * fadeSpeed;
            mat.color = Color.Lerp(initialColor, Color.clear, percent);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
