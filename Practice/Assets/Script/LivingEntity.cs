using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable {

    public float startingHealth;
    public float health { get; protected set; }
    protected bool dead;

    public event System.Action OnDeath;

    protected virtual void Start() {
        health = startingHealth;
    }

    public virtual void TakeDamage(float damage) {
        health -= damage;

        if (health <= 0) {
            Die();
        }
    }
    public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection) {
        //! Do sth
        TakeDamage(damage);
    }

    [ContextMenu("Self Destruct")]
    protected virtual void Die() {
        if (!dead) {
            dead = true;
            if (OnDeath != null) {
                OnDeath();
            }
            GameObject.Destroy(gameObject);
        } 
    }
}
