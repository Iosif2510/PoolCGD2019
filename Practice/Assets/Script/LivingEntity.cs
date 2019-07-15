using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable {

    public float startingHealth;
    public float health { get; protected set; }
    protected bool dead;

    protected Material skinMaterial;
    
    public event System.Action OnDeath;

    protected virtual void Awake()
    {
        skinMaterial = transform.GetComponent<Renderer>().material;
    }

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

    public virtual void TakeHit(Color attackerColor, Vector3 hitPoint, Vector3 hitDirection)
    {
        Color beforeColor = skinMaterial.color;

        skinMaterial.color =  MergeColor(beforeColor, attackerColor);

        if (skinMaterial.color == Color.white) Die();
    }

    public static Color MergeColor(Color c1, Color c2)
    {
        float rValue = c1.r + c2.r;
        if (rValue > 1) rValue = 0;
        float gValue = c1.g + c2.g;
        if (gValue > 1) gValue = 0;
        float bValue = c1.b + c2.b;
        if (bValue > 1) bValue = 0;

        return new Color(rValue, gValue, bValue);
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
