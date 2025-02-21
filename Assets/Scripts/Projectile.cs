using UnityEngine;

public class Projectile : MonoBehaviour
{
    float speed = 10;

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    // 射击子弹
    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }


}
