using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
    /// <summary>
    /// 使用组合方式实现 Toggle 图片切换，无需继承 Toggle 类，Inspector 可直接显示
    /// 将此脚本挂载到任意 Toggle 组件所在的 GameObject 上
    /// </summary>
    [RequireComponent(typeof(Toggle))]
    public class ToggleSwitcher : MonoBehaviour
    {
        [Tooltip("Toggle 开启时显示的图片对象")]
        public GameObject onImage;

        [Tooltip("Toggle 关闭时显示的图片对象")]
        public GameObject offImage;

        private Toggle _toggle;

        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
        }

        private void Start()
        {
            if (_toggle != null)
            {
                // 监听 Toggle 状态变化
                _toggle.onValueChanged.AddListener(OnToggleValueChanged);
                // 初始化显示状态
                OnToggleValueChanged(_toggle.isOn);
            }
        }

        private void OnToggleValueChanged(bool isOn)
        {
            if (onImage != null)
            {
                onImage.SetActive(isOn);
            }

            if (offImage != null)
            {
                offImage.SetActive(!isOn);
            }
        }

        private void OnDestroy()
        {
            if (_toggle != null)
            {
                _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            }
        }
    }
}
