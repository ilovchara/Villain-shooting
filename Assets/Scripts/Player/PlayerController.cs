using System.Drawing;
using UnityEngine;
// MVC设计模式 - Controller主要是控制player的行为
[RequireComponent(typeof(Rigidbody))]
public class PlayerController: MonoBehaviour
{
    Vector3 velocity;
    Rigidbody myRigidbody;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
    }

    public void Move(Vector3 _velocity)
    {
        velocity = _velocity;
    }
    // 固定间隔刷新
    void FixedUpdate()
    {

        myRigidbody.MovePosition(myRigidbody.position + velocity * Time.fixedDeltaTime);
    }

    public void LookAt(Vector3 lookPoint)
    {
        Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(heightCorrectedPoint);    
    }

}
