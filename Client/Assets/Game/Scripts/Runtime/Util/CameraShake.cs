using UnityEngine;
using DG.Tweening;

namespace Gameplay
{
    /// <summary>
    /// 屏幕震动组件 - 使用 DOTween 实现
    /// 添加到主相机上使用
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        [Header("震动参数")]
        [Tooltip("震动强度")]
        [SerializeField] private float defaultShakeIntensity = 0.1f;

        [Tooltip("震动持续时间")]
        [SerializeField] private float defaultShakeDuration = 0.1f;

        [Tooltip("震动频率（每秒震动次数）")]
        [SerializeField] private int vibrato = 20;

        [Tooltip("震动随机性（0-90）")]
        [SerializeField] private float randomness = 45f;

        [Tooltip("是否使用降采样以获得更平滑的效果")]
        [SerializeField] private bool useUnscaledTime = false;

        [Tooltip("是否淡化震动强度")]
        [SerializeField] private bool fadeOut = true;

        private Tween currentShakeTween;

        /// <summary>
        /// 使用默认参数触发震动
        /// </summary>
        public void Shake()
        {
            Shake(defaultShakeDuration, defaultShakeIntensity);
        }

        /// <summary>
        /// 触发震动
        /// </summary>
        /// <param name="duration">震动持续时间（秒）</param>
        /// <param name="intensity">震动强度</param>
        public void Shake(float duration, float intensity)
        {
            // 如果已有震动在执行，停止它
            StopShake();

            // 使用 DOTween 的 DOShakePosition 实现震动
            currentShakeTween = transform
                .DOShakePosition(duration, intensity, vibrato, randomness, false, fadeOut)
                .SetUpdate(useUnscaledTime)
                .SetAutoKill(true);
        }

        /// <summary>
        /// 带旋转的震动（更强烈的效果）
        /// </summary>
        public void ShakeWithRotation(float duration, float intensity, float rotationStrength = 5f)
        {
            StopShake();

            // 同时震动位置和旋转
            Sequence shakeSequence = DOTween.Sequence();

            // 位置震动
            shakeSequence.Join(transform
                .DOShakePosition(duration, intensity, vibrato, randomness, false, fadeOut));

            // 旋转震动
            shakeSequence.Join(transform
                .DOShakeRotation(duration, Vector3.one * rotationStrength, vibrato / 2, randomness, fadeOut));

            shakeSequence.SetUpdate(useUnscaledTime).SetAutoKill(true);
            currentShakeTween = shakeSequence;
        }

        /// <summary>
        /// 立即停止震动并复位
        /// </summary>
        public void StopShake()
        {
            if (currentShakeTween != null && currentShakeTween.IsActive())
            {
                currentShakeTween.Kill();
                currentShakeTween = null;
            }
            // 复位位置和旋转
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        private void OnDestroy()
        {
            StopShake();
        }
    }
}
