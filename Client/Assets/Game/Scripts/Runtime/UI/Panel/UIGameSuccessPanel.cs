using Framework.UIFramework;
using Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace GameUI
{
    public class UIGameSuccessPanel : UIPanel
    {
        [Header("UI组件")]
        [SerializeField] private Button btnFinish;
        [SerializeField] private Button btnAgain;
        [SerializeField] private Text titleText;
        [SerializeField] private CanvasGroup canvasGroup;

        protected override void OnAwake()
        {
            // 绑定按钮事件
            btnFinish.SetOnClick(OnFinishButtonClick);
            btnAgain.SetOnClick(OnAgainButtonClick);
            panelName = "UIGameSuccessPanel";
        }

        protected override void OnInitialize()
        {
            // 初始化操作（只在第一次打开时调用一次）
            Debug.Log($"[ExamplePanel] Initialized: {panelName}");
        }

        protected override void OnOpen(object args)
        {
            // // 打开时的逻辑
            // Debug.Log($"[ExamplePanel] Opened with args: {args}");
            //
            // // 如果有传入参数，可以在这里处理
            // if (args is string message)
            // {
            //     titleText?.SetTextSafe(message);
            // }

            // 播放淡入动画
            if (canvasGroup != null)
            {
                StartCoroutine(UIAnimationHelper.FadeIn(canvasGroup, 0.3f));
            }
        }

        protected override void OnClose()
        {
            // 关闭时的逻辑
            Debug.Log($"[ExamplePanel] Closed");

            // 播放淡出动画
            if (canvasGroup != null)
            {
                StartCoroutine(UIAnimationHelper.FadeOut(canvasGroup, 0.2f, () =>
                {
                    base.OnClose();
                }));
            }
        }

        protected override void OnPause()
        {
            // 当被其他面板覆盖时调用
            Debug.Log($"[ExamplePanel] Paused");
        }

        protected override void OnResume()
        {
            // 当覆盖的面板关闭后恢复时调用
            Debug.Log($"[ExamplePanel] Resumed");
        }

        private void OnFinishButtonClick()
        {
            Close();
            LevelManager.Instance.StopGame();
        }

        private void OnAgainButtonClick()
        {
            Close();
            LevelManager.Instance.StartNextLevel();
        }
        
    }
}