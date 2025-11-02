using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class RemoteSculpt : MonoBehaviour
{
    [Header("Settings")]
    public float brushRadius = 0.5f;
    public float deformStrength = 0.1f;
    public float falloffPower = 2.0f; // 2 = 平滑的 "SmoothStep" 衰减
    public float sculptInterval = 5.0f; // <-- 这是新的、可在Inspector中编辑的计时器

    [Header("References")]
    public GameObject anchorVisual; // 将您创建的 "AnchorVisual" 拖到这里

    // 内部变量
    private Mesh mesh;
    private MeshCollider meshCollider;
    private Vector3[] originalVertices;
    private Vector3[] deformedVertices;

    private bool isSculpting = false;
    private float sculptTimer = 5.0f;
    // private const float SCULPT_INTERVAL = 5.0f; // <-- 旧的常量已被移除

    // 当前活动锚点的数据
    private Vector3 activeAnchorPoint_local;
    private Dictionary<int, float> activeAffectedVertices = new Dictionary<int, float>();

    // 鼠标拖动
    private Vector3 lastMouseScreenPos;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        meshCollider = GetComponent<MeshCollider>();

        // 存储原始顶点数据
        originalVertices = mesh.vertices;
        // 创建一个我们将要修改的工作副本
        deformedVertices = (Vector3[])originalVertices.Clone();

        // 确保高亮显示器在开始时是隐藏的
        if (anchorVisual != null)
        {
            anchorVisual.SetActive(false);
        }
    }

    void Update()
    {
        // 状态机：处理开始和结束
        if (Input.GetMouseButtonDown(0))
        {
            if (!isSculpting)
            {
                StartSculpting();
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (isSculpting)
            {
                StopSculpting();
            }
        }

        // 如果我们处于活动状态，则运行核心逻辑
        if (isSculpting)
        {
            HandleSculpting();
        }
    }

    void StartSculpting()
    {
        isSculpting = true;
        // 立即重置计时器并选择第一个锚点，使用 Inspector 中的值
        sculptTimer = sculptInterval; // <-- 已修改

        SelectNewAnchorPoint();

        // 存储初始鼠标位置
        lastMouseScreenPos = Input.mousePosition;
        
        if (anchorVisual != null)
        {
            anchorVisual.SetActive(true);
        }
    }

    void StopSculpting()
    {
        isSculpting = false;

        if (anchorVisual != null)
        {
            anchorVisual.SetActive(false);
        }

        // （关键性能步骤）
        // 仅在松开鼠标时才更新物理碰撞体
        UpdateMeshCollider();
    }

    // 在鼠标按住的每一帧都会调用
    void HandleSculpting()
    {
        // --- 1. 计时器逻辑 ---
        sculptTimer -= Time.deltaTime;
        if (sculptTimer <= 0)
        {
            SelectNewAnchorPoint();
            sculptTimer = sculptInterval; // <-- 已修改
        }

        // --- 2. 拖动变形逻辑 ---
        Vector3 mouseDelta = Input.mousePosition - lastMouseScreenPos;

        // 检查鼠标是否真的移动了
        if (mouseDelta.sqrMagnitude > 0.01f) 
        {
            // 将 2D 屏幕移动转换为 3D 世界空间拉力
            Vector3 worldPullVector = Calculate3DPullVector(mouseDelta);
            
            // 将这个拉力应用到当前的活动锚点
            ApplyDeformation(worldPullVector);
        }

        // 更新下一帧的鼠标位置
        lastMouseScreenPos = Input.mousePosition;
    }

    void SelectNewAnchorPoint()
    {
        // --- 1. 在模型表面找到一个随机点 ---
        // 我们从一个随机方向向模型发射一条射线

        // 起点在世界中心（假设模型在0,0,0）附近，向外偏移
        Vector3 randomDir = Random.onUnitSphere;
        Vector3 rayStartPoint = transform.position - randomDir * 10f; // 从 "外面" 射 "进来"
        
        Ray ray = new Ray(rayStartPoint, randomDir);
        RaycastHit hit;

        if (meshCollider.Raycast(ray, out hit, 20.0f))
        {
            // 成功击中！
            // 将世界空间撞击点转换为模型的局部空间
            activeAnchorPoint_local = transform.InverseTransformPoint(hit.point);

            // 更新高亮显示器的世界位置
            if (anchorVisual != null)
            {
                anchorVisual.transform.position = hit.point;
            }
        }
        else
        {
            // 如果射线没打中（不太可能，但作为备用），就随便选一个顶点
            int randomIndex = Random.Range(0, originalVertices.Length);
            activeAnchorPoint_local = originalVertices[randomIndex];
            
            if (anchorVisual != null)
            {
                anchorVisual.transform.position = transform.TransformPoint(activeAnchorPoint_local);
            }
        }

        // --- 2. 预先计算受影响的顶点及其衰减强度 ---
        activeAffectedVertices.Clear();

        for (int i = 0; i < deformedVertices.Length; i++)
        {
            // 注意：我们用 `deformedVertices` 来计算距离，
            // 这样变形就可以在已变形的区域上"叠加"
            float dist = Vector3.Distance(deformedVertices[i], activeAnchorPoint_local);

            if (dist < brushRadius)
            {
                // 计算衰减 (0.0 到 1.0)
                // 使用 Mathf.Pow 来获得一个更平滑的曲线，而不是线性下降
                float falloff = Mathf.Pow(1.0f - (dist / brushRadius), falloffPower);
                
                // 存储这个顶点的索引和它的变形强度
                activeAffectedVertices[i] = falloff;
            }
        }
    }

    Vector3 Calculate3DPullVector(Vector2 mouseDelta)
    {
        // "遥控器" 逻辑
        // 获取摄像机的 "右" 和 "上" 方向
        Transform camTransform = Camera.main.transform;
        Vector3 camRight = camTransform.right;
        Vector3 camUp = camTransform.up;

        // 将 2D 鼠标增量转换为相对于摄像机的 3D 移动
        // (忽略 camForward，这样拖动就不会推/拉)
        Vector3 pullVector = (camRight * mouseDelta.x + camUp * mouseDelta.y);

        // 应用强度和 Time.deltaTime (使其帧率无关)
        pullVector *= deformStrength * Time.deltaTime;

        return pullVector;
    }

    void ApplyDeformation(Vector3 worldPullVector)
    {
        // 变形向量是世界空间的，但顶点是局部空间的。
        // 我们需要将 "拉力" 向量转换为局部空间。
        Vector3 localPullVector = transform.InverseTransformDirection(worldPullVector);

        // 遍历我们之前算好的、受影响的顶点
        foreach (KeyValuePair<int, float> vertexData in activeAffectedVertices)
        {
            int index = vertexData.Key;
            float falloff = vertexData.Value;

            // 将 "拉力" * 衰减强度 应用到顶点
            deformedVertices[index] += localPullVector * falloff;
        }

        // --- 实时更新视觉网格 ---
        // 这是让模型在屏幕上看起来变形的关键
        mesh.vertices = deformedVertices;
        mesh.RecalculateNormals(); // 重新计算光照
    }

    void UpdateMeshCollider()
    {
        // 这是性能消耗最高的操作，所以我们只在松手时调用
        meshCollider.sharedMesh = null; // 必须先清空
        meshCollider.sharedMesh = mesh; // 再分配新的变形网格
    }
}