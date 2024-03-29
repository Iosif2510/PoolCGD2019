﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    PlayerController playerController;

    float speed = 10;
    int damage = 1;
    float knockbackForce;
    bool penetration;
    Color ownerColor;

    //* shooting
    float lifeSpan = 0.5f;
    float speedCompensation = .1f;
    float spawnTime;

    //* Collision
    public LayerMask collisionMask;

    void Awake() {
        GetComponent<TrailRenderer>().material.SetColor("_TintColor", Color.white);
        playerController = FindObjectOfType<PlayerController>();
        //print(playerController.gameObject.name);
    }

    public void SetSpeed(float newSpeed) {
        speed = newSpeed;
    }
    
    public void SetKnockbackForce(float _knockbackForce)
    {
        knockbackForce = _knockbackForce;
    }
    
    public void SetPenetration(bool _penetration)
    {
        penetration = _penetration;
    }

    public void GameObjectEnable() {
        gameObject.SetActive(true);
        spawnTime = Time.time;
        //if (playerController != null) 
        ownerColor = playerController.playerColor;
        GetComponent<TrailRenderer>().material.SetColor("_TintColor", ownerColor);

        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, .1f, collisionMask);
        if (initialCollisions.Length > 0) {
            OnhitObject(initialCollisions[0], transform.position, ownerColor);
        }
        //print($"Bullet Color: {ownerColor.r}, {ownerColor.g}, {ownerColor.b}");
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
            OnhitObject(hit.collider, hit.point, ownerColor);
        }
    }

    void OnhitObject(Collider c, Vector3 hitPoint, Color shooterColor)
    {
        LivingEntity attacked = c.GetComponent<LivingEntity>();
        if (attacked != null) attacked.TakeHit(shooterColor, hitPoint, transform.forward, knockbackForce);
        if(!penetration) Disable();
    }

    void OnhitObject(Collider c, Vector3 hitPoint) {
        IDamageable damageableObj = c.GetComponent<IDamageable>();
        if (damageableObj != null) damageableObj.TakeHit(damage, hitPoint, transform.forward);
        //print(hit.collider.gameObject.name);
        if (!penetration) Disable();
    }

    void Disable() {
        gameObject.SetActive(false);
    }
}
