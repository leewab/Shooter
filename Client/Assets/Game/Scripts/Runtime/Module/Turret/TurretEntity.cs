using System;
using UnityEngine;
using DG.Tweening;
using GameConfig;
using UnityEditor;
using UnityEngine.UI;

namespace Gameplay
{
    public class TurretEntity : BaseTurret
    {
        private ConfTurret _confTurret;
        
        [SerializeField] private Text txtBullet;
        [SerializeField] private Transform firePoint;
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        public Action OnDeadEvent;
        public Action<int> OnUpdateHitNum;

        private string _bulletName;
        private TurretPos _turretPos;
        private TurretInfo _turretInfo;
        private int _currentHitNum = 1;
        private float _attackTimer = 0f;
        private Tween recoilPositionTween; 
        private Tween recoilRotationTween; 
        
        public bool IsActive;
        public bool IsFirst;

        private void Update()
        {
            if (!IsActive) return;

            if (_attackTimer > 0)
            {
                _attackTimer -= Time.deltaTime;
                return;
            }

            PerformAttack();
        }

        private void InitEntity()
        {
            IsFirst = _turretPos.RowIndex == 0;
            _confTurret = ConfTurret.GetConf<ConfTurret>(_turretInfo.Id);
            _currentHitNum = _turretInfo.AttackNum;
            var confBullet = ConfBullet.GetConf<ConfBullet>(_confTurret.BulletId);
            _bulletName = confBullet.BulletName;
            txtBullet.text = _currentHitNum.ToString();
            OnUpdateHitNum?.Invoke(_currentHitNum);
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            if (spriteRenderer != null && _confTurret != null)
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
            DragonJoint targetJoint = DragonController.Instance.FindNearestUnblockedMatchingJoint(_confTurret.ColorType, firePoint.position);
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

            var bullet = BulletManager.Instance.InstantiateBullet(_bulletName, firePoint.position, Quaternion.identity) as BulletEntity;
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
            Vector3 recoilPosition = originalPosition - Vector3.up * _confTurret.RecoilDistance; //originalPosition - (Vector3)fireDirection * _confTurret.RecoilDistance;

            // 立即应用后坐力（瞬间）
            transform.position = recoilPosition;
            // transform.LookAt(originalPosition);

            // 使用 DOTween 平滑复位
            recoilPositionTween = transform
                .DOMove(originalPosition, _confTurret.RecoilDuration)
                .SetEase(Ease.OutQuad)
                .SetAutoKill(true);

            // recoilRotationTween = transform.DORotate(Vector3.zero, _confTurret.RecoilDuration)
            //     .SetEase(Ease.Linear)
            //     .SetAutoKill(true);
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
            IsFirst = false;
            IsActive = false;
            OnDeadEvent = null;
            OnUpdateHitNum = null;
            _confTurret = null;
            if (recoilPositionTween != null)
            {
                recoilPositionTween?.Kill();
                recoilPositionTween = null;
            }
            // if (recoilRotationTween != null)
            // {
            //     recoilRotationTween?.Kill();
            //     recoilRotationTween = null;
            // }
        }

        private void OnBulletHitCallback()
        {
            if (_currentHitNum <= 0 && IsActive)
            {
                IsActive = false;
                float deadDuration = 0.1f;
                PlayDeadAnimation(deadDuration);
                Invoke(nameof(Recycle), deadDuration);
            }
        }
        
        /// <summary>
        /// 架设炮台
        /// </summary>
        public override void SetupTurret(Transform parent)
        {
            this.gameObject.name = $"TurretAttack_{_confTurret.Id}";
            this.transform.SetParent(parent);
            this.transform.DOKill();
            this.transform.DOLocalMove(Vector3.zero, 0.1f).SetEase(Ease.Linear).OnComplete(() =>
            {
                this.IsActive = true;
                TurretHandler.Instance.EliminateTurret(_turretPos.RowIndex, _turretPos.ColIndex);
            });
        }

        /// <summary>
        /// 初始化炮台
        /// </summary>
        /// <param name="parameters"></param>
        public override void Init(params object[] parameters)
        {
            _turretInfo = (TurretInfo)parameters[0];
            _turretPos = (TurretPos)parameters[1];
            if (parameters.Length == 3)
            {
                var delay = (float)(parameters[2]);
                Invoke(nameof(InitEntity), delay);
            }
            else
            {
                InitEntity();
            }
        }

        /// <summary>
        /// 回收炮台
        /// </summary>
        public override void Recycle()
        {
            if (TurretManager.Instance.RecycleTurret(this))
            {
                IsActive = false;
                OnDeadEvent?.Invoke();
                Clear();
            }
            else
            {
                Debug.LogError("Turret 回收失败！");
            }
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