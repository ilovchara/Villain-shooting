using JetBrains.Annotations;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask collisionMask;
    float speed = 10;
    float damage = 1;
    float skimWith = 0.1f;


    float lifetime = 3f;
    void Start()
    {
        Destroy(gameObject, lifetime);

        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, 0.1f, collisionMask);
        if (initialCollisions.Length > 0)
        {
            OnHitObject(initialCollisions[0], transform.position);
        }
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    void Update()
    {
        float moveDistance = speed * Time.deltaTime;
        // 检测物体是否被射中
        CheckCollisions(moveDistance);
        transform.Translate(Vector3.forward * moveDistance);
    }
    // 用射线模块检测有缺陷 - 这里后期重构
    void CheckCollisions(float moveDistance)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // Debug绘制射线
        // Debug.DrawRay(ray.origin, ray.direction * moveDistance, Color.red, 0.1f);
        if (Physics.Raycast(ray, out hit, moveDistance + skimWith, collisionMask, QueryTriggerInteraction.Collide))
        {
            OnHitObject(hit.collider, hit.point);
        }
    }

    void OnHitObject(Collider c, Vector3 hitPoint)
    {
        IDamageable damageableObject = c.GetComponent<IDamageable>();
        if (damageableObject != null)
        {
            damageableObject.TakeHit(damage, hitPoint, transform.forward);
        }
        GameObject.Destroy(gameObject);
    }

}
