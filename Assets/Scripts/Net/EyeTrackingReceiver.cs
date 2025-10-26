using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class EyeTrackingReceiver : MonoBehaviour
{
    [Header("网络设置")]
    [Tooltip("监听端口，需与Python脚本一致")]
    public int port = 65432;
    
    [Header("小球控制 (可选，用于调试)")]
    [Tooltip("关联场景中的GazeIndicator小球")]
    public Transform gazeIndicator;
    
    [Tooltip("小球移动范围（X轴）")]
    public float moveRangeX = 10f;

    [Tooltip("小球移动范围（Y轴）")]
    public float moveRangeY = 6f;

    [Header("坐标转换设置")]
    [Tooltip("Python发送坐标对应的屏幕宽度（像素）")]
    public float senderScreenWidth = 1920f;
    
    [Tooltip("Python发送坐标对应的屏幕高度（像素）")]
    public float senderScreenHeight = 1080f;

    // --- 新增代码 ---
    // 创建一个静态变量，用于存储最新的眼动屏幕坐标
    // 任何脚本都可以通过 EyeTrackingReceiver.GazeScreenPosition 来访问它
    public static Vector2 GazeScreenPosition { get; private set; }
    // --- 新增代码结束 ---

    private TcpListener server;
    private TcpClient client;
    private NetworkStream stream;
    private byte[] receiveBuffer = new byte[1024];

    public float _pixelX = 0;
    public float _pixelY = 0;
    private void Start()
    {
        // 小球现在是可选的了
        if (gazeIndicator == null)
        {
            Debug.LogWarning("未关联GazeIndicator小球，将仅更新数据，不移动调试对象。");
        }

        StartServer();
    }

    private void StartServer()
    {
        try
        {
            server = new TcpListener(IPAddress.Loopback, port);
            server.Start();
            Debug.Log($"✅ 服务器已启动，正在端口 {port} 监听连接...");
            server.BeginAcceptTcpClient(OnClientConnected, null);
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ 服务器启动失败: {ex.Message}");
        }
    }

    private void OnClientConnected(IAsyncResult ar)
    {
        try
        {
            client = server.EndAcceptTcpClient(ar);
            stream = client.GetStream();
            Debug.Log("✅ Python客户端已连接");
            ReceiveData();
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ 客户端连接失败: {ex.Message}");
            server.BeginAcceptTcpClient(OnClientConnected, null);
        }
    }

    private void ReceiveData()
    {
        if (stream == null) return;
        try
        {
            stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, OnDataReceived, null);
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ 接收数据失败: {ex.Message}");
            CloseConnection();
        }
    }

    private void OnDataReceived(IAsyncResult ar)
    {
        try
        {
            if (stream == null) return;
            int bytesRead = stream.EndRead(ar);
            if (bytesRead > 0)
            {
                string data = Encoding.UTF8.GetString(receiveBuffer, 0, bytesRead).Trim();
                string[] coordinates = data.Split(',');
                if (coordinates.Length == 2 && float.TryParse(coordinates[0], out float x) && float.TryParse(coordinates[1], out float y))
                {
                    // 使用Dispatcher在主线程更新数据
                    UnityMainThreadDispatcher.Instance.Enqueue(() => 
                    {
                        UpdateGazeData(x, y);
                    });
                }
                else
                {
                    Debug.LogWarning($"⚠️ 数据格式错误: {data}");
                }
                ReceiveData();
            }
            else
            {
                Debug.Log("⚠️ 客户端已断开连接");
                CloseConnection();
                server.BeginAcceptTcpClient(OnClientConnected, null);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ 处理数据时出错: {ex.Message}");
            CloseConnection();
            server.BeginAcceptTcpClient(OnClientConnected, null);
        }
    }

    // 将原来的 UpdateIndicatorPosition 方法改名为 UpdateGazeData
    private void UpdateGazeData(float pixelX, float pixelY)
    {
        Debug.Log(pixelX+"  ???  "+ pixelY);
        _pixelX = pixelX;
        _pixelY = pixelY;
        //// --- 核心修改 ---
        //// Unity的屏幕坐标原点(0,0)在左下角，而眼动数据通常以左上角为原点
        //// 因此我们需要翻转Y轴
        //float unityPixelY = senderScreenHeight - pixelY;
        //GazeScreenPosition = new Vector2(pixelX, unityPixelY);
        //// --- 修改结束 ---

        //// 更新调试用小球的位置（这部分逻辑保持不变，如果不需要可以删除）
        //if (gazeIndicator != null)
        //{
        //    float normalizedX = pixelX / senderScreenWidth;
        //    float normalizedY = pixelY / senderScreenHeight; // 这里用原始Y值，因为小球的世界坐标可能不需要翻转
            
        //    float halfRangeX = moveRangeX / 2f;
        //    float halfRangeY = moveRangeY / 2f;

        //    float posX = Mathf.Clamp((normalizedX - 0.5f) * moveRangeX, -halfRangeX, halfRangeX);
        //    float posY = Mathf.Clamp((normalizedY - 0.5f) * moveRangeY, -halfRangeY, halfRangeY);
            
        //    gazeIndicator.position = new Vector3(posX, posY, 0f);
        //}
    }

    private void CloseConnection()
    {
        if (stream != null) { stream.Dispose(); stream = null; }
        if (client != null) { client.Close(); client = null; }
    }

    private void OnApplicationQuit()
    {
        CloseConnection();
        server?.Stop();
        Debug.Log("🔌 服务器已关闭");
    }
}