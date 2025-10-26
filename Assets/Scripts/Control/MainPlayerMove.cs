using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
public class MainPlayerMove : MonoBehaviour
{
    [HideInInspector]
    public NavMeshAgent navMeshAgent;

    private Vector3 desiredVelocity;
    private Vector3 smoothPositionRef; // 添加位置平滑参考值

    [Header("小球速度")]
    public float moveSpeed = 0.1f;
    public void OnSetData()
    {
        navMeshAgent = this.GetComponent<NavMeshAgent>();

        // 关闭自动速度控制
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

        // 每帧设置目标让NavMesh计算避障 
        navMeshAgent.nextPosition = transform.position;
        navMeshAgent.destination = vector3;
        // 获取NavMesh计算出的避障方向
        if (navMeshAgent.hasPath)
        {
            desiredVelocity = navMeshAgent.desiredVelocity;
        }
        else
        {
            // 没有路径时直接朝向目标
            desiredVelocity = (vector3 - transform.position).normalized * navMeshAgent.speed;
        }

        // 手动应用移动（避免NavMesh的路径查找干扰）
        ApplyManualMovement(vector3, isUpdateMove);
    }
    void ApplyManualMovement(Vector3 vector3, bool isUpdateMove)
    {
        // 使用NavMesh计算的避障方向，但手动控制移动
        if (desiredVelocity.magnitude > 0.1f)
        {
            Vector3 newPosition = transform.position + desiredVelocity * Time.deltaTime;
            // 确保新位置在NavMesh上
            if (NavMesh.SamplePosition(newPosition, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                // 平滑移动到目标位置
                transform.position = Vector3.SmoothDamp(transform.position, hit.position, ref smoothPositionRef, moveSpeed);

                navMeshAgent.nextPosition = transform.position;
            }
            // 旋转朝向移动方向
            if (desiredVelocity.magnitude > 0.5f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(desiredVelocity);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
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
