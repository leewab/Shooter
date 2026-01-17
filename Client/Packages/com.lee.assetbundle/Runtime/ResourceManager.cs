using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 资源加载模式
/// </summary>
public enum LoadMode
{
    // 编辑器模式下的加载方式
    Editor_AssetDatabase,  // 编辑器下使用AssetDatabase加载
    Editor_LocalBundle,    // 编辑器下使用本地Bundle加载
    
    // 真机模式下的加载方式
    Bundle,                // 真机下使用本地Bundle加载
    
    // 微信小游戏特殊模式
    WeChat
}

/// <summary>
/// 资源加载器
/// </summary>
public class ResourceManager : SingletonMono<ResourceManager>
{
    #region 常量定义
    public const string USE_ADDRESSABLES_KEY = "ResourceManager_UseAddressables";
    public const string ASSET_MANIFEST_NAME = "AssetManifest.json";
    #endregion

    #region 成员变量
    public static LoadMode Mode = LoadMode.Editor_AssetDatabase;
    
    // AssetManifest
    private AssetManifest _manifest;
    
    // 资源映射字典
    private Dictionary<string, AssetEntity<Object>> _assetEntities = new(); // AssetPath -> AssetEntity
    private Dictionary<string, BundleEntity> _bundleEntities = new(); // BundleName -> BundleEntity
    private Dictionary<string, string> _assetToBundleMap = new(); // AssetPath -> BundleName
    private Dictionary<string, List<string>> _assetDependencies = new(); // AssetPath -> Dependencies
    #endregion

    #region 生命周期
    protected override void Awake()
    {
        base.Awake();
        Mode = GetLoadMode();
        
        // 初始化时解析AssetManifest，除了AssetDatabase模式外都需要解析
        if (Mode != LoadMode.Editor_AssetDatabase)
        {
            LoadAssetManifest();
        }
    }
    #endregion
    
    #region AssetManifest加载与解析
    
    /// <summary>
    /// 加载并解析AssetManifest文件
    /// </summary>
    private void LoadAssetManifest()
    {
        // 获取ManifestPath
        string manifestPath = GetAssetManifestPath();
        if (string.IsNullOrEmpty(manifestPath)) return;
        
        // 尝试加载manifest文件
        if (File.Exists(manifestPath))
        {
            LoadAssetManifestFromPath(manifestPath);
        }
        else
        {
            // 尝试从备选路径加载
            string alternativePath = PathManager.Combine(PathType.GameData, ASSET_MANIFEST_NAME);
            if (File.Exists(alternativePath))
            {
                LoadAssetManifestFromPath(alternativePath);
            }
            else
            {
                Debug.LogError($"AssetManifest.json not found at {manifestPath} or {alternativePath}");
            }
        }
    }
    
    /// <summary>
    /// 获取AssetManifest文件路径
    /// </summary>
    /// <returns>AssetManifest文件路径</returns>
    private string GetAssetManifestPath()
    {
        if (IsAssetDatabaseMode())
        {
            // AssetDatabase模式不需要解析manifest
            return string.Empty;
        }
        else
        {
            // 本地Bundle模式从本地路径加载
            return IsEditorMode() 
                ? PathManager.Combine(PathType.AssetBundleOutput, ASSET_MANIFEST_NAME)
                : GetLocalBundleFilePath(ASSET_MANIFEST_NAME);
        }
    }
    
    /// <summary>
    /// 从指定路径加载AssetManifest
    /// </summary>
    /// <param name="path">manifest文件路径</param>
    private void LoadAssetManifestFromPath(string path)
    {
        string jsonContent = File.ReadAllText(path);
        _manifest = JsonUtility.FromJson<AssetManifest>(jsonContent);
        BuildAssetMaps();
        Debug.Log($"AssetManifest loaded successfully from {path}, total assets: {_manifest.TotalAssets}, total bundles: {_manifest.TotalBundles}");
    }
    
    /// <summary>
    /// 建立资源映射表
    /// </summary>
    private void BuildAssetMaps()
    {
        if (_manifest == null || _manifest.Assets == null || _manifest.Bundles == null)
            return;
        
        // 清空旧的映射表
        _assetToBundleMap.Clear();
        _assetDependencies.Clear();
        _assetEntities.Clear();
        _bundleEntities.Clear();
        
        // 构建Bundle实体
        foreach (var bundleInfo in _manifest.Bundles)
        {
            BundleEntity bundleEntity = new BundleEntity(bundleInfo.BundleName);
            _bundleEntities[bundleInfo.BundleName] = bundleEntity;
        }
        
        // 构建Asset实体和映射表
        foreach (var assetInfo in _manifest.Assets)
        {
            string assetPath = assetInfo.AssetPath;
            string bundleName = assetInfo.BundleName;
            List<string> dependencies = assetInfo.Dependencies != null ? new List<string>(assetInfo.Dependencies) : new List<string>();
            
            // 构建AssetEntity
            AssetEntity<UnityEngine.Object> assetEntity = new AssetEntity<UnityEngine.Object>(assetPath, bundleName, dependencies);
            _assetEntities[assetPath] = assetEntity;
            
            // 构建Asset到Bundle的映射
            _assetToBundleMap[assetPath] = bundleName;
            
            // 构建Asset依赖映射
            _assetDependencies[assetPath] = dependencies;
        }
    }
    
    #endregion

    #region Asset Bundle 管理

    public AssetEntity<Object> AllocateAssetEntity(string assetPath)
    {
        var bundleName = GetBundleName(assetPath);
        var assetDeps = GetAssetDependencies(assetPath);
        return new AssetEntity<Object>(assetPath, bundleName, assetDeps);
    }
    
