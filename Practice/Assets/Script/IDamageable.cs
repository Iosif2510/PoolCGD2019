using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable {
    void TakeHit(int damage, Vector3 hitPoint, Vector3 hitDirection);
    void TakeHit(Color attackerColor, Vector3 hitPoint, Vector3 hitDirection, float knockbackForce);
    void TakeDamage(int damage);
}
