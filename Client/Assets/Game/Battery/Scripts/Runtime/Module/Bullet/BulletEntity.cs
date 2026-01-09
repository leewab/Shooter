using UnityEngine;
using DG.Tweening;

namespace Gameplay
{
    public class BulletEntity : BaseBullet
    {
        private BulletConf _bulletConfig;
        private ColorType _bulletColor;

        public SpriteRenderer spriteRenderer;

        // 私有变量
        private Vector2 moveDirection; // 固定的移动方向
        private Vector2 startPosition; // 发射起始位置
        private Rigidbody2D rb;
        private LayerMask obstacleLayer;
        private LayerMask targetLayer;
        private bool isStart = false;
        private Tween scaleTween; // 缩放动画 Tween

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;

            // 设置层级
            obstacleLayer = LayerMask.GetMask("Obstacle", "Ground", "Default");
            targetLayer = LayerMask.GetMask("Game", "Default");

            // 设置标签
            gameObject.tag = "Bullet";
        }

        // 设置子弹
        public override void SetupBullet(int id, ColorType colorType, Vector2 direction)
        {
            _bulletColor = colorType;
            _bulletConfig = BulletManager.Instance.GetBulletConf(id);
            startPosition = transform.position; // 记录发射位置
            moveDirection = direction.normalized;

            // 应用初始速度
            if (rb != null)
            {
                rb.velocity = moveDirection * _bulletConfig.Speed;
            }

            UpdateVisuals();

            // 设置初始旋转
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

            // 发射缩放动画 - 使用 DOTween
            PlayLaunchScaleAnimation();

            isStart = true;
        }

        // 发射缩放动画 - 使用 DOTween
        private void PlayLaunchScaleAnimation()
        {
            // 杀死之前的 Tween（如果有）
            scaleTween?.Kill();

            // 设置初始缩放
            transform.localScale = Vector3.one * _bulletConfig.StartScale;

            // 使用 DOTween 播放缩放动画
            scaleTween = transform
                .DOScale(Vector3.one, _bulletConfig.ScaleDuration)
                .SetEase(Ease.OutBack) // 使用 OutBack 缓动，有弹性效果
                .SetAutoKill(true);
        }

        private void Update()
        {
            if (!isStart) return;
            // 检查是否超出最大飞行距离
            float travelDistance = Vector2.Distance(startPosition, transform.position);
            if (travelDistance >= _bulletConfig.MaxTravelDistance)
            {
                Debug.Log("远距离销毁");
                DestroyBullet();
            }
        }

        private void FixedUpdate()
        {
            if (!isStart) return;
            // 保持直线运动
            if (rb != null)
            {
                // 确保子弹保持固定方向和速度
                if (rb.velocity.magnitude < _bulletConfig.Speed * 0.9f)
                {
                    rb.velocity = moveDirection * _bulletConfig.Speed;
                }
            }
        }

        // 更新视觉效果
        private void UpdateVisuals()
        {
            Color bulletColorVisual = TurretManager.Instance.GetColor(_bulletColor);
            if (spriteRenderer != null)
            {
                spriteRenderer.color = bulletColorVisual;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            HandleHit(other.gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleHit(collision.gameObject);
        }

        private void HandleHit(GameObject hitObject)
        {
            // 忽略子弹自身的碰撞
            if (hitObject.CompareTag("Bullet")) return;

            // 检查是否为障碍物
            if (((1 << hitObject.layer) & obstacleLayer) != 0)
            {
                Debug.Log("障碍物销毁! " + hitObject.gameObject.name);
                PlayHitEffects();
                DestroyBullet();
                return;
            }

            // 检查是否为龙节点
            DragonJoint joint = hitObject.GetComponent<DragonJoint>();
            if (joint != null && joint.IsAlive())
            {
                if (joint.GetColorType() == _bulletColor && !joint.IsHead() && !joint.IsTail())
                {
                    // 造成伤害
                    joint.TakeDamage(_bulletConfig.Damage);

                    // 播放命中效果
                    PlayHitEffects();

                    Debug.Log("击中销毁");
                    DestroyBullet();
                }
            }
            // 如果击中非目标物体，也销毁子弹
            else if (!hitObject.CompareTag("Turret")) // 忽略炮台自身的碰撞
            {
                Debug.Log("未击中销毁");
                DestroyBullet();
            }
        }

        // 播放命中效果
        private void PlayHitEffects()
        {
            // 1. 生成命中粒子特效
            if (!string.IsNullOrEmpty(_bulletConfig.HitEffectName))
            {
                GameObject hitEffect = EffectManager.Instance.InstantiateEffect(
                    _bulletConfig.HitEffectName,
                    transform.position,
                    Quaternion.identity);

                if (hitEffect != null)
                {
                    // 自动销毁特效
                    Destroy(hitEffect, _bulletConfig.HitEffectDuration);
                }
            }

            // 2. 播放命中音效
            if (!string.IsNullOrEmpty(_bulletConfig.HitSoundName))
            {
                AudioManager.Instance.PlaySound(_bulletConfig.HitSoundName, transform.position);
            }

            // 3. 屏幕震动
            if (_bulletConfig.ScreenShakeIntensity > 0f)
            {
                // 假设有一个 CameraShake 单例
                var cameraShake = Camera.main?.GetComponent<CameraShake>();
                if (cameraShake != null)
                {
                    cameraShake.Shake(_bulletConfig.ScreenShakeDuration, _bulletConfig.ScreenShakeIntensity);
                }
            }

            // 4. Hit Stop（命中顿帧）
            if (_bulletConfig.HitStopDuration > 0f)
            {
                DoHitStop(_bulletConfig.HitStopDuration);
            }
        }

        // 命中顿帧效果 - 使用 DOTween
        private void DoHitStop(float duration)
        {
            // 保存原始时间缩放
            float originalTimeScale = Time.timeScale;

            // 暂停时间
            Time.timeScale = 0f;

            // 使用 DOTween 的延迟回调（忽略时间缩放）
            DOVirtual.DelayedCall(duration, () =>
            {
                Time.timeScale = originalTimeScale;
            }, true); // true = 使用 unscaled time
        }

        // 销毁子弹
        private void DestroyBullet()
        {
            isStart = false;
            // 禁用碰撞和渲染
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null) collider.enabled = false;
            if (spriteRenderer != null) spriteRenderer.enabled = false;

            // 停止移动
            if (rb != null) rb.velocity = Vector2.zero;

            // // 等待音效播放完毕再销毁
            // if (audioSource != null && audioSource.isPlaying)
            // {
            //     Invoke("ActuallyDestroy", audioSource.clip.length);
            // }
            // else
            {
                Invoke("ActuallyDestroy", 0.1f);
            }
        }

        // 实际销毁对象
        private void ActuallyDestroy()
        {
            Destroy(gameObject);
        }

    }
}