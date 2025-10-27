using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
public class MainPlayerMove : MonoBehaviour
{
    [HideInInspector]
    public NavMeshAgent navMeshAgent;

    private Vector3 desiredVelocity;
    private Vector3 smoothPositionRef; // ����λ��ƽ���ο�ֵ

    [Header("С���ٶ�")]
    public float moveSpeed = 0.1f;
    void Start()
    {
        navMeshAgent = this.GetComponent<NavMeshAgent>();

        // �ر��Զ��ٶȿ���
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

        // ÿ֡����Ŀ����NavMesh������� 
        navMeshAgent.nextPosition = transform.position;
        navMeshAgent.destination = vector3;
        // ��ȡNavMesh������ı��Ϸ���
        if (navMeshAgent.hasPath)
        {
            desiredVelocity = navMeshAgent.desiredVelocity;
        }
        else
        {
            // û��·��ʱֱ�ӳ���Ŀ��
            desiredVelocity = (vector3 - transform.position).normalized * navMeshAgent.speed;
        }

        // �ֶ�Ӧ���ƶ�������NavMesh��·�����Ҹ��ţ�
        ApplyManualMovement(vector3, isUpdateMove);
    }
    void ApplyManualMovement(Vector3 vector3, bool isUpdateMove)
    {
        // ʹ��NavMesh����ı��Ϸ��򣬵��ֶ������ƶ�
        if (desiredVelocity.magnitude > 0.1f)
        {
            Vector3 newPosition = transform.position + desiredVelocity * Time.deltaTime;
            // ȷ����λ����NavMesh��
            if (NavMesh.SamplePosition(newPosition, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                // ƽ���ƶ���Ŀ��λ��
                transform.position = Vector3.SmoothDamp(transform.position, hit.position, ref smoothPositionRef, moveSpeed);

                navMeshAgent.nextPosition = transform.position;
            }
            // ��ת�����ƶ�����
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
