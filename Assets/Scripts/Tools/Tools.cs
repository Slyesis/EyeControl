using System.Collections.Generic;
using UnityEngine;

public class Tools : MonoBehaviour
{
    private static RaycastHit[] hits = new RaycastHit[10]; // 预分配一个足够大的数组
    /// <summary>
    /// 返回碰撞点
    /// </summary>
    /// <returns></returns>
    public static void OnBackHitPointAndGameObject(Vector2 vector2, ref Vector3 vector3, ref GameObject go, string _tag)
    {
        Ray ray = Camera.main.ScreenPointToRay(vector2);
        int hitCount = Physics.RaycastNonAlloc(ray, hits); // 使用 RaycastNonAlloc 避免分配新数组
        for (int i = 0; i < hitCount; i++) // 使用 for 循环遍历
        {
            if (hits[i].collider.CompareTag(_tag)) // 检查是否是 Player
            {
                // 返回 Player 的碰撞位置
                vector3 = hits[i].point;
                go = hits[i].collider.gameObject;
                break;
            }
        }
    }
    /// <summary>
    /// 非玩家换算接触边位置
    /// </summary>
    /// <param name="vector3"></param>
    /// <returns></returns>
    public static Vector3 OnBackNotMainPlayerPos(Vector3 vector3) 
    {
        Vector3 _vec = vector3;
        if (vector3.x>=4.5f)
        {
            _vec.x = 5;
        }
        if (vector3.z>= 4.5f)
        {
            _vec.z = 5;
        }
        if (vector3.x <=-4.5f)
        {
            _vec.x = -5;
        }
        if (vector3.z <= -4.5f)
        {
            _vec.z = -5;
        }
        return _vec;
    }
    /// <summary>
    /// 根据像素计算延长线上的点
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="dist"></param>
    /// <returns></returns>
    public static Vector2 FindPointOnExtension(Vector2 a, Vector2 b, float dist = 1)
    {
        Vector2 ab = b - a;
        float length = ab.magnitude;

        // 如果 a 和 b 重合，无法确定方向，直接返回 b
        if (length < Mathf.Epsilon)
        {
            Debug.Log("无法确定方向！");
            return b;
        }
        Vector2 unit = ab / length;
        Vector2 c = b + unit * dist;
        return c;
    }
    /// <summary>
    /// 从盒子侧面位置计算平面位置
    /// </summary>
    public static Vector3 OnCalculate2DPosition(Transform to, Vector3 axis)
    {
        // 安全地解析枚举，如果解析失败使用默认值
        PlaneType planeType;
        if (!System.Enum.TryParse(to.name, out planeType))
        {
            Debug.LogWarning($"无法将 '{to.name}' 转换为 PlaneType 枚举");
            return axis;
        }
        float _x = 0;
        float _y = 0f;
        float _z = 0;
        switch (planeType)
        {
            case PlaneType.up:
                _x = axis.x;
                _z = axis.y + 5;
                break;
            case PlaneType.down:
                _x = axis.x;
                _z = -axis.y-5f;
                break;
            case PlaneType.center:
                _x = axis.x;
                _z = axis.z;
                break;
            case PlaneType.left:
                _x = -axis.y - 5f;
                _z = axis.z;
                break;
            case PlaneType.right:
                _x = axis.y + 5f;
                _z = axis.z;
                break;
            default:
                return axis;
        }
        Vector3 _vector3 = new Vector3(_x, _y, _z);
        //Debug.Log(planeType.ToString()+"  "+ _vector3);
        return _vector3;
    }
    /// <summary>
    /// 从平面位置计算盒子侧面位置
    /// </summary>
    public static Vector3 On2DPositionCalculate(Vector3 axis)
    {
        // 安全地解析枚举，如果解析失败使用默认值
        PlaneType planeType;
        float _x1 = axis.x;
        float _z1 = axis.z;
        if (_x1 < -5)
        {
            planeType = PlaneType.left;
        }
        else if (_x1 > 5)
        {
            planeType = PlaneType.right;
        }
        else if (_z1 > 5)
        {
            planeType = PlaneType.up;
        }
        else if (_z1 < -5)
        {
            planeType = PlaneType.down;
        }
        else
        {
            planeType = PlaneType.center;
        }
        float _x = 0;
        float _y = 0;
        float _z = 0;
        switch (planeType)
        {
            case PlaneType.up:
                _x = axis.x ;
                _y = axis.z - 5;
                _z = 5;
                break;
            case PlaneType.down:
                _x = axis.x;
                _y = Mathf.Abs(axis.z) - 5;
                _z = -5 ;
                break;
            case PlaneType.center:
                _x = axis.x;
                _y = axis.y;
                _z = axis.z;
                break;
            case PlaneType.left:
                _x = -5;
                _y = Mathf.Abs(axis.x) - 5;
                _z = axis.z;
                break;
            case PlaneType.right:
                _x = 5 ;
                _y = axis.x - 5;
                _z = axis.z;
                break;
            default:
                return axis;
        }
        return new Vector3(_x, _y, _z);
    }
    public static bool OnAgent(Vector3 axis) 
    {
        if (axis.x>5 && axis.z > 5)
        {
            return false;
        }
        if (axis.x > 5 && axis.z <-5)
        {
            return false;
        }
        if (axis.x <-5 && axis.z >5)
        {
            return false;
        }
        if (axis.x < -5 && axis.z <-5)
        {
            return false;
        }
        return true;
    }
    public static string OnCompareXiangXian(Vector3 axis) 
    {
        PlaneType planeType = PlaneType.center;
        float _x1 = axis.x;
        float _z1 = axis.z;
        if (_x1 < -5)
        {
            planeType = PlaneType.left;
        }
        else if (_x1 > 5)
        {
            planeType = PlaneType.right;
        }
        else if (_z1 > 5)
        {
            planeType = PlaneType.up;
        }
        else if (_z1 < -5)
        {
            planeType = PlaneType.down;
        }
        else
        {
            planeType = PlaneType.center;
        }
        return planeType.ToString();
    }
    /// <summary>
    /// 当前Nav到边界向邻边转换
    /// </summary>
    public static Vector3 OnChangePosition(Vector3 vector3) 
    {

        float _x1 = vector3.x;
        float _z1 = vector3.z;
        float _x2 = _x1;
        float _z2 = _z1;
        //第一象限和第三象限都是更换
        if (_x1 > 0 && _z1>0)
        {
            _x2 = _z1;
            _z2 = _x1;
        }
        if (_x1 < 0 && _z1 < 0)
        {
            _x2 = _z1;
            _z2 = _x1;
        }
        if (_x1 < 0 && _z1 > 0)
        {
            _x2 = -_z1;
            _z2 = -_x1;
        }
        if (_x1 > 0  && _z1 < 0)
        {
            _x2 = _z1;
            _z2 = -_x1;
        }
        return new Vector3(_x2, vector3.y, _z2);
    }
    /// <summary>
    /// 随机四个方向像素值
    /// </summary>
    /// <param name="vector2"></param>
    /// <param name="_pixelDis"></param>
    /// <returns></returns>
    public static List<Vector2> RandomVec2(Vector2 center, float pixelDis)
    {
        List<Vector2> vector2s = new List<Vector2>(4);

        vector2s.Add(new Vector2(center.x + pixelDis, center.y));
        vector2s.Add(new Vector2(center.x, center.y + pixelDis));
        vector2s.Add(new Vector2(center.x - pixelDis, center.y));
        vector2s.Add(new Vector2(center.x, center.y - pixelDis));
        // Fisher-Yates 洗牌算法
        for (int i = vector2s.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Vector2 temp = vector2s[i];
            vector2s[i] = vector2s[j];
            vector2s[j] = temp;
        }
        return vector2s;
    }
}
public enum PlaneType 
{
    up, down, center,left, right,
}