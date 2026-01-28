using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ResKit
{
    /// <summary>
    /// Asset实体类，负责Asset资源的加载、引用计数和卸载
    /// </summary>
    public class AssetEntity
    {
        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath { get; private set; }

        /// <summary>
        /// 所属Bundle名称
        /// </summary>
        public string BundleName { get; private set; }

        /// <summary>
        /// 依赖资源路径列表
        /// </summary>
        public List<string> Dependencies { get; private set; }

        /// <summary>
        /// 资源对象
        /// </summary>
        public UnityEngine.Object Asset { get; private set; }

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount { get; private set; }

        /// <summary>
        /// 是否已加载
        /// </summary>
        public bool IsLoaded { get; private set; }

        // 当前加载模式
        private LoadMode _loadMode;
        private BundleEntity _bundleEntity;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="bundleName">所属Bundle名称</param>
        /// <param name="dependencies">依赖资源路径列表</param>
        public AssetEntity(string assetPath, string bundleName, List<string> dependencies)
        {
            AssetPath = assetPath;
            BundleName = bundleName;
            Dependencies = dependencies;
            RefCount = 0;
            IsLoaded = false;
            _loadMode = ResourceManager.Mode;
        }

        #region 同步加载

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <returns>加载的资源</returns>
        public T Load<T>() where T : UnityEngine.Object
        {
            switch (_loadMode)
            {
#if UNITY_EDITOR
                case LoadMode.Editor_AssetDatabase:
                    return LoadWithAsset<T>();
                case LoadMode.Editor_LocalBundle:
#endif
                case LoadMode.Bundle:
                    return LoadWithBundle<T>();
                case LoadMode.WeChat:
                case LoadMode.ResourceLoad:
                    return LoadWithResource<T>();
            }

            return Asset as T;
        }

#if UNITY_EDITOR
        private T LoadWithAsset<T>() where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(AssetPath)) return null;
            if (IsLoaded && Asset != null)
            {
                AddRef();
                return Asset as T;
            }
            Asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(AssetPath);
            IsLoaded = true;
            RefCount = 1;
            return (T)Asset;
        }
#endif
        
        private T LoadWithResource<T>() where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(AssetPath)) return null;
            if (IsLoaded && Asset != null)
            {
                AddRef();
                return Asset as T;
            }
            string assetPath = Path.ChangeExtension(AssetPath.Replace("Assets/Res/", ""), null);
            // Debug.Log("Resource 加载：" + assetPath);
            Asset = Resources.Load<T>(assetPath);
            IsLoaded = true;
            RefCount = 1;
            return (T)Asset;
        }

        private T LoadWithBundle<T>() where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(BundleName)) return null;
            
            if (IsLoaded)
            {
                AddRef();
                return Asset as T;
            }

            if (_bundleEntity == null)
            {
                // Debug.Log("加载：" + BundleName);
                _bundleEntity = ResourceManager.Instance.GetBundleEntity(BundleName);
            }

            if (_bundleEntity.Load())
            {
                // 尝试使用资源路径加载
                T asset = _bundleEntity.LoadAsset<T>(AssetPath);

                // 如果加载失败，尝试使用资源名称加载
                if (asset == null)
                {
                    string assetName = Path.GetFileNameWithoutExtension(AssetPath);
                    asset = _bundleEntity.LoadAsset<T>(assetName);
                }

                // 如果加载失败，尝试使用资源完整名称加载
                if (asset == null)
                {
                    string fullAssetName = Path.GetFileName(AssetPath);
                    asset = _bundleEntity.LoadAsset<T>(fullAssetName);
                }

                if (asset != null)
                {
                    Asset = asset;
                    IsLoaded = true;
                    RefCount = 1;
                }

                return asset;
            }

            return null;
        }

        #endregion

        #region 异步加载

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="callback">加载完成回调</param>
        /// <returns>协程</returns>
        public IEnumerator LoadAsync(Action<UnityEngine.Object> callback)
        {
            switch (_loadMode)
            {
#if UNITY_EDITOR
                case LoadMode.Editor_AssetDatabase:
                    var asset = LoadWithAsset<UnityEngine.Object>();
                    callback?.Invoke(asset);
                    yield break;
                case LoadMode.Editor_LocalBundle:
#endif
                case LoadMode.Bundle:
                case LoadMode.WeChat:
                    yield return LoadAsyncWithBundle(callback);
                    break;
                case LoadMode.ResourceLoad:
                    callback?.Invoke(LoadWithResource<UnityEngine.Object>());
                    yield break;
            }

            yield return null;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="callback">加载完成回调</param>
        /// <returns>协程</returns>
        private IEnumerator LoadAsyncWithBundle(Action<UnityEngine.Object> callback)
        {
            if (IsLoaded)
            {
                AddRef();
                callback?.Invoke(Asset);
                yield break;
            }

            if (_bundleEntity == null)
            {
                _bundleEntity = ResourceManager.Instance.GetBundleEntity(BundleName);
            }

            // 添加异步队列回调
            AddAsyncLoadPipeline(callback);

            // 执行异步加载
            yield return _bundleEntity.LoadAsync(success =>
            {
                if (success)
                {
                    LaunchAsyncLoadPipeline();
                }
                else
                {
                    ClearAsyncLoadPipeline();
                }
            });
        }

        #endregion

        /// <summary>
        /// 卸载资源
        /// </summary>
        public void Unload()
        {
            if (IsLoaded)
            {
                Resources.UnloadAsset(Asset);
                Asset = null;
                IsLoaded = false;

                // bundle引用计数刷新
                if (_bundleEntity.RemoveRef())
                {
                    _bundleEntity.Unload();
                }
            }

            // 清除异步回调管线
            ClearAsyncLoadPipeline();
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


        #region 异步加载回调管线

        private Dictionary<string, List<System.Action<UnityEngine.Object>>> _asyncLoadingPipeline = null;

        private void AddAsyncLoadPipeline(Action<UnityEngine.Object> callback)
        {
            if (_asyncLoadingPipeline == null) _asyncLoadingPipeline = new Dictionary<string, List<Action<UnityEngine.Object>>>();
            if (!_asyncLoadingPipeline.TryGetValue(AssetPath, out var callList))
            {
                _asyncLoadingPipeline.Add(AssetPath, new List<Action<UnityEngine.Object>>() { callback });
            }
            else
            {
                callList.Add(callback);
            }
        }

        private void ClearAsyncLoadPipeline()
        {
            if (_asyncLoadingPipeline != null)
            {
                _asyncLoadingPipeline.Clear();
                _asyncLoadingPipeline = null;
            }
        }

        private void LaunchAsyncLoadPipeline()
        {
            if (Asset == null)
            {
                Debug.LogError("Asset is null");
                return;
            }

            if (_asyncLoadingPipeline == null) return;
            if (_asyncLoadingPipeline.TryGetValue(AssetPath, out var actionList))
            {
                foreach (var action in actionList)
                {
                    action?.Invoke(Asset);
                }
            }
        }

        #endregion
    }
}