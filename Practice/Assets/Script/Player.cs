using System.Collections;
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
    public bool isTutorial = false;

    protected override void Awake() {

        base.Awake();

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
    }

    public override void TakeDamage(int damage)
    {
        if (isTutorial) return;

        base.TakeDamage(damage);
    }
    protected override void Die() {
        AudioManager.Instance.PlaySound("Player Death", transform.position);
        if (MapGenerator.Instance.currentMode == MapGenerator.GameMode.Tutorial) {
            Spawner.Instance.ResetPlayerPosition();
            health = maxHealth;
        }
        else base.Die();
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

        // Color Change Input
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            controller.ChangeColor(skinMaterial, 'r');
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            controller.ChangeColor(skinMaterial, 'g');
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            controller.ChangeColor(skinMaterial, 'b');
        }

        int allGunNum = gunController.allGuns.Length;
        
        if (Input.GetAxis("Mouse ScrollWheel") > 0) {
            int index;
            for (index = positiveMod(gunController.currnetGunIndex - 1, allGunNum); !gunController.acquiredGuns[index];
            index = positiveMod(index - 1, allGunNum)) {}
            print(index);
            gunController.EquipGun(index);
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0) {
            int index;
            for (index = (gunController.currnetGunIndex + 1) % allGunNum; !gunController.acquiredGuns[index];
            index = (index + 1) % allGunNum) {}
            print(index);
            gunController.EquipGun(index);
        }

        //* */ 마우스 스크롤로 무기 변환

    }

    int positiveMod(int n1, int n2) {
        return (n1 % n2 >= 0) ? n1 % n2 : n1 % n2 + n2;
    }

}
