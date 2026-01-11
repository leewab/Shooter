using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Debug = UnityEngine.Debug;

public class ResourceManager : MonoBehaviour
{
    private const string USE_ADDRESSABLES_KEY = "ResourceManager_UseAddressables";

    private static ResourceManager _instance;

    public static ResourceManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("[ResourceManager]");
                _instance = go.AddComponent<ResourceManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private bool _useAddressables;
    private readonly Dictionary<string, UnityEngine.Object> _assetCache = new Dictionary<string, UnityEngine.Object>();

    public bool UseAddressables => _useAddressables;

    private void Awake()
    {
#if UNITY_EDITOR
        _useAddressables = PlayerPrefs.GetInt(USE_ADDRESSABLES_KEY, 1) == 1;
#else
        _useAddressables = true;
#endif
    }

    private void OnDestroy()
    {
        ReleaseAllAssets();
    }

    #region Async Load

    public void LoadAssetAsync<T>(string assetKey, Action<T> onComplete) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(assetKey))
        {
            onComplete?.Invoke(null);
            return;
        }

#if UNITY_EDITOR
        if (_useAddressables)
        {
            LoadAssetAsyncAddressable(assetKey, onComplete);
        }
        else
        {
            T asset = LoadAssetAtPath<T>(assetKey);
            onComplete?.Invoke(asset);
        }
#else
        LoadAssetAsyncAddressable(assetKey, onComplete);
#endif
    }

    private void LoadAssetAsyncAddressable<T>(string assetKey, Action<T> onComplete) where T : UnityEngine.Object
    {
        if (_assetCache.TryGetValue(assetKey, out var cachedAsset))
        {
            onComplete?.Invoke((T)cachedAsset);
            return;
        }

        var operation = Addressables.LoadAssetAsync<T>(assetKey);
        operation.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                _assetCache[assetKey] = op.Result;
                onComplete?.Invoke(op.Result);
            }
            else
            {
                Debug.LogError($"[Addressables] Failed to load: {assetKey}");
                onComplete?.Invoke(null);
            }
        };
    }

    #endregion

    #region Sync Load

    public T SyncLoad<T>(string assetKey, float timeout = 5f) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(assetKey)) return null;

#if UNITY_EDITOR
        if (_useAddressables)
        {
            return SyncLoadAddressable<T>(assetKey, timeout);
        }
        else
        {
            return LoadAssetAtPath<T>(assetKey);
        }
#else
        return SyncLoadAddressable<T>(assetKey, timeout);
#endif
    }

    private T SyncLoadAddressable<T>(string assetKey, float timeout) where T : UnityEngine.Object
    {
        if (_assetCache.TryGetValue(assetKey, out var cachedAsset))
        {
            return (T)cachedAsset;
        }

#if UNITY_EDITOR
        if (!UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.SettingsExists)
        {
            Debug.LogError("[Addressables] System not initialized");
            return null;
        }

        var (exists, _) = ValidateAssetKey<T>(assetKey);
        if (!exists)
        {
            Debug.LogError($"[Addressables] Key not found: {assetKey}");
            return null;
        }
#endif

        try
        {
            var loadTask = LoadAssetWithTimeout<T>(assetKey, timeout);
            loadTask.Wait();

            if (loadTask.Result.success)
            {
                _assetCache[assetKey] = loadTask.Result.asset;
                return loadTask.Result.asset;
            }
            else
            {
                Debug.LogError($"[Addressables] Load failed: {assetKey}, {loadTask.Result.errorType}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Addressables] Exception: {ex.Message}");
            return null;
        }
    }

#if UNITY_EDITOR
    private T LoadAssetAtPath<T>(string assetKey) where T : UnityEngine.Object
    {
        if (_assetCache.TryGetValue(assetKey, out var cachedAsset))
        {
            return (T)cachedAsset;
        }

        T asset = AssetDatabase.LoadAssetAtPath<T>(assetKey);
        if (asset != null)
        {
            _assetCache[assetKey] = asset;
        }
        else
        {
            Debug.LogError($"[AssetDatabase] Failed to load: {assetKey}");
        }
        return asset;
    }

    private static (bool exists, IResourceLocation location) ValidateAssetKey<T>(string assetKey) where T : UnityEngine.Object
    {
        foreach (var locator in Addressables.ResourceLocators)
        {
            if (locator.Locate(assetKey, typeof(T), out var locations) && locations != null && locations.Count > 0)
            {
                return (true, locations[0]);
            }
        }
        return (false, null);
    }
#endif

    private static async Task<(bool success, T asset, string errorType)> LoadAssetWithTimeout<T>(string assetKey, float timeout) where T : UnityEngine.Object
    {
        var stopwatch = Stopwatch.StartNew();
        var handle = Addressables.LoadAssetAsync<T>(assetKey);

        try
        {
            while (!handle.IsDone)
            {
                if (stopwatch.Elapsed.TotalSeconds > timeout)
                {
                    Addressables.Release(handle);
                    return (false, default, "Timeout");
                }
                await Task.Yield();
            }

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return (true, handle.Result, "");
            }
            else
            {
                Addressables.Release(handle);
                return (false, default, "LoadFailed");
            }
        }
        catch
        {
            Addressables.Release(handle);
            return (false, default, "Exception");
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    #endregion

    #region Release

    public void ReleaseAsset(string assetKey)
    {
        if (string.IsNullOrEmpty(assetKey)) return;

        if (_assetCache.TryGetValue(assetKey, out var asset))
        {
#if UNITY_EDITOR
            if (_useAddressables)
            {
                Addressables.Release(asset);
            }
#else
            Addressables.Release(asset);
#endif
            _assetCache.Remove(assetKey);
        }
    }

    public void ReleaseAllAssets()
    {
#if UNITY_EDITOR
        if (_useAddressables)
        {
            foreach (var asset in _assetCache.Values)
            {
                Addressables.Release(asset);
            }
        }
#else
        foreach (var asset in _assetCache.Values)
        {
            Addressables.Release(asset);
        }
#endif
        _assetCache.Clear();
    }

    #endregion

    #region Convenience Methods

    private readonly Dictionary<string, GameObject> _prefabCache = new Dictionary<string, GameObject>();

    public GameObject GetPrefab(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;

        if (_prefabCache.TryGetValue(path, out var prefab) && prefab != null)
        {
            return prefab;
        }

        prefab = SyncLoad<GameObject>(path);
        if (prefab != null)
        {
            _prefabCache[path] = prefab;
        }
        return prefab;
    }

    private readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();

    public Sprite GetSprite(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        if (!name.StartsWith("Assets/"))
        {
            name = $"Assets/Res/UI/{name}.png";
        }

        if (_spriteCache.TryGetValue(name, out var sprite) && sprite != null)
        {
            return sprite;
        }

        sprite = SyncLoad<Sprite>(name);
        if (sprite != null)
        {
            _spriteCache[name] = sprite;
        }
        return sprite;
    }

    #endregion
}
