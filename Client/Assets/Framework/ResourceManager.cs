using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Debug = UnityEngine.Debug;

public class ResourceManager : MonoBehaviour
{
    private static ResourceManager _instance;

    public static ResourceManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ResourceManager();
            }
            
            return _instance;
        }
    }

    private Dictionary<string, AsyncOperationHandle> _loadedAssets = new Dictionary<string, AsyncOperationHandle>();

    // 异步加载资源
    public void LoadAssetAsync<T>(string assetKey, System.Action<T> onComplete)
    {
        if (string.IsNullOrEmpty(assetKey)) return;
        
        if (_loadedAssets.TryGetValue(assetKey, out var handle))
        {
            onComplete?.Invoke((T)handle.Result);
            return;
        }

        var operation = Addressables.LoadAssetAsync<T>(assetKey);
        operation.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                _loadedAssets[assetKey] = op;
                onComplete?.Invoke(op.Result);
            }
            else
            {
                Debug.LogError($"Failed to load asset: {assetKey}");
            }
        };
    }

    // 释放资源
    public void ReleaseAsset(string assetKey)
    {
        if (_loadedAssets.TryGetValue(assetKey, out var handle))
        {
            Addressables.Release(handle);
            _loadedAssets.Remove(assetKey);
        }
    }

    // 释放所有资源
    public void ReleaseAllAssets()
    {
        foreach (var kvp in _loadedAssets)
        {
            Addressables.Release(kvp.Value);
        }
        _loadedAssets.Clear();
    }
    
    
 /// <summary>
    /// 终极同步加载方法（解决所有超时和资源问题）
    /// </summary>
    public static T SyncLoad<T>(string assetKey, float timeout = 5f) where T : UnityEngine.Object
    {
        // 1. 深度验证Addressables系统
        if (!AddressableAssetSettingsDefaultObject.SettingsExists)
        {
            Debug.LogError("[Addressables] 系统未初始化，请检查：\n" +
                "1. 是否安装了Addressables包\n" +
                "2. 是否初始化了AddressableAssetSettings");
            return null;
        }

        // 2. 增强版资源Key验证
        var (exists, location) = ValidateAssetKey<T>(assetKey);
        if (!exists)
        {
            Debug.LogError($"[Addressables] 资源Key验证失败: {assetKey}\n" +
                $"可能原因：\n" +
                $"1. Key拼写错误\n" +
                $"2. 资源未标记为Addressable\n" +
                $"3. 资源未包含在构建中");
            return null;
        }

        // 3. 创建带超时的加载任务
        var loadTask = LoadAssetWithTimeout<T>(assetKey, timeout);
        loadTask.Wait(); // 同步等待

        // 4. 处理结果
        if (loadTask.Result.success)
        {
            return loadTask.Result.asset;
        }
        else
        {
            Debug.LogError($"[Addressables] 加载失败: {assetKey}\n" +
                $"错误类型: {loadTask.Result.errorType}\n" +
                $"详细信息: {loadTask.Result.errorMessage}");
            return null;
        }
    }

    private static (bool exists, IResourceLocation location) ValidateAssetKey<T>(string assetKey) 
        where T : UnityEngine.Object
    {
        foreach (var locator in Addressables.ResourceLocators)
        {
            if (locator.Locate(assetKey, typeof(T), out var locations))
            {
                if (locations != null && locations.Count > 0)
                {
                    return (true, locations[0]);
                }
            }
        }
        return (false, null);
    }

    private static async Task<(bool success, T asset, string errorType, string errorMessage)> 
        LoadAssetWithTimeout<T>(string assetKey, float timeout) where T : UnityEngine.Object
    {
        var stopwatch = Stopwatch.StartNew();
        AsyncOperationHandle<T> handle = default;
        string errorType = "";
        string errorMessage = "";

        try
        {
            // 1. 开始加载
            handle = Addressables.LoadAssetAsync<T>(assetKey);

            // 2. 等待完成或超时
            while (!handle.IsDone)
            {
                if (stopwatch.Elapsed.TotalSeconds > timeout)
                {
                    errorType = "Timeout";
                    errorMessage = $"超过最大等待时间 {timeout}秒";
                    Addressables.Release(handle);
                    return (false, default, errorType, errorMessage);
                }
                await Task.Yield();
            }

            // 3. 处理结果
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return (true, handle.Result, "", "");
            }
            else
            {
                errorType = "LoadFailed";
                errorMessage = handle.OperationException?.ToString() ?? "未知错误";
                Addressables.Release(handle);
                return (false, default, errorType, errorMessage);
            }
        }
        catch (Exception ex)
        {
            errorType = "SystemException";
            errorMessage = ex.ToString();
            if (!handle.Equals(default))
                Addressables.Release(handle);
            return (false, default, errorType, errorMessage);
        }
        finally
        {
            stopwatch.Stop();
        }
    }
    
    private Dictionary<string, GameObject> _GameObjectMap = new Dictionary<string, GameObject>();
    public GameObject GetPrefab(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (_GameObjectMap.TryGetValue(path, out GameObject obj))
        {
            if (obj != null) return obj;
        }
        
        obj = SyncLoad<GameObject>(path);
        _GameObjectMap.Add(path, obj);
        return obj;
    }
    
    private Dictionary<string, Sprite> _SpriteMap = new Dictionary<string, Sprite>();
    public Sprite GetSprite(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (!name.StartsWith("Assets/"))
        {
            name = $"Assets/Res/UI/{name}.png";
        }
        
        if (_SpriteMap.TryGetValue(name, out Sprite obj))
        {
            if (obj != null) return obj;
        }
        
        obj = SyncLoad<Sprite>(name);
        _SpriteMap.Add(name, obj);
        return obj;
    }
    
}