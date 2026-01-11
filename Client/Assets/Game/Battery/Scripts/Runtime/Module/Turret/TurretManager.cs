using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gameplay
{
    public class TurretManager
    {
        private static TurretManager instance;
        public static TurretManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TurretManager();
                }
                return instance;
            }
        }

        public Color GetColor(ColorType color)
        {
            switch (color)
            {
                case ColorType.Red:
                    return Color.red;
                case ColorType.Green:
                    return Color.green;
                case ColorType.Blue:
                    return Color.blue;
                case ColorType.Yellow:
                    return Color.yellow;
                case ColorType.Orange:
                    return new Color(0.5f, 0.2f, 0.016f, 1);
                case ColorType.Purple:
                    return new Color(0.5f, 0f, 0f, 1);
            }
            
            return Color.white;
        }

        public int GetRandomTurretId()
        {
            return Random.Range(0, 4);
        }
        
        
        private const string TurretPrefabPath = "Assets/Res/Game/Turret.prefab";
        
        private GameObject _turretPrefab;
        public GameObject TurretPrefab
        {
            get
            {
                if (_turretPrefab == null)
                {
#if UNITY_EDITOR
                    _turretPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TurretPrefabPath);
#else
                    _turretPrefab = ResourceManager.Instance.GetPrefab(TurretPrefabPath);
#endif
                }

                if (_turretPrefab == null)
                {
                    Debug.LogError("加载预制体失败！" + TurretPrefabPath);
                }
                return _turretPrefab;
            }
        }

        public BaseTurret InstantiateTurret(Transform parent, Vector3 position, Quaternion rotation)
        {
            return GameObjectPool<BaseTurret>.Instance.GetObject(TurretPrefab, parent, position, rotation);
        }

        public void RecycleTurret(BaseTurret turret)
        {
            if (turret == null) return;
            GameObjectPool<BaseTurret>.Instance.RecycleObject(TurretPrefab, turret);
        }
        
        private static Dictionary<int, ConfTurret> _TurretConf = new Dictionary<int, ConfTurret>()
        {
            {
                0,
                new ConfTurret()
                {
                    Id = 0,
                    ColorType = ColorType.Red,
                    AttackCooldown = 1f,            // 攻击冷却时间
                    DamagePerShot = 1, // 每次攻击伤害
                    MaxHitNum = 3, // 最大攻击数量
                    FireSound = "Fire",
                    BulletName = "Bullet",
                    // 发射效果配置
                    RecoilDistance = 0.1f,          // 后坐力后退距离
                    RecoilDuration = 0.15f,         // 后坐力复位时间
                    RecoilRotation = 5f,            // 后坐力旋转角度（枪口上抬）
                    MuzzleFlashDuration = 0.05f,    // 炮口闪光持续时间
                    MuzzleEffectName = "MuzzleFlash", // 炮口特效名称
                    MuzzleEffectScale = 1f,         // 炮口特效缩放
                }
            },
            {
                1,
                new ConfTurret()
                {
                    Id = 1,
                    ColorType = ColorType.Green,
                    AttackCooldown = 1f, // 攻击冷却时间
                    DamagePerShot = 1, // 每次攻击伤害
                    MaxHitNum = 3, // 最大攻击数量
                    FireSound = "Fire",
                    BulletName = "Bullet",
                    // 发射效果配置
                    RecoilDistance = 0.1f,
                    RecoilDuration = 0.15f,
                    RecoilRotation = 5f,
                    MuzzleFlashDuration = 0.05f,
                    MuzzleEffectName = "MuzzleFlash",
                    MuzzleEffectScale = 1f,
                }
            },
            {
                2, new ConfTurret()
                {
                    Id = 2,
                    ColorType = ColorType.Blue,
                    AttackCooldown = 1f, // 攻击冷却时间
                    DamagePerShot = 1, // 每次攻击伤害
                    MaxHitNum = 3, // 最大攻击数量
                    FireSound = "Fire",
                    BulletName = "Bullet",
                    // 发射效果配置
                    RecoilDistance = 0.1f,
                    RecoilDuration = 0.15f,
                    RecoilRotation = 5f,
                    MuzzleFlashDuration = 0.05f,
                    MuzzleEffectName = "MuzzleFlash",
                    MuzzleEffectScale = 1f,
                }
            },
            {
                3, new ConfTurret()
                {
                    Id = 3,
                    ColorType = ColorType.Yellow,
                    AttackCooldown = 1f, // 攻击冷却时间
                    DamagePerShot = 1, // 每次攻击伤害
                    MaxHitNum = 3, // 最大攻击数量
                    FireSound = "Fire",
                    BulletName = "Bullet",
                    // 发射效果配置
                    RecoilDistance = 0.12f,        // 黄色炮台后坐力稍大
                    RecoilDuration = 0.15f,
                    RecoilRotation = 6f,
                    MuzzleFlashDuration = 0.05f,
                    MuzzleEffectName = "MuzzleFlash",
                    MuzzleEffectScale = 1.2f,      // 特效稍大
                }
            },
            {
                4, new ConfTurret()
                {
                    Id = 4,
                    ColorType = ColorType.Orange,
                    AttackCooldown = 1f, // 攻击冷却时间
                    DamagePerShot = 1, // 每次攻击伤害
                    MaxHitNum = 3, // 最大攻击数量
                    FireSound = "Fire",
                    BulletName = "Bullet",
                    // 发射效果配置
                    RecoilDistance = 0.1f,
                    RecoilDuration = 0.15f,
                    RecoilRotation = 5f,
                    MuzzleFlashDuration = 0.05f,
                    MuzzleEffectName = "MuzzleFlash",
                    MuzzleEffectScale = 1f,
                }
            }
        };
        
        public ConfTurret GetTurretConf(int id)
        {
            return _TurretConf.GetValueOrDefault(id);
        }
        
    }
}