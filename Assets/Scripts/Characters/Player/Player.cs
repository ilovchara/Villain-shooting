// 1. 玩家的移动效果 2. 玩家的射击效果
using System;
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


    [SerializeField] PlayerInput input;
    [SerializeField] float moveSpeed = 5f;
    new Rigidbody rigidbody;


    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();

    }

    protected override void Start()
    {
        base.Start();
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;

        input.EnableGamePlayerInput();

    }

    void Update()
    {
        Shooting();
    }

    #region 移动效果


    [Obsolete]
    void OnEnable()
    {
        input.onMove += Move;
        input.onStopMove += StopMove;
    }

    [Obsolete]
    void OnDisable()
    {
        input.onMove -= Move;
        input.onStopMove -= StopMove;
    }

    [Obsolete]
    private void StopMove()
    {
        rigidbody.velocity = Vector3.zero;
    }

    [Obsolete]
    private Coroutine moveCoroutine;

    [Obsolete]
    private void Move(Vector3 moveInput)
    {
        // 持续按键输入时更新刚体的速度
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized; // 保持方向一致
        rigidbody.velocity = moveDirection * moveSpeed;
    }

    #endregion

    #region 射击
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
    #endregion
}
