using System.Collections.Generic;
using Gameplay;
using UnityEngine;

/// <summary>
/// 单例泛型对象池（仅支持继承MonoBehaviour的Unity对象）
/// </summary>
/// <typeparam name="T">待缓存的对象类型，必须继承MonoBehaviour</typeparam>
public class GameObjectPool<T> : Singleton<GameObjectPool<T>> where T : PoolMonoObject
{
    
    // 缓存池：键为预制体，值为该预制体对应的空闲对象队列
    private Dictionary<GameObject, Queue<T>> _poolDict = new Dictionary<GameObject, Queue<T>>();
    
    // 对象池根节点（用于整理Hierarchy面板，避免对象混乱）
    private Transform _poolRoot;

    /// <summary>
    /// 初始化对象池根节点
    /// </summary>
    protected override void OnInitialize()
    {
        CreatePoolRoot();
    }

    /// <summary>
    /// 创建对象池根节点（Hierarchy中显示）
    /// </summary>
    private void CreatePoolRoot()
    {
        var rootObj = new GameObject($"[{typeof(T).Name}ObjectPool]");
        _poolRoot = rootObj.transform;
    }

    /// <summary>
    /// 从对象池中获取对象
    /// </summary>
    /// <param name="prefab">对象预制体</param>
    /// <param name="position">对象生成位置</param>
    /// <param name="rotation">对象生成旋转</param>
    /// <returns>复用或新创建的对象</returns>
    public T GetObject(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
    {
        // 1. 验证预制体有效性
        if (prefab == null)
        {
            Debug.LogError($"[{typeof(T).Name}ObjectPool] 预制体不能为空！");
            return null;
        }

        // 2. 如果该预制体的缓存队列不存在，创建新队列
        if (!_poolDict.ContainsKey(prefab))
        {
            _poolDict.Add(prefab, new Queue<T>());
        }

        var objectQueue = _poolDict[prefab];
        T targetObj = null;

        // 3. 队列中有空闲对象，直接复用
        if (objectQueue.Count > 0)
        {
            targetObj = objectQueue.Dequeue();
            if (targetObj != null)
            {
                // Debug.Log("对象池去除 " + prefab.name + _poolDict[prefab].Count);
                var objTransform = targetObj.transform;
                objTransform.SetParent(parent);
                objTransform.position = position;
                objTransform.rotation = rotation;
                targetObj.gameObject.SetActive(true);
            }
        }
        // 4. 队列中无空闲对象，创建新对象
        else
        {
            GameObject newObj = Object.Instantiate(prefab, position, rotation, parent);
            targetObj = newObj.GetComponent<T>();
            if (targetObj == null)
            {
                targetObj = newObj.AddComponent<T>();
                Debug.LogWarning($"[{typeof(T).Name}ObjectPool] 预制体缺少{typeof(T).Name}组件，已自动添加");
            }
            newObj.name = $"{prefab.name}_Pooled";
        }

        return targetObj;
    }

    /// <summary>
    /// 将对象回收至对象池
    /// </summary>
    /// <param name="prefab">对象对应的原始预制体</param>
    /// <param name="obj">待回收的对象</param>
    public void RecycleObject(GameObject prefab, T obj)
    {
        // 1. 验证参数有效性
        if (prefab == null || obj == null)
        {
            Debug.LogError($"[{typeof(T).Name}ObjectPool] 回收参数不能为空！");
            return;
        }

        // 2. 如果该预制体的缓存队列不存在，创建新队列
        if (!_poolDict.ContainsKey(prefab))
        {
            _poolDict.Add(prefab, new Queue<T>());
        }

        // 3. 重置对象状态并回收
        var objTransform = obj.transform;
        objTransform.SetParent(_poolRoot); // 归位到对象池根节点
        objTransform.localScale = Vector3.one;
        objTransform.rotation = Quaternion.Euler(Vector3.zero);
        obj.gameObject.SetActive(false);   // 隐藏对象
        _poolDict[prefab].Enqueue(obj);    // 加入缓存队列
    }

    /// <summary>
    /// 清空指定预制体的缓存对象
    /// </summary>
    /// <param name="prefab">目标预制体</param>
    public void ClearPool(GameObject prefab)
    {
        if (_poolDict.ContainsKey(prefab))
        {
            var objectQueue = _poolDict[prefab];
            while (objectQueue.Count > 0)
            {
                var obj = objectQueue.Dequeue();
                if (obj != null)
                {
                    obj.Destroy();
                    Object.Destroy(obj.gameObject);
                }
            }
            _poolDict.Remove(prefab);
        }
    }

    /// <summary>
    /// 清空整个对象池
    /// </summary>
    public void ClearAllPool()
    {
        foreach (var keyValue in _poolDict)
        {
            var objectQueue = keyValue.Value;
            while (objectQueue.Count > 0)
            {
                var obj = objectQueue.Dequeue();
                if (obj != null)
                {
                    Object.Destroy(obj.gameObject);
                }
            }
        }
        _poolDict.Clear();
    }
}

