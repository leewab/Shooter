using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

namespace Gameplay
{
    public class TurretEntity : BaseTurret
    {
        private TurretData _turretData;
        private TurretConf _turretConf;

        public UnityEvent OnDeadEvent;
        // 开火点
        public Transform firePoint;
        // 射线线条渲染器
        public LineRenderer rayLine;
        // 目标层级
        public LayerMask targetLayer;

        // 组件
        public SpriteRenderer spriteRenderer;

        private bool isActive = false;
        private bool isFirst = false;

        // 目标管理
        private int currentHitNum = 0;
        private float attackTimer = 0f;
        private List<Vector2> rayPoints = new List<Vector2>();
        // 记录每条射线是否命中
        private List<bool> rayHits = new List<bool>();
        // 每条射线命中的关节
        private List<DragonJoint> hitJoints = new List<DragonJoint>();

        // DOTween Tween 引用
        private Tween recoilPositionTween;
        private Tween recoilRotationTween; 

        private void OnDestroy()
        {
            // 清理 DOTween
            recoilPositionTween?.Kill();
            recoilRotationTween?.Kill();

            _turretData = null;
            _turretConf = null;
            spriteRenderer = null;
            rayLine = null;
            if (rayPoints != null)
            {
                rayPoints.Clear();
                rayPoints = null;
            }
            if (rayHits != null)
            {
                rayHits.Clear();
                rayHits = null;
            }
            if (hitJoints != null)
            {
                hitJoints.Clear();
                hitJoints = null;
            }
        }

        private void Start()
        {
            // 设置线条渲染器
            SetupLineRenderer();
        }

        private void Update()
        {
            if (!isActive) return;
            
            // 更新攻击计时器
            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
            }

            // 炮台失效
            if (currentHitNum <= 0)
            {
                DestroyTurret();
            }

            // 执行射线检测
            PerformRaycastDetection();

            // 更新射线可视化
            UpdateRayVisualization();
        }

        private void InitializeTurretConf(int id)
        {
            _turretConf = TurretManager.Instance.GetTurretConf(id);
            currentHitNum = _turretConf.MaxHitNum;
        }

