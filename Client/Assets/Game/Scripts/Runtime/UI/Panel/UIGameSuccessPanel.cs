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
        [SerializeField] private Button btnNext;

        protected override void OnAwake()
        {
            // 绑定按钮事件
            btnFinish.SetOnClick(OnFinishButtonClick);
            btnNext.SetOnClick(OnNextButtonClick);
            panelName = "UIGameSuccessPanel";
        }

        protected override void OnInitialize()
        {
            // 初始化操作（只在第一次打开时调用一次）
            Debug.Log($"[ExamplePanel] Initialized: {panelName}");
        }

        protected override void OnOpen(object args)
        {

        }

        protected override void OnClose()
        {
            // 关闭时的逻辑
            Debug.Log($"[ExamplePanel] Closed");

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

        private void OnNextButtonClick()
        {
            Close();
            LevelManager.Instance.StartNextLevel();
        }
        
    }
}