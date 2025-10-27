using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class DeformableSphere : MonoBehaviour
{
    [Header("变形设置")]
    [Tooltip("变形的强度")]
    public float deformationStrength = 0.5f;

    private Mesh mesh;
    private MeshFilter meshFilter;
    private Vector3[] originalVertices;
    private Vector3[] deformedVertices;
    private float minY, maxY;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh = Instantiate(meshFilter.mesh);
        originalVertices = mesh.vertices;
        deformedVertices = new Vector3[originalVertices.Length];
        CalculateBounds();
    }

    void CalculateBounds()
    {
        minY = float.MaxValue;
        maxY = float.MinValue;
        foreach (Vector3 v in originalVertices)
        {
            if (v.y < minY) minY = v.y;
            if (v.y > maxY) maxY = v.y;
        }
        Debug.Log($"[DeformableSphere] Y轴边界已计算: Min={minY}, Max={maxY}");
    }

    public void ResetDeformation()
    {
        if (mesh == null || originalVertices == null) return;
        mesh.vertices = originalVertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    public void TriggerDeformation(List<Vector3> trajectory)
    {
        Debug.Log("--- 阶段三：开始变形 (TriggerDeformation 已被调用) ---");

        if (trajectory == null || trajectory.Count < 2)
        {
            Debug.LogWarning("轨迹点不足，无法变形。"); 
            return;
        }

        // 1. 获取世界坐标系的变形向量
        Vector3 worldDeformationVector = trajectory[trajectory.Count - 1] - trajectory[0];
        Debug.Log($"[DeformableSphere] 世界变形向量 (World): {worldDeformationVector.ToString("F4")}");
        
        // 2. 检查强度
        Debug.Log($"[DeformableSphere] 变形强度 (Strength): {deformationStrength}");
        if (Mathf.Approximately(deformationStrength, 0f))
        {
            Debug.LogError("[DeformableSphere] 变形强度为 0！无法变形。");
            return; 
        }

        // --- 【【【【【这是修复】】】】】 ---
        // 3. 将 "世界" 变形向量 转换为 "局部" 变形向量
        //    这会考虑物体当前的旋转状态。
        Vector3 localDeformationVector = transform.InverseTransformDirection(worldDeformationVector);
        Debug.Log($"[DeformableSphere] 局部变形向量 (Local): {localDeformationVector.ToString("F4")}");
        // --- 【【【【【修复结束】】】】】 ---


        if (Mathf.Approximately(minY, maxY))
        {
            Debug.LogError("[DeformableSphere] Y轴边界无效 (Min == Max)，无法计算权重。");
            return; 
        }

        System.Array.Copy(originalVertices, deformedVertices, originalVertices.Length);
        
        for (int i = 0; i < deformedVertices.Length; i++)
        {
            Vector3 originalPos = originalVertices[i];
            
            float weight = Mathf.InverseLerp(minY, maxY, originalPos.y);

            // 4. 现在我们使用 "localDeformationVector"
            deformedVertices[i] = originalPos + (localDeformationVector * weight * deformationStrength);
        }

        mesh.vertices = deformedVertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        Debug.Log("--- 变形已应用到网格 (Deformation Applied) ---");
    }
}