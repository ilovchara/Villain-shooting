using System;
using System.Collections;
using UnityEngine;
// MVC设计模式 - Controller主要是控制player的行为
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // 朝向问题
    public void LookAt(Vector3 lookPoint)
    {
        Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(heightCorrectedPoint);
    }







}
