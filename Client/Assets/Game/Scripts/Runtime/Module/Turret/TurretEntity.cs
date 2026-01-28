using System;
using UnityEngine;
using DG.Tweening;
using GameConfig;
using UnityEngine.UI;

namespace Gameplay
{
    public class TurretEntity : BaseTurret
    {
        private TurretData _turretData;
        private ConfTurret _confTurret;
        
        [SerializeField] private Text txtBullet;
        [SerializeField] private Transform firePoint;
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        public Action<int> OnDeadEvent;
        public Action<int> OnUpdateHitNum;

        private int _delayActive = 30;
        private int _currentHitNum = 0;
        private float _attackTimer = 0f;
        private Tween recoilPositionTween; 
        
        public bool IsActive;
        public bool IsFirst;

        private void Update()
        {
            if (!IsActive) return;
            if (_delayActive > 0)
            {
                _delayActive--;
                return;
            }

            if (_attackTimer > 0)
            {
                _attackTimer -= Time.deltaTime;
                return;
            }

            PerformAttack();
        }

        private void InitializeComponents()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = TurretManager.Instance.GetTurretSprite(_confTurret.Icon);
            }
            if (firePoint == null)
            {
                firePoint = transform;
            }
        }

        private void PerformAttack()
        {
            DragonJoint targetJoint = DragonManager.Instance.FindNearestUnblockedMatchingJoint(_confTurret.ColorType, firePoint.position);
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
            bullet?.Init(_confTurret.Id, _confTurret.ColorType, direction, joint.transform.position);
            bullet?.AddRegister(OnBulletHitCallback);
            
            _currentHitNum--;
            txtBullet.text = _currentHitNum.ToString();
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

        private void PlayMuzzleFlash()
        {
            if (string.IsNullOrEmpty(_confTurret.MuzzleEffectName)) return;
            // 生成炮口特效
            var muzzleEffect = EffectManager.Instance.InstantiateEffect(
                _confTurret.MuzzleEffectName,
                firePoint.position,
                transform.rotation);
        }
        
        private void PlayDeadAnimation(float duration)
        {
            // 使用 DOTween 平滑复位
            recoilPositionTween = transform
                .DOScale(0, duration)
                .SetEase(Ease.OutQuad)
                .SetAutoKill(true);
        }

        private void Clear()
        {
            OnDeadEvent = null;
            OnUpdateHitNum = null;
            recoilPositionTween?.Kill();
            _turretData = null;
            _confTurret = null;
        }

        private void RecycleTurret()
        {
            IsActive = false;
            TurretManager.Instance.RecycleTurret(this);
            OnDeadEvent?.Invoke(_turretData.Index);
            Clear();
        }

        private void OnBulletHitCallback()
        {
            if (_currentHitNum <= 0)
            {
                IsActive = false;
                float deadDuration = 0.1f;
                PlayDeadAnimation(deadDuration);
                Invoke(nameof(RecycleTurret), deadDuration);
            }
        }
        
        
        /// <summary>
        /// 架设炮台
        /// </summary>
        public override void SetupTurret(Transform parent)
        {
            this.transform.SetParent(parent);
            this.transform.DOKill();
            this.transform.DOLocalMove(Vector3.zero, 0.1f).SetEase(Ease.Linear);
            this._delayActive = 120;
            this.IsActive = true;
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
            IsFirst = td.Row == 0;
            _confTurret = ConfTurret.GetConf<ConfTurret>(td.Id);
            _currentHitNum = _turretData.TurretInfo.CurrentHitNum;
            txtBullet.text = _currentHitNum.ToString();
            OnUpdateHitNum?.Invoke(_currentHitNum);
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