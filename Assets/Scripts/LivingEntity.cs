using UnityEngine;
using UnityEngine.Rendering;

public class LivingEntity : MonoBehaviour, IDamageable
{

    public float startingHealth;
    protected float health;
    protected bool dead;

    protected  virtual void Start()
    {
        health = startingHealth;
    }

    // 伤害 - 设置伤害值和射线
    public void TakeHit(float damage, RaycastHit hit)
    {
        health -= damage;

        if (health <= 0 && !dead)
        {
            Die();
        }
    }

    public void Die()
    {
        dead = true;
        GameObject.Destroy(gameObject);
    }

}
