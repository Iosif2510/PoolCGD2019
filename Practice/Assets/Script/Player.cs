﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (PlayerController))]
[RequireComponent (typeof (GunController))]
public class Player : LivingEntity
{
    public float moveSpeed = 5;

    public Crosshairs crosshairs;

    PlayerController controller;
    GunController gunController;
    Camera ViewCamera;

    void Awake() {
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        ViewCamera = Camera.main;
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    protected override void Start() {
        base.Start();
    }

    void OnNewWave(int waveNumber) {
        health = startingHealth;
        gunController.EquipGun(0);
    }

    protected override void Die() {
        AudioManager.instance.PlaySound("Player Death", transform.position);
        base.Die();
    }

    void Update() {
        //* Movement Input
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.Move(moveVelocity);

        //* Look Input
        Ray ray = ViewCamera.ScreenPointToRay(Input.mousePosition);
        Plane GroundPlane = new Plane(Vector3.up, Vector3.up * gunController.gunHeight);

        float rayDistance;

        if (GroundPlane.Raycast(ray, out rayDistance)) {
            Vector3 point = ray.GetPoint(rayDistance);
            //Debug.DrawLine(ray.origin, point, Color.red);
            controller.LookAt(point);
            crosshairs.transform.position = point;
            crosshairs.DetectTargets(ray);
            if ((point - transform.position).sqrMagnitude > 1) gunController.Aim(point);
        }

        //* Weapon Input
        if (Input.GetMouseButton(0)) {
            gunController.OnTriggerHold();
        }
        if (Input.GetMouseButtonUp(0)) {
            gunController.OnTriggerRelease();
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            gunController.Reload();
        }

        if (transform.position.y < -10) {
            TakeDamage(health);
        }

    }

}
