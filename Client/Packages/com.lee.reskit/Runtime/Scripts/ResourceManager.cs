using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ResKit
{
    /// <summary>
    /// 资源加载模式
    /// </summary>
    public enum LoadMode
    {
        // 编辑器模式下的加载方式
        Editor_AssetDatabase, // 编辑器下使用AssetDatabase加载
        Editor_LocalBundle,   // 编辑器下使用本地Bundle加载

        ResourceLoad,
        
        // 真机模式下的加载方式
        Bundle, // 真机下使用本地Bundle加载

        // 微信小游戏特殊模式
        WeChat
    }

    /// <summary>
    /// 资源加载器
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        private static ResourceManager _instance;

        public static ResourceManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    // 查找场景中已存在的实例
                    _instance = FindObjectOfType<ResourceManager>();

                    // 场景中无实例，创建新的游戏对象承载单例
                    if (_instance == null)
                    {
                        GameObject singletonObj = new GameObject($"[{typeof(ResourceManager).Name}Singleton]");
                        _instance = singletonObj.AddComponent<ResourceManager>();
                        DontDestroyOnLoad(singletonObj);
                    }
                }

                return _instance;
            }
        }

        #region 常量定义

        public const string USE_ADDRESSABLES_KEY = "ResourceManager_UseAddressables";
        public const string ASSET_MANIFEST_NAME = "AssetManifest.json";

        #endregion

        #region 成员变量

        public static LoadMode Mode = LoadMode.Editor_AssetDatabase;

        // AssetManifest
        private AssetManifest _manifest;

        // 资源映射字典
        private Dictionary<string, AssetEntity> _assetEntities = new(); // AssetPath -> AssetEntity
        private Dictionary<string, BundleEntity> _bundleEntities = new(); // BundleName -> BundleEntity
        private Dictionary<string, string> _assetToBundleMap = new(); // AssetPath -> BundleName
        private Dictionary<string, List<string>> _assetDependencies = new(); // AssetPath -> Dependencies

        #endregion

        #region 生命周期

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this as ResourceManager;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            Mode = GetLoadMode();

            // 初始化时解析AssetManifest，除了AssetDatabase模式外都需要解析
            if (Mode == LoadMode.Editor_LocalBundle ||
                Mode == LoadMode.Bundle ||
                Mode == LoadMode.WeChat)
            {
                LoadAssetManifest();
            }
        }

        #endregion

        #region AssetManifest加载与解析

        public AssetEntity GetAssetEntity(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError("AssetPath 为空");
                return null;
            }
            
            if (_assetEntities.TryGetValue(assetPath, out AssetEntity assetEntity))
            {
                return assetEntity;
            }
            
            var bundleName = IsEditorMode() ? null : GetBundleName(assetPath);
            var assetDeps = IsEditorMode() ? null : GetAssetDependencies(assetPath);
            return AllocateAssetEntity(assetPath, bundleName, assetDeps);
        }

        public AssetEntity AllocateAssetEntity(string assetPath, string bundleName, List<string> dependencies)
        {
            if (!_assetEntities.TryGetValue(assetPath, out AssetEntity assetEntity))
            {
                _assetEntities.Add(assetPath, assetEntity = new AssetEntity(assetPath, bundleName, dependencies));
            }
            
            return assetEntity;
        }

        public BundleEntity GetBundleEntity(string bundleName)
        {
            if (string.IsNullOrEmpty(bundleName))
            {
                Debug.LogError("BundleName 为空");
                return null;
            }
            
            // 构建Bundle实体
            if (_bundleEntities.TryGetValue(bundleName, out BundleEntity bundleEntity))
            {
                return bundleEntity;
            }

            return AllocateBundleEntity(bundleName);
        }

        public BundleEntity AllocateBundleEntity(string bundleName)
        {
            if (!_bundleEntities.TryGetValue(bundleName, out BundleEntity bundleEntity))
            {
                _bundleEntities.Add(bundleName, bundleEntity = new BundleEntity(bundleName));
            }

            return bundleEntity;
        }

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
                Debug.LogError($"AssetManifest.json not found at {manifestPath}");
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
                PathType pathType = IsEditorMode() ? PathType.AssetBundleOutput : GetPathType();
                string platformDir = PathManager.GetPlatformFolder();
                return PathManager.GetLocalBundleFilePath(pathType, platformDir,ASSET_MANIFEST_NAME);
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
            Debug.Log(
                $"AssetManifest loaded successfully from {path}, total assets: {_manifest.TotalAssets}, total bundles: {_manifest.TotalBundles}");
        }

        /// <summary>
        /// 建立资源映射表
        /// </summary>
        private void BuildAssetMaps()
        {
            if (_manifest == null || _manifest.Assets == null) return;

            // 清空旧的映射表
            _assetToBundleMap.Clear();
            _assetDependencies.Clear();
            _assetEntities.Clear();
            _bundleEntities.Clear();

            // 构建Asset实体和映射表
            foreach (var assetInfo in _manifest.Assets)
            {
                string assetPath = assetInfo.AssetPath;
                string bundleName = assetInfo.BundleName;
                List<string> dependencies = assetInfo.Dependencies != null ? new List<string>(assetInfo.Dependencies) : new List<string>();

                // 构建Bundle实体
                AllocateBundleEntity(bundleName);
                
                // 构建AssetEntity
                AllocateAssetEntity(assetPath, bundleName, dependencies);

                // 构建Asset到Bundle的映射
                _assetToBundleMap[assetPath] = bundleName;

                // 构建Asset依赖映射
                _assetDependencies[assetPath] = dependencies;
            }
        }

        #endregion

        #region Asset Bundle 管理

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
            // Debug.Log(path);
            if (!_assetEntities.TryGetValue(path, out AssetEntity assetEntity))
            {
                assetEntity = GetAssetEntity(path);
            }

            return assetEntity.Load<T>();
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径</param>
        /// <param name="callback">加载完成回调</param>
        /// <returns>协程</returns>
        public void LoadAsync<T>(string path, System.Action<T> callback) where T : UnityEngine.Object
        {
            if (!_assetEntities.TryGetValue(path, out AssetEntity assetEntity))
            {
                assetEntity = GetAssetEntity(path);
            }

            var iEnumerator = assetEntity.LoadAsync(obj =>
            {
                callback?.Invoke(obj as T);
            });
            StartCoroutine(iEnumerator);
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

            if (_assetEntities.TryGetValue(normalizedPath, out AssetEntity assetEntity))
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
        
        public PathType GetPathType()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    return IsBundleMode() ? PathType.AssetBundleOutput : PathType.Assets;
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.Android:
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WebGLPlayer:
                    return IsBundleMode() ? PathType.StreamingAssets : PathType.Assets;
            }

            return PathType.AssetBundleOutput;
        }
        
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
        /// 获取当前加载模式
        /// </summary>
        /// <returns>加载模式</returns>
        public static LoadMode GetLoadMode()
        {
            return LoadMode.ResourceLoad;
#if UNITY_EDITOR
            Mode = (LoadMode)PlayerPrefs.GetInt(USE_ADDRESSABLES_KEY, (int)LoadMode.Editor_AssetDatabase);
            return Mode;
#else
            return LoadMode.ResourceLoad;
#endif
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
}