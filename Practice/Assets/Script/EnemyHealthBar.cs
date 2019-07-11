using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    public Enemy myEnemy;
    public static Vector3 initialPosition { get; private set; } = new Vector3(-0.5f, 2, 0);

    Vector3 initialScale;
    Material myMaterial;

    void Awake() {
        myMaterial = GetComponent<Renderer>().material;
    }

    void Start() {
        initialScale = transform.localScale;
    }

    void LateUpdate() {
        transform.position = myEnemy.transform.position + initialPosition;
        transform.localScale = new Vector3(initialScale.x * myEnemy.health / myEnemy.startingHealth, initialScale.y, initialScale.z);
    }

    public void SetEnemy(Enemy enemy) {
        myEnemy = enemy;
        myMaterial.color = myEnemy.ownColor;
        myEnemy.OnDeath += EnemyDeath;
    }

    void EnemyDeath() {
        Destroy(gameObject);
    }
}
