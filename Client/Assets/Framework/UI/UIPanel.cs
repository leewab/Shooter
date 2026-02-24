using System;
using UnityEngine;

namespace Framework.UIFramework
{
    /// <summary>
    /// UI面板基类，所有UI面板都应继承此类
    /// </summary>
    public abstract class UIPanel : MonoBehaviour
    {
        protected string panelName;

        protected bool IsInitialized { get; private set; }
        protected bool IsOpen { get; private set; }
        
        public virtual PanelShowMode ShowMode => PanelShowMode.Single;

        protected virtual void Awake()
        {
            panelName = this.name;
            OnAwake();
        }

        protected virtual void Start()
        {
            OnStart();
        }

        protected virtual void OnDestroy()
        {
            OnDestroyed();
        }

        #region 生命周期回调

        protected virtual void OnAwake() { }
        protected virtual void OnStart() { }

        /// <summary>
        /// 初始化（只在第一次打开时调用）
        /// </summary>
        protected virtual void OnInitialize() { }

        /// <summary>
        /// 打开时调用
        /// </summary>
        protected virtual void OnOpen(object args = null) { }

        /// <summary>
        /// 关闭时调用
        /// </summary>
        protected virtual void OnClose() { }

        /// <summary>
        /// 暂停时调用（当面板被覆盖时）
        /// </summary>
        protected virtual void OnPause() { }

        /// <summary>
        /// 恢复时调用（当覆盖的面板关闭后）
        /// </summary>
        protected virtual void OnResume() { }

        /// <summary>
        /// 销毁时调用
        /// </summary>
        protected virtual void OnDestroyed() { }

        #endregion

        #region 公共方法

        /// <summary>
        /// 初始化面板
        /// </summary>
        public void Initialize()
        {
            if (!IsInitialized)
            {
                OnInitialize();
                IsInitialized = true;
            }
        }

        /// <summary>
        /// 打开面板
        /// </summary>
        public void Open(object args = null)
        {
            if (!IsInitialized)
            {
                Initialize();
            }

            gameObject.SetActive(true);
            IsOpen = true;
            OnOpen(args);
        }

        /// <summary>
        /// 关闭面板
        /// </summary>
        public void Close()
        {
            if (!IsOpen) return;

            IsOpen = false;
            OnClose();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 暂停面板
        /// </summary>
        public void Pause()
        {
            if (!IsOpen) return;
            OnPause();
        }

        /// <summary>
        /// 恢复面板
        /// </summary>
        public void Resume()
        {
            if (!IsOpen) return;
            OnResume();
        }

        #endregion

    }

    /// <summary>
    /// 面板类型
    /// </summary>
    public enum PanelType
    {
        /// <summary>普通面板</summary>
        Normal,
        /// <summary>固定面板（如主界面）</summary>
        Fixed,
        /// <summary>弹窗面板</summary>
        Popup,
        /// <summary>提示面板（如Toast）</summary>
        Toast
    }

    /// <summary>
    /// 面板显示模式
    /// </summary>
    public enum PanelShowMode
    {
        /// <summary>单例模式（只有一个实例）</summary>
        Single,
        /// <summary>多例模式（每次打开创建新实例）</summary>
        Multiple
    }
}
