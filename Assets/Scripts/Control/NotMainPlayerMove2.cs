using System;
using UnityEngine;
using UnityEngine.AI;

// 1. 确保类名 (NotMainPlayerMove2) 与文件名 (NotMainPlayerMove2.cs) 一致
// 2. 确保它继承 (derive from) MonoBehaviour
public class NotMainPlayerMove2 : MonoBehaviour
{
    [Header("要跟随的目标")]
    [Tooltip("请把场景中的 MainPlayer 物体拖到这里")]
    public MainPlayerMove mainPlayerMoveRef; // <-- 我们用这个新字段来代替 OnSetData

    [HideInInspector]
    public Transform mainPlayer;

    public BindableProperty<Vector3> mainPlayerRQ = new BindableProperty<Vector3>();
    
    // 3. 我们使用 Start() 来进行自我初始化
    void Start()
    {
        // 检查 mainPlayerMoveRef 是否已在 Inspector 中设置
        if (mainPlayerMoveRef == null)
        {
            Debug.LogError("NotMainPlayerMove: 'Main Player Move Ref' 未设置！请在 Inspector 中拖拽 MainPlayer。");
            enabled = false; // 禁用此脚本以防止报错
            return;
        }

        // 这是 OnSetData 以前的逻辑
        mainPlayer = mainPlayerMoveRef.transform;
        mainPlayerRQ.OnValueChanged = OnChangePosition;
    }

    // 4. 你原来的 Update 逻辑，完全不变
    private void Update()
    {
        if (mainPlayer == null)
        {
            return;
        }
        mainPlayerRQ.Value = mainPlayer.position;
    }

    // 5. 你原来的 OnChangePosition 逻辑，完全不变
    private void OnChangePosition(Vector3 vector)
    {
        if (!Tools.OnAgent(vector))
        {
            return;
        }
        transform.position = Tools.On2DPositionCalculate(vector);
        float _x = transform.position.x;
        float _z = transform.position.z;
        _x = _x > 4.5f ? 4.5f : _x;
        _x = _x < -4.5f ? -4.5f : _x;
        _z = _z > 4.5f ? 4.5f : _z;
        _z = _z < -4.5f ? -4.5f : _z;
        transform.position = new Vector3(_x, transform.position.y, _z);
    }
}