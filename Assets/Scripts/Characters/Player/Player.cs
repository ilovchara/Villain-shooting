// MVC设计模式 - player主要是提供数据和存储数据
using UnityEditor;
using UnityEngine;
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{
    // 玩家视角跟随鼠标
    Camera viewCamera;

    // 用于控制player的计算操作
    PlayerController controller;
    GunController gunController;

    protected override void Start()
    {
        base.Start();
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;

    }

    void Update()
    {

        Shooting();

    }

    // 玩家射击
    void Shooting()
    {
        // 射线检测
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            // 测试射线
            // Debug.DrawLine(ray.origin, point, Color.red);
            controller.LookAt(point);
        }

        // 射击
        if (Input.GetMouseButton(0))
        {
            gunController.Shoot();
        }

    }

}
