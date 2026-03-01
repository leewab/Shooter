using System;
using UnityEngine;
using DG.Tweening;
using GameConfig;
using UnityEngine.UI;
using Game.Core;

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

        private ColorType _currentColor;
        private TurretPos _turretPos;
        private TurretInfo _turretInfo;
        private int _currentHitNum = 1;
        private float _attackTimer = 0f;
        private Tween recoilPositionTween; 
        private Tween recoilRotationTween;
        private DragonJoint _targetJoint;
        
        public bool IsActive;
        public bool IsFirst;

        private void Update()
        {
            if (!IsActive) return;
            AutoAttack();
        }

        private void InitEntity()
        {
            IsFirst = _turretPos.RowIndex == 0;
            _confTurret = ConfTurret.GetConf<ConfTurret>(_turretInfo.Id);
            _currentHitNum = _turretInfo.AttackNum;
            _currentColor = _confTurret.ColorType;
            txtBullet.text = _currentHitNum.ToString();
            OnUpdateHitNum?.Invoke(_currentHitNum);
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            if (firePoint == null)
            {
                firePoint = transform;
            }
            if (spriteRenderer)
            {
                var turretSprite = TurretManager.Instance.GetSprite(_confTurret.Icon);
                spriteRenderer.sprite = turretSprite;
            }
        }

        private void AutoAttack()
        {
            // cd时间
            if (_attackTimer > 0)
            {
                _attackTimer -= Time.deltaTime;
                return;
            }
            
            // 获取目标节点
            if (_targetJoint == null || !_targetJoint.IsAlive() || !_targetJoint.IsActive())
            {
                _targetJoint = DragonController.Instance.FindMatchingJoint(_confTurret.ColorType);
                if (_targetJoint == null)
                {
                    Debug.Log("没有获取到可攻击的目标！检测是否有待攻击的");
                    // 没有可攻击的目标 查看所有龙骨 检测是否有待攻击的 没有该Turret作废
                    var waitJoint = DragonController.Instance.FindMatchingNoActiveJoint(_confTurret.ColorType);
                    if (waitJoint == null && _currentHitNum > 0)
                    {
                        _attackTimer = 99;
                        Debug.Log("没有可待攻击的龙骨！ 停止攻击！");
                    }
                    else
                    {
                        _attackTimer = _confTurret.AttackCooldown;
                    }
                    return;
                }
            }
            _attackTimer = _confTurret.AttackCooldown;
            AttackJoint();
        }

        private void AttackJoint()
        {
            if (_targetJoint == null) return;
            _targetJoint.SetLockByTurret(true);
            Vector2 direction = (_targetJoint.transform.position - firePoint.position).normalized;
            PlayRecoilAnimation(direction);
            PlayMuzzleFlash();
            PlayAttackAudio();

            var bullet = BulletManager.Instance.InstantiateBullet(PathDefine.BulletPath, firePoint.position, Quaternion.identity) as BulletEntity;
            bullet?.Init(_confTurret.Id, _confTurret.ColorType, _targetJoint);
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

        private void PlayAttackAudio()
        {
            if (string.IsNullOrEmpty(_confTurret.FireSound)) return;
            var audioSource = gameObject.GetOrAddComponent<AudioSource>();
            AudioManager.Instance.PlaySound(audioSource, _confTurret.FireSound);
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
            if (_currentHitNum <= 0)
            {
                // 当前Turret没有子弹了 Joint锁定取消
                IsActive = false;
                float deadDuration = 0.1f;
                PlayDeadAnimation(deadDuration);
                Invoke(nameof(Recycle), deadDuration);
                if (_targetJoint)
                {
                    _targetJoint.SetLockByTurret(false);
                    _targetJoint = null;
                }
            }
            else
            {
                // 当前Turret还有子弹 但是Joint没有激活或者没有存活 取消锁定
                if (_targetJoint && (!_targetJoint.IsActive() || !_targetJoint.IsAlive()))
                {
                    _targetJoint.SetLockByTurret(false);
                    _targetJoint = null;
                }
            }
        }

        public ColorType GetColorType()
        {
            return _currentColor;
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
            gameObject.name = $"Turret_{_turretPos.RowIndex}x{_turretPos.ColIndex}_{(ColorType)_turretInfo.Type}";
            // 延迟初始化是由于前排的炮台还没有移动完
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