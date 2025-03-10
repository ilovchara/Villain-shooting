using UnityEngine;

public interface IDamageable
{
    // 伤害
    void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDiretion);
    void TakeDamage(float damage);
}