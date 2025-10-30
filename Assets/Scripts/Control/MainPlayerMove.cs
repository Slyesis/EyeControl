using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using MimicSpace; // === 新增代码 (1/4): 引用 Mimic 的命名空间 ===

public class MainPlayerMove : MonoBehaviour
{
    [HideInInspector]
    public NavMeshAgent navMeshAgent;

    private Vector3 desiredVelocity;
    private Vector3 smoothPositionRef; // λƽοֵ

    [Header("Сٶ")]
    public float moveSpeed = 0.1f;

    // === 新增代码 (2/4): 用来引用 Mimic 组件 ===
    private Mimic myMimic;

    void Start()
    {
        navMeshAgent = this.GetComponent<NavMeshAgent>();

        // === 新增代码 (3/4): 在 Start 中获取 Mimic 组件 ===
        myMimic = this.GetComponent<Mimic>();
        if (myMimic == null)
        {
            Debug.LogWarning("在 MainPlayerMove 对象上没有找到 Mimic 组件！腿部特效将无法工作。");
        }
        // ==========================================

        // رԶٶȿ
        navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;
    }
    public void OnMove(Vector3 vector3, bool isUpdateMove = false)
    {
        if (isUpdateMove)
        {
            transform.DOMove(vector3, moveSpeed);
            //transform.position = Vector3.Lerp(transform.position, vector3, moveSpeed * Time.deltaTime);
            //transform.position = vector3;
        }
        else 
        {

            isMoveSucc(vector3, isUpdateMove);
        }
    }
    void isMoveSucc(Vector3 vector3, bool isUpdateMove)
    {

        // ÿ֡ĿNavMesh 
        navMeshAgent.nextPosition = transform.position;
        navMeshAgent.destination = vector3;
        // ȡNavMeshıϷ
        if (navMeshAgent.hasPath)
        {
            desiredVelocity = navMeshAgent.desiredVelocity;
        }
        else
        {
            // û·ʱֱӳĿ
            desiredVelocity = (vector3 - transform.position).normalized * navMeshAgent.speed;
        }

        // ֶӦƶNavMesh·Ҹţ
        ApplyManualMovement(vector3, isUpdateMove);
    }
    void ApplyManualMovement(Vector3 vector3, bool isUpdateMove)
    {
        // ʹNavMeshıϷ򣬵ֶƶ
        if (desiredVelocity.magnitude > 0.1f)
        {
            Vector3 newPosition = transform.position + desiredVelocity * Time.deltaTime;
            // ȷλNavMesh
            if (NavMesh.SamplePosition(newPosition, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                // ƽƶĿλ
                transform.position = Vector3.SmoothDamp(transform.position, hit.position, ref smoothPositionRef, moveSpeed);

                navMeshAgent.nextPosition = transform.position;
            }
            // תƶ
            if (desiredVelocity.magnitude > 0.5f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(desiredVelocity);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }

        // === 新增代码 (4/4): 将 NavMeshAgent 的速度传递给 Mimic ===
        if (myMimic != null)
        {
            // Mimic 脚本需要这个 velocity 变量来知道往哪里放腿
            myMimic.velocity = desiredVelocity;
        }
        // ====================================================
    }
    
    bool IsPointOnNavMesh(Vector3 position)
    {
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 0.1f, NavMesh.AllAreas))
        {
            return true;
        }
        return false;
    }
}