    /// <summary>
    /// 获取资源对应的Bundle名称
    /// </summary>
    /// <param name="assetPath">资源路径</param>
    /// <returns>Bundle名称</returns>
    public string GetBundleName(string assetPath)
    {
        // 标准化资源路径
        string normalizedPath = NormalizePath(assetPath);
        
        // 从映射表中获取Bundle名称
        if (_assetToBundleMap.TryGetValue(normalizedPath, out string bundleName))
        {
            return bundleName;
        }
        
        // 如果找不到，尝试使用资源文件名
        string fileName = Path.GetFileNameWithoutExtension(normalizedPath);
        if (_assetToBundleMap.TryGetValue(fileName, out bundleName))
        {
            return bundleName;
        }
        return fileName.ToLower();
    }

    /// <summary>
    /// 获取Asset依赖资源
    /// </summary>
    /// <param name="assetPath"></param>
    /// <returns></returns>
    public List<string> GetAssetDependencies(string assetPath)
    {
        // 标准化资源路径
        string normalizedPath = NormalizePath(assetPath);
        // 从映射表中获取Bundle名称
        if (_assetDependencies.TryGetValue(normalizedPath, out List<string> assetDependencies))
        {
            return assetDependencies;
        }
        
        return null;
    }

    #endregion
    
    #region 资源加载
    
    /// <summary>
    /// 同步加载资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">资源路径</param>
    /// <returns>加载的资源</returns>
    public T Load<T>(string path) where T : UnityEngine.Object
    {
        if (!_assetEntities.TryGetValue(path, out AssetEntity<UnityEngine.Object> assetEntity))
        {
            assetEntity = AllocateAssetEntity(path);
        }

        return assetEntity.Load() as T;
    }
    
    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">资源路径</param>
    /// <param name="callback">加载完成回调</param>
    /// <returns>协程</returns>
    public IEnumerator LoadAsync<T>(string path, System.Action<T> callback) where T : UnityEngine.Object
    {
        if (!_assetEntities.TryGetValue(path, out AssetEntity<Object> assetEntity))
        {
            assetEntity = AllocateAssetEntity(path);
        }

        yield return assetEntity.LoadAsync(obj =>
        {
            callback?.Invoke(obj as T);
        });
    }
    
    #endregion
    
    #region 资源释放
    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="assetPath">资源路径</param>
    public void Release(string assetPath)
    {
        // 标准化资源路径
        string normalizedPath = NormalizePath(assetPath);
        
        if (_assetEntities.TryGetValue(normalizedPath, out AssetEntity<Object> assetEntity))
        {
            if (assetEntity.RemoveRef())
            {
                // 卸载资源
                assetEntity.Unload();
            }
        }
    }
  
    #endregion
    
    #region 辅助方法
    
    /// <summary>
    /// 标准化资源路径
    /// </summary>
    /// <param name="path">资源路径</param>
    /// <returns>标准化后的路径</returns>
    private string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }
    
    /// <summary>
    /// 获取本地Bundle文件的路径
    /// </summary>
    /// <param name="filePath">Bundle名称</param>
    /// <returns>Bundle路径</returns>
    private string GetLocalBundleFilePath(string filePath)
    {
        PathType basePathType = GetLocalPathType(Application.platform);
        string platformFolder = GetLocalDirectory(Application.platform);
        return PathManager.Combine(basePathType, "AssetBundles", platformFolder, filePath);
    }

    private PathType GetLocalPathType(RuntimePlatform platform)
    {
        switch (platform)
        {
            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.Android:
                return PathType.PersistentData;
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                return PathType.StreamingAssets;
            case RuntimePlatform.WebGLPlayer:
                break;
        }
        
        return PathType.AssetBundleOutput;
    }
    
    private string GetLocalDirectory(RuntimePlatform platform)
    {
        switch (platform)
        {
            case RuntimePlatform.IPhonePlayer:
                return "IOS";
            case RuntimePlatform.Android:
                return "Android";
            case RuntimePlatform.WindowsPlayer:
                return "Windows";
            case RuntimePlatform.WindowsEditor:
                return "Windows";
            case RuntimePlatform.WebGLPlayer:
                return "WeChat";
            default:
                return platform.ToString();
        }
    }
    
    /// <summary>
    /// 获取当前加载模式
    /// </summary>
    /// <returns>加载模式</returns>
    public static LoadMode GetLoadMode()
    {
        Mode = (LoadMode)PlayerPrefs.GetInt(USE_ADDRESSABLES_KEY, (int)LoadMode.Editor_AssetDatabase);
        return Mode;
    }
    
    /// <summary>
    /// 检查是否为编辑器模式
    /// </summary>
    /// <returns>是否为编辑器模式</returns>
    private bool IsEditorMode()
    {
        return Mode == LoadMode.Editor_AssetDatabase || 
               Mode == LoadMode.Editor_LocalBundle;
    }
    
    /// <summary>
    /// 检查是否为AssetDatabase模式
    /// </summary>
    /// <returns>是否为AssetDatabase模式</returns>
    private bool IsAssetDatabaseMode()
    {
        return Mode == LoadMode.Editor_AssetDatabase;
    }
    
    /// <summary>
    /// 检查是否为本地Bundle模式
    /// </summary>
    /// <returns>是否为本地Bundle模式</returns>
    private bool IsBundleMode()
    {
        return Mode == LoadMode.Editor_LocalBundle || 
               Mode == LoadMode.Bundle ||
               Mode == LoadMode.WeChat;
    }
    
    #endregion
}