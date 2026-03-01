using DG.Tweening;
using Framework.UIFramework;
using Gameplay;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace GameUI
{
    public class UIGameMainPanel : UIPanel
    {
        [Header("UI组件")]
        [SerializeField] private Button btnSpeed;
        [SerializeField] private Toggle _TogPlay;
        [SerializeField] private Text titleText;
        [SerializeField] private Text _TxtTimeScale;
        [SerializeField] private Text _TxtGameTime;
        [SerializeField] private Transform _BgGameTime;
        
        private float[] TimeArray = new[] { 1f, 2f, 3f, 4f };
        private int _CurTimeIndex = 0;
        
        protected override void OnAwake()
        {
            // 绑定按钮事件
            btnSpeed.SetOnClick(OnSpeedButtonClick);
            _TogPlay.SetOnToggle(OnToggleValueChanged);
            panelName = "UIGameMainPanel";
        }

        protected override void OnInitialize()
        {
            // 初始化操作（只在第一次打开时调用一次）
            Debug.Log($"[ExamplePanel] Initialized: {panelName}");
        }

        protected override void OnOpen(object args)
        {
            LevelManager.Instance.OnLevelChange += OnLevelChange;
            GameController.Instance.OnGameCountdown += OnGameCountdown;
        }

        protected override void OnClose()
        {
            LevelManager.Instance.OnLevelChange -= OnLevelChange;
            GameController.Instance.OnGameCountdown -= OnGameCountdown;
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
            
        }

        private void OnSpeedButtonClick()
        {
            ++_CurTimeIndex;
            if (_CurTimeIndex >= TimeArray.Length) _CurTimeIndex = 0;
            var timeScale = TimeArray[_CurTimeIndex];
            if (_TxtTimeScale != null) _TxtTimeScale.text = $"x{timeScale}";
            Time.timeScale = timeScale;
        }

        private void OnToggleValueChanged(bool isOn)
        {
            Time.timeScale = isOn ? TimeArray[_CurTimeIndex] : 0;
        }

        private void OnLevelChange(int level)
        {
            if (titleText)
            {
                titleText.text = $"第{level}关";
            }
        }

        private void OnGameCountdown(int countdown)
        {

            if (countdown > 0)
            {
                if (_TxtGameTime)
                {
                    _TxtGameTime.transform.localScale = new Vector3(1, 1, 1);
                    _TxtGameTime.text = countdown.ToString();
                    _TxtGameTime.transform.DOScale(0.9f, 0.5f).onComplete += () =>
                    {
                        _TxtGameTime.transform.DOScale(0, 0.3f);
                    };
                }

                if (_BgGameTime && !_BgGameTime.gameObject.activeSelf)
                {
                    _BgGameTime.gameObject.SetActive(true);
                }
            }
            else
            {
                if (_TxtGameTime)
                {
                    _TxtGameTime.transform.localScale = Vector3.zero;
                }
                if (_BgGameTime)
                {
                    _BgGameTime.gameObject.SetActive(false);
                }
            }
        }


    }
}