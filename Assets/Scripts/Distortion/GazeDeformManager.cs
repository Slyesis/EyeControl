using UnityEngine;
using System.Collections.Generic;

public class GazeDeformManager : MonoBehaviour
{
    [Header("变形目标")]
    [Tooltip("即 MainPlayer")]
    public DeformableSphere deformableMesh; 

    [Header("移动目标")]
    [Tooltip("即 MainPlayer")]
    public MainPlayerMove mainPlayerMove;

    // --- 新增：躲避逻辑需要 NotMainPlayer ---
    [Header("躲避逻辑参考")]
    [Tooltip("即 NotMainPlayer")]
    public NotMainPlayerMove notMainPlayerMove; // 你使用的是 NotMainPlayerMove 还是 NotMainPlayerMove2? 拖拽对应的脚本
    // --- 新增结束 ---

    [Header("输入设置")]
    public bool useMouseInput = true;
    public EyeTrackingReceiver trackingReceiver;

    [Header("录制设置")]
    [Tooltip("射线可以命中的物体标签")]
    public string recordingSurfaceTag = "MainPlane";
    [Tooltip("射线可以命中的物体标签 (用于躲避)")]
    public string avoidSurfaceTag = "NotMainPlane"; // 来自 PlayerManager.cs

    // --- 新增：从 PlayerManager.cs 拷贝来的字段 ---
    [Header("躲避逻辑设置 (来自 PlayerManager)")]
    public float pixelDistance = 200f;
    public float onePixelDistance = 20f;
    public RectTransform imgRect; // 调试UI
    private List<Vector2> vector2s = new List<Vector2>(4);
    // --- 新增结束 ---

    // 内部变量
    private List<Vector3> gazeTrajectory = new List<Vector3>(); // 专门用于存储凝视轨迹
    private bool isRecording = false;
    private float screenHeight;

    void Start()
    {
        // ... (保留 deformableMesh 和 mainPlayerMove 的 null 检查) ...
        if (deformableMesh == null || mainPlayerMove == null)
        {
            Debug.LogError("GazeDeformManager: 'Deformable Mesh' 或 'Main Player Move' 未设置！");
            enabled = false;
            return;
        }

        if (notMainPlayerMove == null)
        {
            Debug.LogError("GazeDeformManager: 'Not Main Player Move' 未设置！躲避逻辑需要它。");
            enabled = false;
            return;
        }

        screenHeight = Screen.height;
    }

    void Update()
    {
        if (useMouseInput)
        {
            HandleMouseInput();
        }
        else
        {
            HandleEyeTrackingInput();
        }
    }

    // --- 鼠标输入（已更新） ---
    void HandleMouseInput()
    {
        Vector2 mousePos = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            isRecording = true;
            gazeTrajectory.Clear();
            deformableMesh.ResetDeformation();
            Debug.Log("开始录制轨迹 & 躲避...");
        }

        if (isRecording && Input.GetMouseButton(0))
        {
            // 1. 录制凝视轨迹（只录制，不移动）
            RecordGaze3DPoint(mousePos); 

            // 2. 执行躲避逻辑（只移动，不录制）
            OnMove_Dodge(mousePos);
        }

        if (isRecording && Input.GetMouseButtonUp(0))
        {
            isRecording = false;
            Debug.Log("录制停止。轨迹点数: " + gazeTrajectory.Count);

            // 3. 使用“凝视轨迹”来变形
            if (gazeTrajectory.Count > 1)
            {
                deformableMesh.TriggerDeformation(gazeTrajectory);
            }
        }
    }
    
    // --- 眼动输入（已更新） ---
    void HandleEyeTrackingInput()
    {
        if (trackingReceiver == null) return;

        // (我们仍然使用空格键来启动/停止，因为你的Python脚本没有发送"停止"信号)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isRecording = !isRecording; // 切换状态
            if (isRecording)
            {
                gazeTrajectory.Clear();
                deformableMesh.ResetDeformation();
                Debug.Log("眼动录制开始 & 躲避...");
            }
            else
            {
                Debug.Log("眼动录制停止。轨迹点数: " + gazeTrajectory.Count);
                if (gazeTrajectory.Count > 1)
                {
                    deformableMesh.TriggerDeformation(gazeTrajectory);
                }
            }
        }

        if (isRecording)
        {
            // 匹配 PlayerManager.cs 的Y轴翻转
            float flippedY = screenHeight - trackingReceiver._pixelY; 
            Vector2 eyePos = new Vector2(trackingReceiver._pixelX, flippedY);

            // 1. 录制凝视轨迹
            RecordGaze3DPoint(eyePos);
            // 2. 执行躲避逻辑
            OnMove_Dodge(eyePos);
        }
    }

    /// <summary>
    /// 仅录制凝视轨迹到列表
    /// </summary>
    void RecordGaze3DPoint(Vector2 screenPosition)
    {
        Vector3 hitPoint = Vector3.zero;
        GameObject hitObject = null;
        Tools.OnBackHitPointAndGameObject(screenPosition, ref hitPoint, ref hitObject, recordingSurfaceTag);

        if (hitObject != null)
        {
            gazeTrajectory.Add(hitPoint);
        }
    }

    // --- 以下是 PlayerManager.cs 中的“躲避”逻辑，被完整地拷贝了过来 ---

    private void OnMove_Dodge(Vector2 vector2) 
    {
        if (imgRect != null) imgRect.anchoredPosition = vector2;

        Vector3 _vector3 = Tools.OnBackNotMainPlayerPos(notMainPlayerMove.transform.position);
        Vector2 mainVec2 = Camera.main.WorldToScreenPoint(_vector3);
        
        float _distance = Vector2.Distance(mainVec2, vector2);
        
        if (_distance <= pixelDistance)
        {
            bool hasMoved = false;
            if (OnMoveSucc_Dodge(vector2, mainVec2, pixelDistance * 0.5f + onePixelDistance))
            {
                hasMoved = true;
            }
            
            if (!hasMoved)
            {
                vector2s = Tools.RandomVec2(mainVec2, pixelDistance);
                for (int i = 0; i < vector2s.Count; i++)
                {
                    if (OnMoveSucc_Dodge(vector2s[i], mainVec2, pixelDistance * 2))
                    {
                        hasMoved = true;
                        break;
                    }
                }
                vector2s.Clear();
            }
        }
    }
    
    private bool OnMoveSucc_Dodge(Vector2 vector2, Vector2 mainVec2, float moveDis) 
    {
        Vector2 extendVec2 = Tools.FindPointOnExtension(vector2, mainVec2, moveDis);
        
        Vector3 vector3 = Vector3.zero;
        GameObject go = null;
        // 注意：这里使用 "avoidSurfaceTag"
        Tools.OnBackHitPointAndGameObject(extendVec2, ref vector3, ref go, avoidSurfaceTag);

        Vector3 vector31 = Vector3.zero;
        GameObject go1 = null;
        Tools.OnBackHitPointAndGameObject(mainVec2, ref vector31, ref go1, avoidSurfaceTag);

        if (go == null || go1 == null)
        {
            return false;
        }
        
        string xiangXian1 = go1.name;
        string xiangXian2 = go.name;
        bool isMove = false;
        if (string.Equals(xiangXian1, "center") || string.Equals(xiangXian2, "center"))
        {
        }
        else if (!string.Equals(xiangXian1, xiangXian2))
        {
            isMove = true;
        }
        
        Vector3 vector311 = Tools.OnCalculate2DPosition(go.transform, vector3);
        
        // 命令 MainPlayer 移动
        mainPlayerMove.OnMove(vector311, isMove);
        return true;
    }
}