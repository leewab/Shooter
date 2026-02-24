using UnityEngine;

/// <summary>
/// 通用单例基类（用于对象池的单例实现）
/// </summary>
/// <typeparam name="T">单例类型，必须继承MonoBehaviour</typeparam>
public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _isApplicationQuitting = false;

    /// <summary>
    /// 全局唯一实例
    /// </summary>
    public static T Instance
    {
        get
        {
            // 应用退出时不再创建实例
            if (_isApplicationQuitting)
            {
                Debug.LogWarning($"[{typeof(T).Name}] 应用正在退出，无法获取单例实例");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    // 查找场景中已存在的实例
                    _instance = FindObjectOfType<T>();

                    // 场景中无实例，创建新的游戏对象承载单例
                    if (_instance == null)
                    {
                        GameObject singletonObj = new GameObject($"[{typeof(T).Name}Singleton]");
                        _instance = singletonObj.AddComponent<T>();
                        DontDestroyOnLoad(singletonObj);
                    }
                }
                
                return _instance;
            }
        }
    }

    /// <summary>
    /// 防止被重复创建
    /// </summary>
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 标记应用退出状态
    /// </summary>
    protected virtual void OnApplicationQuit()
    {
        _isApplicationQuitting = true;
    }
}