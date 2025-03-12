using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform muzzle;
    public Projectile projectile;
    // 射击间隔时间
    public float msBetweenShots = 100;
    public float muzzleVelocity = 35;

    MuzzleFlash muzzleFlash;
    public Transform shell;
    public Transform shellEjection;


    float nextShotTime;

    void Start()
    {
        muzzleFlash = GetComponent<MuzzleFlash>();  // 确保 Gun 对象上有 MuzzleFlash 组件
    }


    public void Shoot()
    {
        if (Time.time > nextShotTime)
        {
            // 间隔时间 默认间隔时间是0.02秒
            nextShotTime = Time.time + msBetweenShots / 1000;
            Projectile newProjectile = Instantiate(projectile, muzzle.position, muzzle.rotation) as Projectile;
            newProjectile.SetSpeed(muzzleVelocity);
            Instantiate(shell, shellEjection.position, shellEjection.rotation);
            muzzleFlash.Activate();

        }
    }


}
