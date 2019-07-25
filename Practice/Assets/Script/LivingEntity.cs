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

    public virtual void TakeHit(Color attackerColor, Vector3 hitPoint, Vector3 hitDirection)
    {
        Color beforeColor = skinMaterial.color;

        skinMaterial.color =  MergeColor(beforeColor, attackerColor);

        if (skinMaterial.color == Color.white) Die();
    }

    public static Color MergeColor(Color c1, Color c2)
    {
        if((c1.r == 1 && c2.r == 1) || (c1.g == 1 && c2.g == 1) || (c1.b == 1 && c2.b == 1))
            return c1;

        float rValue = c1.r + c2.r;
        float gValue = c1.g + c2.g;
        float bValue = c1.b + c2.b;

        return new Color(rValue, gValue, bValue);
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
