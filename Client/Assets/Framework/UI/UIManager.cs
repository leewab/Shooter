using System;
using System.Collections;
using System.Collections.Generic;
using GameUI;
using ResKit;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.UIFramework
{
    /// <summary>
    /// UIFramework扩展方法
    /// </summary>
    public static class UIManager
    {
        #region Open Panel
        
        /// <summary>
        /// 快捷打开面板（无参数）
        /// </summary>
        public static void Open<T>(object args = null) where T : UIPanel
        {
            var uiInfo = UIDefine.GetUIInfo(typeof(T).Name);
            UIHandler.Instance.OpenPanel(uiInfo.UIPath, uiInfo.PanelType, args);
        }

        /// <summary>
        /// 快捷打开面板（无参数）
        /// </summary>
        public static void Open(this string panelName)
        {
            var uiInfo = UIDefine.GetUIInfo(panelName);
            UIHandler.Instance.OpenPanel(uiInfo.UIPath, uiInfo.PanelType);
        }

        /// <summary>
        /// 快捷打开面板（带参数）
        /// </summary>
        public static void Open(this string panelName, object args)
        {
            var uiInfo = UIDefine.GetUIInfo(panelName);
            UIHandler.Instance.OpenPanel(uiInfo.UIPath, uiInfo.PanelType, args);
        }
        
        /// <summary>
        /// 快捷打开面板（带回调）
        /// </summary>
        public static void Open(this string panelName, Action<UIPanel> onComplete)
        {
            var uiInfo = UIDefine.GetUIInfo(panelName);
            UIHandler.Instance.OpenPanel(uiInfo.UIPath, uiInfo.PanelType, null, onComplete);
        }


        /// <summary>
        /// 快捷打开面板（带参数和回调）
        /// </summary>
        public static void Open(this string panelName, object args, Action<UIPanel> onComplete)
        {
            var uiInfo = UIDefine.GetUIInfo(panelName);
            UIHandler.Instance.OpenPanel(uiInfo.UIPath, uiInfo.PanelType, args, onComplete);
        }

        #endregion

        #region Close Panel
        /// <summary>
        /// 快捷关闭面板
        /// </summary>
        public static void Close<T>() where T : UIPanel
        {
            var uiInfo = UIDefine.GetUIInfo(typeof(T).Name);
            UIHandler.Instance.ClosePanel(uiInfo.UIPath);
        }
        
        /// <summary>
        /// 快捷关闭面板
        /// </summary>
        public static void Close(this string panelName)
        {
            var uiInfo = UIDefine.GetUIInfo(panelName);
            UIHandler.Instance.ClosePanel(uiInfo.UIPath);
        }

        /// <summary>
        /// 关闭当前面板
        /// </summary>
        public static void CloseCurrent()
        {
            UIHandler.Instance.CloseCurrentPanel();
        }

        #endregion

        #region Preload/Unload

        /// <summary>
        /// 快捷预加载面板
        /// </summary>
        public static void Preload(this string panelName, Action onComplete = null)
        {
            var uiInfo = UIDefine.GetUIInfo(panelName);
            UIHandler.Instance.PreloadPanel(uiInfo.UIPath, uiInfo.PanelType, onComplete);
        }

        /// <summary>
        /// 快捷卸载面板
        /// </summary>
        public static void Unload(this string panelName)
        {
            var uiInfo = UIDefine.GetUIInfo(panelName);
            UIHandler.Instance.UnloadPanel(uiInfo.UIPath);
        }

        #endregion

        #region Get Panel

        /// <summary>
        /// 获取已缓存的面板
        /// </summary>
        public static UIPanel GetPanel(this string panelName)
        {
            var uiInfo = UIDefine.GetUIInfo(panelName);
            return UIHandler.Instance.GetCachedPanel(uiInfo.UIPath);
        }

        #endregion


        #region GetSprite

        public static Sprite GetSprite(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            if (!name.StartsWith("Assets/"))
            {
                name = $"{PathDefine.PATH_RES_PRODUCT_DIR}/UI/{name}.png";
            }
            
            return ResourceManager.Instance.Load<Sprite>(name);
        }

        #endregion
    }

    /// <summary>
    /// UI组件扩展辅助类
    /// </summary>
    public static class UIComponentExtensions
    {
        /// <summary>
        /// 设置按钮点击事件
        /// </summary>
        public static void SetOnClick(this Button button, Action action)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action?.Invoke());
        }

        /// <summary>
        /// 安全设置文本
        /// </summary>
        public static void SetTextSafe(this Text text, string content)
        {
            if (text != null)
            {
                text.text = content;
            }
        }

        /// <summary>
        /// 安全设置图片
        /// </summary>
        public static void SetImageSafe(this Image image, Sprite sprite)
        {
            if (image != null)
            {
                image.sprite = sprite;
            }
        }

        /// <summary>
        /// 安全激活GameObject
        /// </summary>
        public static void SetActiveSafe(this GameObject obj, bool active)
        {
            if (obj != null)
            {
                obj.SetActive(active);
            }
        }
    }

    /// <summary>
    /// UI动画辅助类
    /// </summary>
    public static class UIAnimationHelper
    {
        /// <summary>
        /// 淡入动画
        /// </summary>
        public static IEnumerator FadeIn(CanvasGroup canvasGroup, float duration, Action onComplete = null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.gameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            onComplete?.Invoke();
        }

        /// <summary>
        /// 淡出动画
        /// </summary>
        public static IEnumerator FadeOut(CanvasGroup canvasGroup, float duration, Action onComplete = null)
        {
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(startAlpha * (1f - elapsed / duration));
                yield return null;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.gameObject.SetActive(false);
            onComplete?.Invoke();
        }

        /// <summary>
        /// 缩放动画
        /// </summary>
        public static IEnumerator ScaleAnimation(Transform transform, Vector3 from, Vector3 to, float duration, Action onComplete = null)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(from, to, t);
                yield return null;
            }

            transform.localScale = to;
            onComplete?.Invoke();
        }
    }
}
