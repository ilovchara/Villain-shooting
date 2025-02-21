// MVC设计模式 - player主要是提供数据和存储数据
using UnityEngine;
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : MonoBehaviour
{

    public float moveSpeed = 5;
    // 玩家视角跟随鼠标
    Camera viewCamera;

    // 用于控制player的计算操作
    PlayerController controller;
    GunController gunController;

    void Start()
    {
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;

    }

    void Update()
    {
        // 获取用户输入的方法 - Raw不会平滑处理
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.Move(moveVelocity);

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
