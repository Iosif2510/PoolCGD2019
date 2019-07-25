using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable {

    public float startingHealth;
    public float health { get; protected set; }
    public float maxHealth { get; protected set; }
    protected bool dead;

    protected Material skinMaterial;
    
    public event System.Action OnDeath;
    public event System.Action<Vector3> OnDeathPosition;

    protected virtual void Awake()
    {
        skinMaterial = transform.GetComponent<Renderer>().material;
    }

    protected virtual void Start() {
        health = startingHealth;
        maxHealth = startingHealth;
    }
    public void AddHealth(float amount)
    {
        health += amount;
        if (health > maxHealth) health = maxHealth;
    }
    public void AddMaxHealth(float amount)
    {
        maxHealth += amount;
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

    public virtual void TakeHit(Color attackerColor, Vector3 hitPoint, Vector3 hitDirection, float knockbackForce)
    {
        Color beforeColor = skinMaterial.color;

        MergeColor(attackerColor);

        if (skinMaterial.color == Color.white) Die();
    }

    public void MergeColor(Color addedColor)
    {
        Color beforeColor = skinMaterial.color;

        if((beforeColor.r == 1 && addedColor.r == 1) || (beforeColor.g == 1 && addedColor.g == 1) || (beforeColor.b == 1 && addedColor.b == 1))
            return;

        float rValue = beforeColor.r + addedColor.r;
        float gValue = beforeColor.g + addedColor.g;
        float bValue = beforeColor.b + addedColor.b;

        skinMaterial.color =  new Color(rValue, gValue, bValue);
    }

    [ContextMenu("Self Destruct")]
    protected virtual void Die() {
        if (!dead) {
            dead = true;
            if (OnDeath != null) {
                OnDeath();
            }
            if(OnDeathPosition != null)
            {
                OnDeathPosition(transform.position);
            }
            GameObject.Destroy(gameObject);
        } 

    }
}
