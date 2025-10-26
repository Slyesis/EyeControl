using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("主玩家")]
    public MainPlayerMove mainPlayerMove;
    [Header("从玩家")]
    public NotMainPlayerMove notMainPlayerMove;

    [Header("像素固定距离值")]
    public float pixelDistance = 10f;
    [Header("像素单个步长")]
    public float onePixelDistance = 1.0f;
    // Start is called before the first frame update

    [Header("True鼠标False眼睛")]
    public bool isTestInput = true;

    public EyeTrackingReceiver trackingReceiver;

    public Gaze gaze;
    public float xPixel = 0;
    public float yPixel = 0;
    /// <summary>
    /// 随机逃避
    /// </summary>
    private List<Vector2> vector2s = new List<Vector2>(4);

    public RectTransform imgRect;

    public bool isDebug = false;
    void Start()
    {
        Debug.unityLogger.logEnabled = isDebug;

        xPixel = Screen.width;
        yPixel = Screen.height;

        mainPlayerMove.OnSetData();
        notMainPlayerMove.OnSetData(mainPlayerMove);

    }
    // Update is called once per frame
    void Update()
    {
        if (isTestInput)
        {
            if (Input.GetMouseButton(0))
            {
                OnMove(Input.mousePosition);
            }
        }
        else
        {
            OnMove(new Vector2(trackingReceiver._pixelX, yPixel - trackingReceiver._pixelY));
        }
    }
    private void OnMove(Vector2 vector2) 
    {
        imgRect.anchoredPosition = vector2;
        Vector3 _vector3 = Tools.OnBackNotMainPlayerPos(notMainPlayerMove.transform.position);
        //1.计算主角和非主角的屏幕坐标差距
        Vector2 mainVec2 = Camera.main.WorldToScreenPoint(_vector3);
        //Debug.Log(mainVec2 + " 非主角像素点！");
        //Debug.Log(vector2 + " 传入像素点！");
        float _distance = Vector2.Distance(mainVec2, vector2);
        //Debug.Log(_distance+" 当前像素距离？");
        //2.差距小于固定值，开始以固定像素移动
        if (_distance <= pixelDistance)
        {
            bool hasMoved = false;
            if (OnMoveSucc(vector2, mainVec2, pixelDistance * 0.5f + onePixelDistance))
            {
                hasMoved = true;
            }
            //方向闪躲List
            // 如果直接移动失败，尝试四个方向
            if (!hasMoved)
            {
                Debug.Log(mainVec2);
                vector2s = Tools.RandomVec2(mainVec2, pixelDistance);
                for (int i = 0; i < vector2s.Count; i++)
                {
                    Debug.Log(vector2s[i].ToString()+" ***");
                    if (OnMoveSucc(vector2s[i], mainVec2, pixelDistance * 2))
                    {
                        hasMoved = true;
                        break;  // 找到一个可行方向就停止
                    }
                }
                if (hasMoved)
                {
                    Debug.LogError("找到了！！！");
                }
                else
                {
                    Debug.LogError("没有找到&&&&&&&&&&&&&&");
                }
                vector2s.Clear();
            }
        }
    }
    private bool OnMoveSucc(Vector2 vector2,Vector2 mainVec2,float moveDis) 
    {
        //延申的像素点
        Vector2 extendVec2 = Tools.FindPointOnExtension(vector2, mainVec2, moveDis);
        //Debug.Log(extendVec2 + " 延长之后的像素坐标");
        //3.计算碰撞NotMainPlayer的坐标值
        Vector3 vector3 = Vector3.zero;
        GameObject go = null;
        //获取碰撞的位置和碰撞的物体
        Tools.OnBackHitPointAndGameObject(extendVec2, ref vector3, ref go, "NotMainPlane");
        Vector3 vector31 = Vector3.zero;
        GameObject go1 = null;
        Tools.OnBackHitPointAndGameObject(mainVec2, ref vector31, ref go1, "NotMainPlane");
        if (go == null)
        {
            return false;
        }
        //4.位置转化为平面坐标系
        if (go1 == null)
        {
            //Debug.LogError("点击位置异常！");
            return false;
        }
        //点击位置
        string xiangXian1 = go1.name;
        //延申位置
        string xiangXian2 = go.name;
        bool isMove = false;
        if (string.Equals(xiangXian1, "center") || string.Equals(xiangXian2, "center"))
        {
        }
        else if (!string.Equals(xiangXian1, xiangXian2))
        {
            //在临边跨界时 执行瞬移
            isMove = true;
        }
        else
        {
        }
        Vector3 vector311 = Tools.OnCalculate2DPosition(go.transform, vector3);
        mainPlayerMove.OnMove(vector311, isMove);
        return true;
    }
}
