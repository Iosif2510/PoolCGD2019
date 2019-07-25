using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable {

    public int startingHealth;
    public int health { get; protected set; }
    public int maxHealth { get; protected set; }
    public int healthLimit;
    protected bool dead;

    protected Material skinMaterial;
    
    public event System.Action OnDeath;
    public event System.Action<Vector3> OnDeathPosition;
    public event System.Action OnHealthChange;

    protected virtual void Awake()
    {
        skinMaterial = transform.GetComponent<Renderer>().material;
        health = startingHealth;
        maxHealth = startingHealth;
    }

    protected virtual void Start() {

    }
    
    public void AddHealth(int amount)
    {
        health += amount;
        if (health > maxHealth) health = maxHealth;
        OnHealthChange();
    }
    public void AddMaxHealth(int amount)
    {
        maxHealth += amount;
        OnHealthChange();
    }

    public virtual void TakeDamage(int damage) {
        health -= damage;
        OnHealthChange();

        if (health <= 0) {
            Die();
        }
    }
    public virtual void TakeHit(int damage, Vector3 hitPoint, Vector3 hitDirection) {
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
