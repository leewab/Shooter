using System;
using System.Collections.Generic;
using ResKit;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Framework.UIFramework
{
    /// <summary>
    /// UI框架核心管理器
    /// 基于Addressable的ResourceManager实现UI的加载、打开、关闭管理
    /// </summary>
    public class UIHandler : MonoBehaviour
    {
        private static UIHandler _instance;
        public static UIHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("[UIFramework]");
                    _instance = go.AddComponent<UIHandler>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("UI层级配置")]
        [SerializeField] private UIRoot uiRoot;

        // UI面板缓存（单例模式的面板缓存）
        private readonly Dictionary<string, UIPanel> _panelCache = new Dictionary<string, UIPanel>();

        // UI栈（用于管理面板的显示顺序）
        private readonly Stack<UIPanel> _panelStack = new Stack<UIPanel>();

        private bool IsInitialized { get; set; }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 初始化UI框架
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized) return;

            // 创建UI层级结构
            CreateUILayers();
            IsInitialized = true;

            Debug.Log("[UIFramework] Initialized");
        }

        /// <summary>
        /// 创建UI层级结构
        /// </summary>
        private void CreateUILayers()
        {
            GameObject layerRoot = GameObject.Find("UIRoot");
            if (layerRoot == null)
            {
                layerRoot = new GameObject("UIRoot");
                layerRoot.transform.SetParent(transform);
                layerRoot.transform.localPosition = Vector3.zero;
                layerRoot.transform.localScale = Vector3.one;

                RectTransform rectTransform = layerRoot.GetComponent<RectTransform>();
                if (rectTransform == null) rectTransform = layerRoot.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.sizeDelta = Vector2.zero;
            }

            uiRoot = layerRoot.GetComponent<UIRoot>();
            if (uiRoot == null) uiRoot = layerRoot.AddComponent<UIRoot>();

            // 创建各层级并赋值给uiRoot字段
            uiRoot.BackgroundLayer = GetOrCreateLayer("BackgroundLayer", 0).transform;
            uiRoot.NormalLayer = GetOrCreateLayer("NormalLayer", 100).transform;
            uiRoot.PopupLayer = GetOrCreateLayer("PopupLayer", 200).transform;
            uiRoot.ToastLayer = GetOrCreateLayer("ToastLayer", 300).transform;
            uiRoot.TopLayer = GetOrCreateLayer("TopLayer", 400).transform;

            GameObject GetOrCreateLayer(string name, int sortingOrder)
            {
                var layer = uiRoot.transform.Find(name);
                GameObject layerObj = layer == null ? new GameObject(name) : layer.gameObject;
                layerObj.transform.SetParent(layerRoot.transform);
                layerObj.transform.localPosition = Vector3.zero;
                layerObj.transform.localScale = Vector3.one;

                RectTransform layerRect = layerObj.GetComponent<RectTransform>();
                if (layerRect == null) layerRect = layerObj.AddComponent<RectTransform>();
                layerRect.anchorMin = Vector2.zero;
                layerRect.anchorMax = Vector2.one;
                layerRect.sizeDelta = Vector2.zero;

                Canvas layerCanvas = layerObj.GetComponent<Canvas>();
                if (layerCanvas == null) layerCanvas = layerObj.AddComponent<Canvas>();
                layerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                layerCanvas.sortingOrder = sortingOrder;
                
                return layerObj;
            }
        }

        /// <summary>
        /// 打开UI面板
        /// </summary>
        /// <param name="assetKey">Addressable资源Key</param>
        /// <param name="panelType">面板类型（用于确定层级）</param>
        /// <param name="args">传递给面板的参数</param>
        /// <param name="onComplete">完成回调</param>
        public void OpenPanel(string assetKey, PanelType panelType = PanelType.Normal, object args = null, Action<UIPanel> onComplete = null)
        {
            if (!IsInitialized)
            {
                Initialize();
            }

            // 处理路径格式
            string fullAssetKey = ProcessAssetKey(assetKey);

            // 如果是单例模式且已缓存，直接显示
            if (_panelCache.TryGetValue(fullAssetKey, out var cachedPanel))
            {
                OpenPanelInternal(cachedPanel, args, onComplete);
                return;
            }

            // 异步加载面板
            LoadPanelAsync(fullAssetKey, panelType, (panel) =>
            {
                if (panel != null)
                {
                    OpenPanelInternal(panel, args, onComplete);
                }
                else
                {
                    onComplete?.Invoke(null);
                    Debug.LogError($"[UIFramework] Failed to open panel: {fullAssetKey}");
                }
            });
        }

        /// <summary>
        /// 加载UI面板
        /// </summary>
        private void LoadPanelAsync(string assetKey, PanelType panelType, Action<UIPanel> onComplete)
        {
            // 使用ResourceManager加载
            ResourceManager.Instance.LoadAsync<GameObject>(assetKey, (prefab) =>
            {
                if (prefab == null)
                {
                    onComplete?.Invoke(null);
                    return;
                }

                // 实例化面板到指定层级
                Transform parentLayer = uiRoot.GetLayerByPanelType(panelType);
                GameObject panelObj = Instantiate(prefab, parentLayer);
                UIPanel panel = panelObj.GetComponent<UIPanel>();

                if (panel == null)
                {
                    panel = panelObj.AddComponent<UIPanel>();
                    Debug.LogWarning($"[UIFramework] Panel prefab missing UIPanel component: {assetKey}");
                }

                // 设置名字
                panelObj.name = prefab.name;

                // 如果是单例模式，加入缓存
                if (panel.ShowMode == PanelShowMode.Single)
                {
                    _panelCache[assetKey] = panel;
                }

                onComplete?.Invoke(panel);
            });
        }

        /// <summary>
        /// 内部打开面板逻辑
        /// </summary>
        private void OpenPanelInternal(UIPanel panel, object args, Action<UIPanel> onComplete)
        {
            // 暂停当前栈顶面板
            if (_panelStack.Count > 0)
            {
                UIPanel topPanel = _panelStack.Peek();
                if (topPanel != null && topPanel != panel)
                {
                    topPanel.Pause();
                }
            }

            // 打开新面板
            panel.Open(args);

            // 如果是单例模式，加入栈
            if (panel.ShowMode == PanelShowMode.Single)
            {
                if (_panelStack.Contains(panel))
                {
                    // 如果已在栈中，先移除再添加到顶部
                    var tempStack = new Stack<UIPanel>();
                    while (_panelStack.Count > 0)
                    {
                        var p = _panelStack.Pop();
                        if (p != panel)
                        {
                            tempStack.Push(p);
                        }
                    }
                    while (tempStack.Count > 0)
                    {
                        _panelStack.Push(tempStack.Pop());
                    }
                }
                _panelStack.Push(panel);
            }

            onComplete?.Invoke(panel);
        }

        /// <summary>
        /// 关闭UI面板
        /// </summary>
        /// <param name="assetKey">Addressable资源Key</param>
        public void ClosePanel(string assetKey)
        {
            string fullAssetKey = ProcessAssetKey(assetKey);

            if (_panelCache.TryGetValue(fullAssetKey, out var panel))
            {
                ClosePanelInternal(panel);
            }
            else
            {
                Debug.LogWarning($"[UIFramework] Panel not found: {fullAssetKey}");
            }
        }

        /// <summary>
        /// 关闭当前面板
        /// </summary>
        public void CloseCurrentPanel()
        {
            if (_panelStack.Count > 0)
            {
                UIPanel topPanel = _panelStack.Pop();
                ClosePanelInternal(topPanel);

                // 恢复上一个面板
                if (_panelStack.Count > 0)
                {
                    UIPanel prevPanel = _panelStack.Peek();
                    prevPanel?.Resume();
                }
            }
        }

        /// <summary>
        /// 内部关闭面板逻辑
        /// </summary>
        private void ClosePanelInternal(UIPanel panel)
        {
            if (panel == null) return;

            panel.Close();

            // 如果是多例模式，销毁实例
            if (panel.ShowMode == PanelShowMode.Multiple)
            {
                Destroy(panel.gameObject);
            }
        }

        /// <summary>
        /// 预加载UI面板
        /// </summary>
        public void PreloadPanel(string assetKey, PanelType panelType = PanelType.Normal, Action onComplete = null)
        {
            OpenPanel(assetKey, panelType, null, (panel) =>
            {
                panel?.Close();
                onComplete?.Invoke();
            });
        }

        /// <summary>
        /// 卸载并销毁面板（从缓存中移除）
        /// </summary>
        public void UnloadPanel(string assetKey)
        {
            string fullAssetKey = ProcessAssetKey(assetKey);

            if (_panelCache.TryGetValue(fullAssetKey, out var panel))
            {
                ClosePanelInternal(panel);
                _panelCache.Remove(fullAssetKey);

                if (panel != null)
                {
                    Destroy(panel.gameObject);
                }

                // 释放Addressable资源
                ResourceManager.Instance.Release(fullAssetKey);
            }
        }

        /// <summary>
        /// 获取已缓存的面板
        /// </summary>
        public UIPanel GetCachedPanel(string assetKey)
        {
            string fullAssetKey = ProcessAssetKey(assetKey);
            _panelCache.TryGetValue(fullAssetKey, out var panel);
            return panel;
        }

        /// <summary>
        /// 处理资源Key格式
        /// </summary>
        private string ProcessAssetKey(string assetKey)
        {
            if (string.IsNullOrEmpty(assetKey))
            {
                return assetKey;
            }

            // 如果不是完整路径，添加默认前缀
            if (!assetKey.StartsWith("Assets/"))
            {
                return $"{PathDefine.PATH_RES_PRODUCT_DIR}/UI/Prefab/{assetKey}.prefab";
            }

            return assetKey;
        }

        /// <summary>
        /// 清空所有面板（场景切换时调用）
        /// </summary>
        public void ClearAllPanels()
        {
            foreach (var kvp in _panelCache)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value.gameObject);
                }
            }

            _panelCache.Clear();
            _panelStack.Clear();
        }

        /// <summary>
        /// 获取UI层级根节点
        /// </summary>
        public UIRoot GetUILayer()
        {
            return uiRoot;
        }
    }
}
