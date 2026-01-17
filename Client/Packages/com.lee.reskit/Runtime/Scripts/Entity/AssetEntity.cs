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
    public class AssetEntity<T> where T : UnityEngine.Object
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
        public T Load()
        {
            switch (_loadMode)
            {
#if UNITY_EDITOR
                case LoadMode.Editor_AssetDatabase:
                    return LoadWithAsset();
                case LoadMode.Editor_LocalBundle:
#endif
                case LoadMode.Bundle:
                    return LoadWithBundle();
                case LoadMode.WeChat:
                    break;
            }

            return null;
        }

#if UNITY_EDITOR
        private T LoadWithAsset()
        {
            if (string.IsNullOrEmpty(AssetPath)) return null;
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(AssetPath);
        }
#endif

        private T LoadWithBundle()
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
        public IEnumerator LoadAsync(Action<T> callback)
        {
            switch (_loadMode)
            {
#if UNITY_EDITOR
                case LoadMode.Editor_AssetDatabase:
                    var asset = LoadWithAsset();
                    callback?.Invoke(asset);
                    yield break;
                case LoadMode.Editor_LocalBundle:
#endif
                case LoadMode.Bundle:
                case LoadMode.WeChat:
                    yield return LoadAsyncWithBundle(callback);
                    break;
            }

            yield return null;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="callback">加载完成回调</param>
        /// <returns>协程</returns>
        private IEnumerator LoadAsyncWithBundle(Action<T> callback)
        {
            if (IsLoaded)
            {
                AddRef();
                callback?.Invoke(Asset as T);
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

        private Dictionary<string, List<System.Action<T>>> _asyncLoadingPipeline = null;

        private void AddAsyncLoadPipeline(Action<T> callback)
        {
            if (_asyncLoadingPipeline == null) _asyncLoadingPipeline = new Dictionary<string, List<Action<T>>>();
            if (!_asyncLoadingPipeline.TryGetValue(AssetPath, out var callList))
            {
                _asyncLoadingPipeline.Add(AssetPath, new List<Action<T>>() { callback });
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
                    action?.Invoke(Asset as T);
                }
            }
        }

        #endregion
    }
}