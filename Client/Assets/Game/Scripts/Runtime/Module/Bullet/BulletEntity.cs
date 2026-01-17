using UnityEngine;
using DG.Tweening;
using GameConfig;

namespace Gameplay
{
    public class BulletEntity : BaseBullet
    {
        private ConfBullet _confBulletConfig;
        private ColorType _bulletColor;

        [SerializeField] private SpriteRenderer spriteRenderer;

        // 私有变量
        private Vector2 _moveDirection; // 固定的移动方向
        private Vector2 _startPosition; // 发射起始位置
        private Rigidbody rb;
        private LayerMask obstacleLayer;
        private LayerMask targetLayer;
        private bool _isStart = false;
        private Tween scaleTween; // 缩放动画 Tween

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

            // 设置层级
            obstacleLayer = LayerMask.GetMask("Obstacle", "Ground", "Default");

            // 设置标签
            gameObject.tag = "Bullet";
        }
        
        private void Update()
        {
            // if (!_isStart) return;
            // // 检查是否超出最大飞行距离
            // float travelDistance = Vector2.Distance(_startPosition, transform.position);
            // if (travelDistance >= _confBulletConfig.MaxTravelDistance)
            // {
            //     Debug.Log("远距离销毁");
            //     DestroyBullet();
            // }
        }

        private void FixedUpdate()
        {
            // if (!_isStart) return;
            // // 保持直线运动
            // if (rb != null)
            // {
            //     // 确保子弹保持固定方向和速度
            //     if (rb.velocity.magnitude < _confBulletConfig.Speed * 0.9f)
            //     {
            //         rb.velocity = _moveDirection * _confBulletConfig.Speed;
            //     }
            // }
        }

        // 发射缩放动画 - 使用 DOTween
        private void PlayLaunchScaleAnimation()
        {
            // 杀死之前的 Tween（如果有）
            scaleTween?.Kill();

            // 设置初始缩放
            transform.localScale = Vector3.one * _confBulletConfig.StartScale;

            // 使用 DOTween 播放缩放动画
            scaleTween = transform
                .DOScale(Vector3.one, _confBulletConfig.ScaleDuration)
                .SetEase(Ease.OutBack) // 使用 OutBack 缓动，有弹性效果
                .SetAutoKill(true);
        }
        
        // 更新视觉效果
        private void UpdateVisuals()
        {
            Color bulletColorVisual = TurretManager.Instance.GetColor(_bulletColor);
            if (spriteRenderer != null)
            {
                spriteRenderer.color = bulletColorVisual;
                spriteRenderer.enabled = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleHit(other.gameObject);
        }

        private void OnCollisionEnter(Collision collision)
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
                    joint.TakeDamage(_confBulletConfig.Damage);

                    // 播放命中效果
                    PlayHitEffects();

                    Debug.Log("击中销毁");
                    DestroyBullet();
                }
            }
            // 如果击中非目标物体，也销毁子弹  忽略炮台自身的碰撞
            else if (!hitObject.CompareTag("Turret")) 
            {
                Debug.Log("未击中销毁");
                DestroyBullet();
            }
        }

        // 播放命中效果
        private void PlayHitEffects()
        {
            // 1. 生成命中粒子特效
            if (!string.IsNullOrEmpty(_confBulletConfig.HitEffectName))
            {
                GameObject hitEffect = EffectManager.Instance.InstantiateEffect(
                    _confBulletConfig.HitEffectName,
                    transform.position,
                    Quaternion.identity);

                if (hitEffect != null)
                {
                    // 自动销毁特效
                    Destroy(hitEffect, _confBulletConfig.HitEffectDuration);
                }
            }

            // 2. 播放命中音效
            if (!string.IsNullOrEmpty(_confBulletConfig.HitSoundName))
            {
                AudioManager.Instance.PlaySound(_confBulletConfig.HitSoundName, transform.position);
            }

            // 3. 屏幕震动
            if (_confBulletConfig.ScreenShakeIntensity > 0f)
            {
                // 假设有一个 CameraShake 单例
                var cameraShake = Camera.main?.GetComponent<CameraShake>();
                if (cameraShake != null)
                {
                    cameraShake.Shake(_confBulletConfig.ScreenShakeDuration, _confBulletConfig.ScreenShakeIntensity);
                }
            }

            // 4. Hit Stop（命中顿帧）
            if (_confBulletConfig.HitStopDuration > 0f)
            {
                DoHitStop(_confBulletConfig.HitStopDuration);
            }
        }

        // 命中顿帧效果 - 使用 DOTween
        private void DoHitStop(float duration)
        {
            Time.timeScale = 0f;
            DOVirtual.DelayedCall(duration, () =>
            {
                Time.timeScale = 1f;
            }, true).SetUpdate(true);
        }

        // 销毁子弹
        private void DestroyBullet()
        {
            _isStart = false;
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
                Invoke(nameof(RecyleBullet), 0.1f);
            }
        }

        // 实际销毁对象
        private void RecyleBullet()
        {
            Clear();
            BulletManager.Instance.RecycleBullet(this);
        }

        private void Clear()
        {
            _confBulletConfig = null;
            if (rb != null) rb.velocity = Vector2.zero;  // 添加这行
        }
        
        
        public override void Init(params object[] parameters)
        {
            int id = (int)parameters[0];
            ColorType colorType = (ColorType)parameters[1];
            Vector2 direction =  (Vector2)parameters[2];
            Vector3 targetPost = (Vector3)parameters[3];
            _bulletColor = colorType;
            _confBulletConfig = BaseConf.GetConf<ConfBullet>(id);
            _startPosition = this.transform.position; 
            _moveDirection = direction.normalized;

            UpdateVisuals();
            
            // // 应用初始速度
            // if (rb != null)
            // {
            //     rb.velocity = _moveDirection * _confBulletConfig.Speed;
            // }

            // 设置初始旋转
            float angle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg;
            // transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
            transform.DORotate(new Vector3(0, 0, angle - 90f), 0.3f);
            transform.DOMove(targetPost, 0.3f).SetEase(Ease.Linear);

            // 发射缩放动画 - 使用 DOTween
            PlayLaunchScaleAnimation();

            _isStart = true;
        }

        public override void Recycle()
        {
            RecyleBullet();
        }

        public override void Destroy()
        {
            Clear();
        }
    }
}