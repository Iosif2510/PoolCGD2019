using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    float speed = 10;
    float damage = 1;


    //* shooting
    float lifeSpan = 0.5f;
    float speedCompensation = .1f;
    float spawnTime;

    //* Collision
    public LayerMask collisionMask;
    public Color trailColor;

    void Start() {
        GetComponent<TrailRenderer>().material.SetColor("_TintColor", trailColor);
    }

    public void SetSpeed(float newSpeed) {
        speed = newSpeed;
    }

    void OnEnable() {
        spawnTime = Time.time;

        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, .1f, collisionMask);
        if (initialCollisions.Length > 0) {
            OnhitObject(initialCollisions[0], transform.position);
        }
    }

    void Update()
    {
        float moveDistance = speed * Time.deltaTime;
        CheckCollisions(moveDistance);
        transform.Translate(Vector3.forward * Time.deltaTime * speed);

        //disabling bullet if too much time spent
        if (Time.time - spawnTime > lifeSpan) {
            Disable();
        }
    }

    void CheckCollisions(float moveDistance) {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, moveDistance + speedCompensation, collisionMask, QueryTriggerInteraction.Collide)) {
            OnhitObject(hit.collider, hit.point);
        }
    }

    void OnhitObject(Collider c, Vector3 hitPoint) {
        IDamageable damageableObj = c.GetComponent<IDamageable>();
        if (damageableObj != null) damageableObj.TakeHit(damage, hitPoint, transform.forward);
        //print(hit.collider.gameObject.name);
        Disable();
    }

    void Disable() {
        gameObject.SetActive(false);
    }
}