        private void InitializeComponents()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = TurretManager.Instance.GetColor(_turretConf.ColorType);
            }
            if (firePoint == null)
            {
                firePoint = transform;
            }
            if (rayLine == null)
            {
                rayLine = GetComponent<LineRenderer>();
                if (rayLine == null) rayLine = gameObject.AddComponent<LineRenderer>();
            }
        }

        // 初始化射线方向
        private void InitializeRayDirections()
        {
            rayPoints = ScreenTopDivider.GetCoverageAreaPositions(20,20, 80, 50);
        }

        // 初始化命中记录
        private void InitializeHitRecords()
        {
            if (rayHits == null) rayHits = new List<bool>();
            rayHits.Clear();
            for (int i = 0; i < rayPoints.Count; i++)
            {
                rayHits.Add(false);
            }
        }

        // 设置线条渲染器
        private void SetupLineRenderer()
        {
            if (rayLine == null) return;

            rayLine.startWidth = 0.1f;
            rayLine.endWidth = 0.05f;
            rayLine.material = new Material(Shader.Find("Sprites/Default"));
            rayLine.textureMode = LineTextureMode.Tile;
        }

        // 执行射线检测
        private void PerformRaycastDetection()
        {
            if (rayPoints == null || rayPoints.Count <= 0) return;
            if (hitJoints == null) hitJoints = new List<DragonJoint>();
            hitJoints.Clear();
            // 遍历所有射线方向
            for (int i = 0; i < rayPoints.Count; i++)
            {
                Vector2 rayStart = (Vector2)firePoint.position;
                Vector2 direction = (rayPoints[i] - rayStart).normalized;
                float distance = Vector2.Distance(rayStart, rayPoints[i]);

                // 发射射线检测
                RaycastHit2D[] hits = Physics2D.RaycastAll(rayStart, direction, distance, targetLayer);

                // 处理每个命中的目标
                foreach (var hit in hits)
                {
                    if (hit.collider != null)
                    {
                        // 检查是否为Dragon标签
                        if (hit.collider.CompareTag("DragonJoint"))
                        {
                            // 获取ColoredDragonJoint2D组件
                            DragonJoint joint = hit.collider.GetComponent<DragonJoint>();
                            if (joint != null && joint.IsAlive())
                            {
                                // 检查颜色类型是否匹配
                                if (!TurretManager.Instance.CheckColorType(joint.GetColorType(), _turretConf.ColorType)) continue;
                                hitJoints.Add(joint);
                                rayHits.Add(true);
                                break; // 每条射线只记录第一个命中的目标
                            }
                        }
                    }
                }
            }

            // 如果有射线命中且冷却结束，进行攻击
            if (attackTimer <= 0)
            {
                AttackHitJoints();
            }
        }

        // 攻击命中的关节
        private void AttackHitJoints()
        {
            if (currentHitNum <= 0) return;
            bool attacked = false;

            // 攻击所有命中射线的关节
            for (int i = 0; i < hitJoints.Count; i++)
            {
                if (hitJoints[i] != null)
                {
                    DragonJoint joint = hitJoints[i];
                    if (joint.IsAlive())
                    {
                        // 对关节发射子弹
                        AttackJoint(joint);
                        hitJoints.Remove(joint);
                        attacked = true;
                        currentHitNum--;
                        break;
                    }
                }
            }

            // 如果有任何攻击发生，重置攻击计时器
            if (attacked)
            {
                attackTimer = _turretConf.AttackCooldown;
                // // 播放音效
                // var fireSound = TurretManager.Instance.GetSound(_turretConf.FireSound);
                // if (audioSource != null && fireSound != null)
                // {
                //     audioSource.PlayOneShot(fireSound);
                // }
            }
        }

        // 攻击单个关节
        private void AttackJoint(DragonJoint joint)
        {
            if (joint == null) return;

            // 1. 播放后坐力动画（DOTween）
            PlayRecoilAnimation();

            // 2. 播放炮口闪光特效
            PlayMuzzleFlash();

            var bullet = BulletManager.Instance.InstantiateBullet(_turretConf.BulletName, firePoint.position, Quaternion.identity) as BulletEntity;
            // 计算子弹方向
            Vector2 direction = (joint.transform.position - firePoint.position).normalized;
            bullet?.SetupBullet(_turretConf.Id, _turretConf.ColorType, direction);
        }

        // 后坐力动画 - 使用 DOTween
        private void PlayRecoilAnimation()
        {
            if (_turretConf.RecoilDistance <= 0f && _turretConf.RecoilRotation <= 0f)
                return;

            // 计算后坐力目标状态
            Vector3 recoilPosition = transform.position - transform.up * _turretConf.RecoilDistance;
            float targetRotation = _turretConf.RecoilRotation;

            // 立即应用后坐力（瞬间）
            transform.position = recoilPosition;

            // 使用 DOTween 平滑复位
            // 位置复位
            recoilPositionTween = transform
                .DOMove(transform.position + transform.up * _turretConf.RecoilDistance, _turretConf.RecoilDuration)
                .SetEase(Ease.OutQuad)
                .SetAutoKill(true);

            // 旋转复位
            if (_turretConf.RecoilRotation > 0f)
            {
                recoilRotationTween = transform
                    .DORotate(Vector3.zero, _turretConf.RecoilDuration)
                    .SetEase(Ease.OutQuad)
                    .SetAutoKill(true);

                // 立即应用旋转
                transform.Rotate(0, 0, targetRotation);
            }
        }

        // 炮口闪光 - 直接调用，不需要协程
        private void PlayMuzzleFlash()
        {
            if (string.IsNullOrEmpty(_turretConf.MuzzleEffectName))
                return;

            // 生成炮口特效
            GameObject muzzleEffect = EffectManager.Instance.InstantiateEffect(
                _turretConf.MuzzleEffectName,
                firePoint.position,
                transform.rotation);

            if (muzzleEffect != null)
            {
                // 设置特效缩放
                muzzleEffect.transform.localScale = Vector3.one * _turretConf.MuzzleEffectScale;

                // 自动销毁
                Destroy(muzzleEffect, _turretConf.MuzzleFlashDuration);
            }
        }

        private void DestroyTurret()
        {
            isActive = false;
            _turretData = null;
            _turretConf = null;
            if (rayPoints != null)
            {
                rayPoints.Clear();
                rayPoints = null;
            }
            if (rayHits != null)
            {
                rayHits.Clear();
                rayHits = null;
            }
            if (hitJoints != null)
            {
                hitJoints.Clear();
                hitJoints = null;
            }

            rayLine = null;
            TurretManager.Instance.RecycleTurret(this);
            OnDeadEvent?.Invoke();
        }

        // 设置射线参数
        public override void SetTurret(TurretData td)
        {
            isFirst = td.PositionIndex == 0;
            _turretData = td;
            InitializeTurretConf(td.Id);
            InitializeComponents();
        }

        public void RemoveTurret()
        {
            TurretHandler.Instance.EliminateTurret(_turretData);
        }

        public void SetActive(bool active)
        {
            isActive = active;
            // 初始化射线方向
            InitializeRayDirections();
            // 初始化命中记录
            InitializeHitRecords();
        }

        public bool GetIsActive()
        {
            return isActive;
        }

        public bool GetIsFirst()
        {
            return isFirst;
        }
        
        
        // 更新射线可视化
        private void UpdateRayVisualization()
        {
            if (rayLine == null || rayPoints == null)
            {
                Debug.Log(isActive);
                return;
            }

            List<Vector3> linePositions = new List<Vector3>();
            List<Color> lineColors = new List<Color>();

            // 收集所有射线的位置和颜色
            for (int i = 0; i < rayPoints.Count; i++)
            {
                Vector2 rayStart = (Vector2)firePoint.position;
                Vector2 rayEnd = rayPoints[i];
                bool isHit = rayHits[i];
                lineColors.Add(isHit ? Color.green : Color.red);
                lineColors.Add(isHit ? Color.green : Color.red);
                linePositions.Add(rayStart);
                linePositions.Add(rayEnd);
            }

            // 设置线条渲染器的位置
            rayLine.positionCount = linePositions.Count;
            rayLine.SetPositions(linePositions.ToArray());

            // 设置每条射线的颜色
            for (int i = 0; i < lineColors.Count; i += 2)
            {
                rayLine.startColor = lineColors[i];
                rayLine.endColor = lineColors[i + 1];
            }
        }

    }
}