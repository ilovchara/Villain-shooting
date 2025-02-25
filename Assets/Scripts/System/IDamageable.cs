using UnityEngine;

public interface IDamageable
{
    // 伤害
    void TakeHit(float damage, RaycastHit hit);
}