using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ResKit
{
    /// <summary>
    /// Bundle实体类，负责AssetBundle对象的加载、引用计数和卸载
    /// </summary>
    public class BundleEntity
    {
        /// <summary>
        /// Bundle名称
        /// </summary>
        public string BundlePath { get; private set; }

        /// <summary>
        /// AssetBundle对象
        /// </summary>
        public AssetBundle Bundle { get; private set; }

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount { get; private set; }

        /// <summary>
        /// 是否已加载
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bundlePath">Bundle名称</param>
        /// <param name="assetPaths">包含的资源路径列表</param>
        public BundleEntity(string bundleName)
        {
            PathType pathType = ResourceManager.Instance.GetPathType();
            string platformDir = PathManager.GetPlatformFolder();
            BundlePath = PathManager.GetLocalBundleFilePath(pathType, platformDir, bundleName);
            RefCount = 0;
            IsLoaded = false;
        }

        /// <summary>
        /// 同步加载AssetBundle
        /// </summary>
        /// <param name="bundlePath">Bundle文件路径</param>
        /// <returns>是否加载成功</returns>
        public bool Load()
        {
            if (string.IsNullOrEmpty(BundlePath))
            {
                Debug.LogError("BundlePath is empty");
                return false;
            }
            
            if (IsLoaded)
            {
                AddRef();
                return true;
            }

            AssetBundle bundle = AssetBundle.LoadFromFile(BundlePath, 0, 0);
            if (bundle != null)
            {
                Bundle = bundle;
                IsLoaded = true;
                RefCount = 1;
                return true;
            }
            else
            {
                Debug.LogError("Load failed " + BundlePath);
            }

            return false;
        }

        /// <summary>
        /// 异步加载AssetBundle
        /// </summary>
        /// <returns>异步操作对象</returns>
        public IEnumerator LoadAsync(Action<bool> callback)
        {
            if (string.IsNullOrEmpty(BundlePath))
            {
                Debug.LogError("BundleName is empty");
                callback?.Invoke(false);
                yield break;
            }
            
            if (IsLoaded)
            {
                AddRef();
                callback?.Invoke(true);
                yield break;
            }

            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(BundlePath, 0, 0);
            yield return request;

            if (request.assetBundle != null)
            {
                Bundle = request.assetBundle;
                IsLoaded = true;
                RefCount = 1;
                callback?.Invoke(true);
            }
            else
            {
                Debug.LogError("Load failed " + BundlePath);
                callback?.Invoke(false);
            }
        }

        /// <summary>
        /// 增加引用计数
        /// </summary>
        public void AddRef()
        {
            RefCount++;
        }

        /// <summary>
        /// 减少引用计数
        /// </summary>
        /// <returns>引用计数是否为0</returns>
        public bool RemoveRef()
        {
            RefCount--;
            return RefCount <= 0;
        }

        /// <summary>
        /// 卸载AssetBundle
        /// </summary>
        public void Unload()
        {
            if (IsLoaded && Bundle != null)
            {
                Resources.UnloadAsset(Bundle);
                Bundle.Unload(false);
                Bundle = null;
                IsLoaded = false;
            }
        }

        #region Load Asset

        public T LoadAsset<T>(string assetPath) where T : Object
        {
            return Bundle?.LoadAsset<T>(assetPath);
        }

        #endregion
    }
}