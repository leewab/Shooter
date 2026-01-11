using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

namespace Gameplay
{
    public class TurretEntity : BaseTurret
    {
        private TurretData _turretData;
        private ConfTurret _confTurret;
        
        [SerializeField] private Transform firePoint;
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        public UnityEvent<int> OnDeadEvent;
        public UnityEvent<int> OnUpdateHitNum;

        private int _delayActive = 30;
        private int _currentHitNum = 0;
        private float _attackTimer = 0f;

        private Tween recoilPositionTween; 
        
        private bool _isActive = false;
        public bool IsActive => _isActive;
        
        private bool _isFirst = false;
        public bool IsFirst => _isFirst;

        private void Update()
        {
            if (!_isActive) return;
            if (_delayActive > 0)
            {
                _delayActive--;
                return;
            }

            if (_currentHitNum <= 0)
            {
                _isActive = false;
                OnDeadEvent?.Invoke(_turretData.Index);
                Invoke(nameof(RecycleTurret), 1);
                return;
            }

            if (_attackTimer > 0)
            {
                _attackTimer -= Time.deltaTime;
                return;
            }

            PerformAttack();
        }

        private void InitializeTurretConf(int confId)
        {
            _confTurret = TurretManager.Instance.GetTurretConf(confId);
            _currentHitNum = _confTurret.MaxHitNum;
            OnUpdateHitNum?.Invoke(_currentHitNum);
        }

        private void InitializeComponents()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = TurretManager.Instance.GetColor(_confTurret.ColorType);
            }
            if (firePoint == null)
            {
                firePoint = transform;
            }
        }

        private void PerformAttack()
        {
            DragonJoint targetJoint = DragonManager.Instance.FindNearestMatchingJoint(_confTurret.ColorType, firePoint.position);
            if (targetJoint != null)
            {
                AttackJoint(targetJoint);
                _attackTimer = _confTurret.AttackCooldown;
            }
        }

        private void AttackJoint(DragonJoint joint)
        {
            if (joint == null) return;

            Vector2 direction = (joint.transform.position - firePoint.position).normalized;

            PlayRecoilAnimation(direction);
            PlayMuzzleFlash();

            var bullet = BulletManager.Instance.InstantiateBullet(_confTurret.BulletName, firePoint.position, Quaternion.identity) as BulletEntity;
            bullet?.Init(_confTurret.Id, _confTurret.ColorType, direction);

            _currentHitNum--;
            OnUpdateHitNum?.Invoke(_currentHitNum);
        }

        // 后坐力动画 - 使用 DOTween
        private void PlayRecoilAnimation(Vector2 fireDirection)
        {
            if (_confTurret.RecoilDistance <= 0f && _confTurret.RecoilRotation <= 0f)
                return;

            Vector3 originalPosition = transform.position;

            // 计算后坐力位置（沿子弹发射反方向）
            Vector3 recoilPosition = originalPosition - (Vector3)fireDirection * _confTurret.RecoilDistance;

            // 立即应用后坐力（瞬间）
            transform.position = recoilPosition;

            // 使用 DOTween 平滑复位
            recoilPositionTween = transform
                .DOMove(originalPosition, _confTurret.RecoilDuration)
                .SetEase(Ease.OutQuad)
                .SetAutoKill(true);
        }

        // 炮口闪光 - 直接调用，不需要协程
        private void PlayMuzzleFlash()
        {
            if (string.IsNullOrEmpty(_confTurret.MuzzleEffectName))
                return;

            // 生成炮口特效
            GameObject muzzleEffect = EffectManager.Instance.InstantiateEffect(
                _confTurret.MuzzleEffectName,
                firePoint.position,
                transform.rotation);

            if (muzzleEffect != null)
            {
                // 设置特效缩放
                muzzleEffect.transform.localScale = Vector3.one * _confTurret.MuzzleEffectScale;

                // 自动销毁
                Destroy(muzzleEffect, _confTurret.MuzzleFlashDuration);
            }
        }

        private void Clear()
        {
            OnUpdateHitNum = null;
            recoilPositionTween?.Kill();
            _turretData = null;
            _confTurret = null;
        }

        private void RecycleTurret()
        {
            _isActive = false;
            TurretManager.Instance.RecycleTurret(this);
            Clear();
        }
        
        
        /// <summary>
        /// 架设炮台
        /// </summary>
        public override void SetupTurret(Transform parent)
        {
            this.transform.SetParent(parent);
            this.transform.localPosition = Vector3.zero;
            this._delayActive = 120;
            this._isActive = true;
            TurretHandler.Instance.EliminateTurret(_turretData);
        }

        /// <summary>
        /// 初始化炮台
        /// </summary>
        /// <param name="parameters"></param>
        public override void Init(params object[] parameters)
        {
            TurretData td = (TurretData)parameters[0];
            if (td == null) return;
            _turretData = td;
            _isFirst = td.PositionIndex == 0;
            InitializeTurretConf(td.Id);
            InitializeComponents();
        }

        /// <summary>
        /// 回收炮台
        /// </summary>
        public override void Recycle()
        {
            RecycleTurret();
        }

        /// <summary>
        /// 销毁炮台
        /// </summary>
        public override void Destroy()
        {
            Clear();
        }
    }
}