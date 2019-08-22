using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : LivingEntity
{
    public enum State { Idle, Chasing, Attacking };
    State currentState;
    public bool isDummy = false;

    public ParticleSystem deathEffect;
    public WhiteDeathEffect whiteDeathEffect;
    public static event System.Action OnDeathStatic;

    public Color ownColor;

    //ParticleSystem.MainModule deathEffectMain;
    NavMeshAgent pathfinder;
    //Material skinMaterial;
    Transform target;
    LivingEntity targetEntity;

    float attackDistanceThreshold = .3f;
    float timeBetweenAttacks = 1;
    int damage;

    float nextAttackTime;
    float myCollisionRadius;
    float targetCollisionRadius;

    bool hasTarget;

    /* 
    todo 색깔을 담는 멤버 변수 정의, 이에 따라 색깔 바뀌어야 함
    todo GetComponent<Renderer>().material.color
    */

    protected override void Awake() {

        base.Awake();

        startingHealth = 1;
        health = startingHealth;

        pathfinder = GetComponent<NavMeshAgent>();

        if (GameObject.FindGameObjectWithTag("Player") != null) {
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
        }
    }

    protected override void Start()
    {
        base.Start();
        
        if (hasTarget && !isDummy) {
            currentState = State.Chasing;
            targetEntity.OnDeath += OnTargetDeath;

            StartCoroutine(UpdatePath());
        }

    }

    /*
    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection) {
        AudioManager.Instance.PlaySound("Impact", transform.position);
        if (damage >= health) { //Death Effect
            if (OnDeathStatic != null) OnDeathStatic();
            AudioManager.Instance.PlaySound("Enemy Death", transform.position);
            Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, deathEffect.main.startLifetime.constant);
        }

        base.TakeHit(damage, hitPoint, hitDirection);
    }
    */

    public override void TakeHit(Color attackerColor, Vector3 hitPoint, Vector3 hitDirection, float knockbackForce)
    {
        AudioManager.Instance.PlaySound("Impact", transform.position);
        MergeColor(attackerColor);
        if(knockbackForce != 0)
        {
            StartCoroutine(Knockback(hitDirection.normalized, knockbackForce));
            return;
        }
        if (skinMaterial.color == Color.white)
        { //Death Effect
            if (OnDeathStatic != null) OnDeathStatic();
            AudioManager.Instance.PlaySound("Enemy Death", transform.position);
            WhiteDeathEffect wde = Instantiate(whiteDeathEffect, transform.position, Quaternion.identity) as WhiteDeathEffect;
            Die();
        }
    }

    void OnTargetDeath() {
        hasTarget = false;
        currentState = State.Idle;
    }

    void Update()
    {
        if (hasTarget && !isDummy) {
            if (Time.time > nextAttackTime) {
                float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;
                if (sqrDstToTarget < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2)) {
                    nextAttackTime = Time.time + timeBetweenAttacks;
                    AudioManager.Instance.PlaySound("Enemy Attack", transform.position);
                    StartCoroutine(Attack());
                }
            }
        }
    }

    public void SetCharacteristics(float moveSpeed, Color skinColor, ParticleSystem _deathEffect) {
        pathfinder.speed = moveSpeed;
        damage = 1;

        ownColor = new Color(skinColor.r, skinColor.g, skinColor.b, 1);
        deathEffect = _deathEffect;
        skinMaterial.color = ownColor;
    }
    public void SetDummy(bool _isDummy)
    {
        isDummy = _isDummy;
        if (isDummy) currentState = State.Idle;
        else currentState = State.Chasing;
    }

    protected override void Die() {
        /*
        if (Random.Range(0, 10) == 0) { //spawn gun item by chance
            GunItem spawnedGunItem = Instantiate(gunItem, transform.position, Quaternion.identity) as GunItem;
            Debug.Log("Item spawned");
            int randomItemIndex = (int)Random.Range(1,gunController.allGuns.Length);
            Debug.Log(randomItemIndex);
            spawnedGunItem.SetGunItem(randomItemIndex);
        }
        */
        base.Die();
    }

    IEnumerator Knockback(Vector3 hitDirection, float knockbackForce)
    {
        currentState = State.Idle;
        float knockbackTime = 0.1f;
        while(knockbackTime >= 0)
        {
            transform.position += Vector3.Lerp(hitDirection * knockbackForce, Vector3.zero, knockbackTime / 0.5f);
            knockbackTime -= Time.deltaTime;
            yield return null;
        }
        currentState = State.Chasing;
        if (skinMaterial.color == Color.white)
        { //Death Effect
            if (OnDeathStatic != null) OnDeathStatic();
            AudioManager.Instance.PlaySound("Enemy Death", transform.position);
            WhiteDeathEffect wde = Instantiate(whiteDeathEffect, transform.position, Quaternion.identity) as WhiteDeathEffect;
            Die();
        }
    }

    IEnumerator Attack() {

        currentState = State.Attacking;
        pathfinder.enabled = false;

        Vector3 originalPosition = transform.position;
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius);

        float attackSpeed = 3;
        float percent = 0;

        bool hasAppliedDamage = false;

        while (percent <= 1) {

            if (percent >= .5f && !hasAppliedDamage) {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }

            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent,2) + percent) * 4;
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);
            yield return null;
        }

        pathfinder.enabled = true;
        currentState = State.Chasing;
    }

    IEnumerator UpdatePath() {
        float refreshRate = 0.25f;

        while (hasTarget) {
            if (currentState == State.Chasing) {
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                Vector3 targetposition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold / 2);
                if (!dead) {
                    pathfinder.SetDestination(targetposition);
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
