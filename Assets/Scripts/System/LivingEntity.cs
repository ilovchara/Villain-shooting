using UnityEngine;
using UnityEngine.Rendering;

public class LivingEntity : MonoBehaviour, IDamageable
{

    public float startingHealth;
    protected float health;
    protected bool dead;
    // 事件 - 死亡
    public event System.Action OnDeath;

    protected virtual void Start()
    {
        health = startingHealth;
    }

    [ContextMenu("Self Destruct")]
    protected void Die()
    {
        dead = true;
        if (OnDeath != null)
        {
            // 通知别人这个死亡事件发生了
            OnDeath();
        }
        GameObject.Destroy(gameObject);
    }

    // 伤害 - 设置伤害值和射线
    public void TakeHit(float damage, RaycastHit hit)
    {
        // To DO
        TakeDamage(damage);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0 && !dead)
        {
            Die();
        }
    }
}
