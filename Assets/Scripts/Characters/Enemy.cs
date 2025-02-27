using System.Collections;
using UnityEngine;
using UnityEngine.AI;
// 敌人的逻辑 - 1. 寻路 2. 攻击玩家
[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{

    // 怕调用冲突 - 这里通过枚举变量来解决
    public enum State { Idle, Chasing, Attacking };
    // 记录当前状态
    State currentState;
    Material skinMaterial;

    Color originalColor;
    NavMeshAgent pathfinder;
    Transform target;
    float attackDistanceThreshold = 1.2f;
    float timeBetweenAttacks = 1;


    float nextAttackTime;
    float myCollisionRadius;
    float targetCollisionRadius;


    protected override void Start()
    {
        base.Start();
        // 默认是追踪状态

        pathfinder = GetComponent<NavMeshAgent>();
        skinMaterial = GetComponent<Renderer>().material;
        // 保存原始颜色 
        originalColor = skinMaterial.color;

        currentState = State.Chasing;
        target = GameObject.FindGameObjectWithTag("Player").transform;

        myCollisionRadius = GetComponent<CapsuleCollider>().radius;
        targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;


        StartCoroutine(nameof(UpdatePath));
    }

    void Update()
    {
        if (Time.time > nextAttackTime)
        {
            float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;
            if (sqrDstToTarget < Mathf.Pow(attackDistanceThreshold, 2))
            {
                nextAttackTime = Time.time + timeBetweenAttacks;
                StartCoroutine(nameof(Attack));
            }
        }
    }

    IEnumerator Attack()
    {
        currentState = State.Attacking;
        pathfinder.enabled = false;

        // 获取目标之后 扑过去
        Vector3 originalPosition = transform.position;
        Vector3 attackPosition = target.position;

        float attackSpeed = 3;
        float percent = 0;

        skinMaterial.color = Color.red;
        while (percent <= 1)
        {
            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);
            yield return null;
        }

        skinMaterial.color = originalColor;
        currentState = State.Chasing;
        // 路径追踪 启用
        pathfinder.enabled = true;

    }


    // 协程 - 隔一段时间跟踪
    IEnumerator UpdatePath()
    {
        float refreshRate = 0.25f;
        while (target != null)
        {
            // 只有当前是追踪状态才会执行
            if (currentState == State.Chasing)
            {
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius);
                if (!dead)
                {
                    pathfinder.SetDestination(targetPosition);
                }
            }
            // 为什么需要这个在循环内部
            yield return new WaitForSeconds(refreshRate);
        }
    }



}