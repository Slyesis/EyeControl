using UnityEngine;
using System.Collections.Generic;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher instance;
    private Queue<System.Action> actions = new Queue<System.Action>();

    // 确保在主线程初始化单例
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 场景切换时保持对象不销毁
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static UnityMainThreadDispatcher Instance
    {
        get
        {
            if (instance == null)
            {
                // 如果没有实例，创建一个临时对象承载脚本（确保在主线程）
                GameObject go = new GameObject("UnityMainThreadDispatcher");
                instance = go.AddComponent<UnityMainThreadDispatcher>();
            }
            return instance;
        }
    }

    void Update()
    {
        lock (actions)
        {
            while (actions.Count > 0)
            {
                actions.Dequeue().Invoke();
            }
        }
    }

    public void Enqueue(System.Action action)
    {
        lock (actions)
        {
            actions.Enqueue(action);
        }
    }
}