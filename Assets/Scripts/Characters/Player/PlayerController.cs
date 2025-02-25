using System;
using UnityEngine;
// MVC设计模式 - Controller主要是控制player的行为
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{

#region 移动效果
    [SerializeField] PlayerInput input;
    [SerializeField]float moveSpeed = 5f;
    new Rigidbody rigidbody;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        input.EnableGamePlayerInput();
    }

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
    private void Move(Vector3 moveInput)
    {
        Vector3 moveAmount =  new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed;
        rigidbody.velocity = moveAmount;
    }
#endregion

    // 朝向问题
    public void LookAt(Vector3 lookPoint)
    {
        Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(heightCorrectedPoint);
    }

}
