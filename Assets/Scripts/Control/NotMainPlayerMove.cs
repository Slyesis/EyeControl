using System;
using UnityEngine;
using UnityEngine.AI;

public class NotMainPlayerMove : MonoBehaviour
{
    [HideInInspector]
    public Transform mainPlayer;

    public BindableProperty<Vector3> mainPlayerRQ = new BindableProperty<Vector3>();
    public void OnSetData(MainPlayerMove mainPlayerMove)
    {
        mainPlayer = mainPlayerMove.transform;
        mainPlayerRQ.OnValueChanged = OnChangePosition;
    }
    private void Update()
    {
        if (mainPlayer == null)
        {
            return;
        }
        mainPlayerRQ.Value = mainPlayer.position;
    }
    /// <summary>
    /// 盒子内小球同步平面小球位置
    /// </summary>
    /// <param name="vector"></param>
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
