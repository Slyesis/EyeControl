using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("�����")]
    public MainPlayerMove mainPlayerMove;
    [Header("�����")]
    public NotMainPlayerMove notMainPlayerMove;

    [Header("���ع̶�����ֵ")]
    public float pixelDistance = 10f;
    [Header("���ص�������")]
    public float onePixelDistance = 1.0f;
    // Start is called before the first frame update

    [Header("True���False�۾�")]
    public bool isTestInput = true;

    public EyeTrackingReceiver trackingReceiver;

    public Gaze gaze;
    public float xPixel = 0;
    public float yPixel = 0;
    /// <summary>
    /// ����ӱ�
    /// </summary>
    private List<Vector2> vector2s = new List<Vector2>(4);

    public RectTransform imgRect;

    public bool isDebug = false;
    void Start()
    {
        Debug.unityLogger.logEnabled = isDebug;

        xPixel = Screen.width;
        yPixel = Screen.height;

        //mainPlayerMove.OnSetData();
        //notMainPlayerMove.OnSetData(mainPlayerMove);

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
        //1.�������Ǻͷ����ǵ���Ļ������
        Vector2 mainVec2 = Camera.main.WorldToScreenPoint(_vector3);
        //Debug.Log(mainVec2 + " ���������ص㣡");
        //Debug.Log(vector2 + " �������ص㣡");
        float _distance = Vector2.Distance(mainVec2, vector2);
        //Debug.Log(_distance+" ��ǰ���ؾ��룿");
        //2.���С�ڹ̶�ֵ����ʼ�Թ̶������ƶ�
        if (_distance <= pixelDistance)
        {
            bool hasMoved = false;
            if (OnMoveSucc(vector2, mainVec2, pixelDistance * 0.5f + onePixelDistance))
            {
                hasMoved = true;
            }
            //��������List
            // ���ֱ���ƶ�ʧ�ܣ������ĸ�����
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
                        break;  // �ҵ�һ�����з����ֹͣ
                    }
                }
                if (hasMoved)
                {
                    Debug.LogError("�ҵ��ˣ�����");
                }
                else
                {
                    Debug.LogError("û���ҵ�&&&&&&&&&&&&&&");
                }
                vector2s.Clear();
            }
        }
    }
    private bool OnMoveSucc(Vector2 vector2,Vector2 mainVec2,float moveDis) 
    {
        //��������ص�
        Vector2 extendVec2 = Tools.FindPointOnExtension(vector2, mainVec2, moveDis);
        //Debug.Log(extendVec2 + " �ӳ�֮�����������");
        //3.������ײNotMainPlayer������ֵ
        Vector3 vector3 = Vector3.zero;
        GameObject go = null;
        //��ȡ��ײ��λ�ú���ײ������
        Tools.OnBackHitPointAndGameObject(extendVec2, ref vector3, ref go, "NotMainPlane");
        Vector3 vector31 = Vector3.zero;
        GameObject go1 = null;
        Tools.OnBackHitPointAndGameObject(mainVec2, ref vector31, ref go1, "NotMainPlane");
        if (go == null)
        {
            return false;
        }
        //4.λ��ת��Ϊƽ������ϵ
        if (go1 == null)
        {
            //Debug.LogError("���λ���쳣��");
            return false;
        }
        //���λ��
        string xiangXian1 = go1.name;
        //����λ��
        string xiangXian2 = go.name;
        bool isMove = false;
        if (string.Equals(xiangXian1, "center") || string.Equals(xiangXian2, "center"))
        {
        }
        else if (!string.Equals(xiangXian1, xiangXian2))
        {
            //���ٱ߿��ʱ ִ��˲��
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
