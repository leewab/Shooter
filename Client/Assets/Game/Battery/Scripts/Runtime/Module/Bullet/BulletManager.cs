using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gameplay
{
    public class BulletManager : Singleton<BulletManager>
    {
        #region Bullet
        
        private const string BulletPrefabPath = "Assets/Res/Battery/GamePlay/Bullet.prefab";
        
        private Transform _bulletPool;
        public Transform BulletPool
        {
            get
            {
                if (_bulletPool == null)
                {
                    _bulletPool = new GameObject("BulletPool").transform;
                }
                
                return _bulletPool;
            }
        }

        private GameObject _bulletPrefab;
        public GameObject BulletPrefab
        {
            get
            {
                if (_bulletPrefab == null)
                {
#if UNITY_EDITOR
                    _bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BulletPrefabPath);
#else
                    _bullet = ResourceManager.Instance.GetPrefab(BulletPrefabPath);
#endif
                }

                if (_bulletPrefab == null)
                {
                    Debug.LogError("加载预制体失败！" + BulletPrefabPath);
                }
                return _bulletPrefab;
            }
        }
        
        public BaseBullet InstantiateBullet(string bulletName, Vector3 position, Quaternion rotation)
        {
            return GameObjectPool<BaseBullet>.Instance.GetObject(BulletPrefab, BulletPool, position, rotation);
        }
        
        
        private static Dictionary<int, BulletConf> _BulletConf = new Dictionary<int, BulletConf>()
        {
            {
                0,
                new BulletConf()
                {
                    Damage = 1, // 伤害
                    Speed = 150f, // 速度
                    MaxTravelDistance = 200f, // 最大飞行距离
                    // 视觉效果配置
                    StartScale = 1.5f, // 发射时的初始缩放
                    ScaleDuration = 0.2f, // 缩放到正常大小的持续时间
                    // 命中效果配置
                    HitEffectName = "BulletHit", // 命中特效名称
                    HitEffectDuration = 0.5f, // 命中特效持续时间
                    HitSoundName = "BulletHit", // 命中音效名称
                    HitStopDuration = 0.02f, // 命中顿帧时长（秒）
                    ScreenShakeIntensity = 0.1f, // 屏幕震动强度
                    ScreenShakeDuration = 0.1f, // 屏幕震动持续时间
                    // 兼容旧配置
                    EffectName = "Bullet",
                    AudioName = "Bullet",
                }
            },
            {
                1,
                new BulletConf()
                {
                    Damage = 1, // 伤害
                    Speed = 150f, // 速度
                    MaxTravelDistance = 200f, // 最大飞行距离
                    // 视觉效果配置
                    StartScale = 1.5f,
                    ScaleDuration = 0.2f,
                    // 命中效果配置
                    HitEffectName = "BulletHit",
                    HitEffectDuration = 0.5f,
                    HitSoundName = "BulletHit",
                    HitStopDuration = 0.02f,
                    ScreenShakeIntensity = 0.1f,
                    ScreenShakeDuration = 0.1f,
                    // 兼容旧配置
                    EffectName = "Bullet",
                    AudioName = "Bullet",
                }
            },
            {
                2,
                new BulletConf()
                {
                    Damage = 1, // 伤害
                    Speed = 150f, // 速度
                    MaxTravelDistance = 200f, // 最大飞行距离
                    // 视觉效果配置
                    StartScale = 1.5f,
                    ScaleDuration = 0.2f,
                    // 命中效果配置
                    HitEffectName = "BulletHit",
                    HitEffectDuration = 0.5f,
                    HitSoundName = "BulletHit",
                    HitStopDuration = 0.02f,
                    ScreenShakeIntensity = 0.1f,
                    ScreenShakeDuration = 0.1f,
                    // 兼容旧配置
                    EffectName = "Bullet",
                    AudioName = "Bullet",
                }
            },
            {
                3,
                new BulletConf()
                {
                    Damage = 1, // 伤害
                    Speed = 150f, // 速度
                    MaxTravelDistance = 200f, // 最大飞行距离
                    // 视觉效果配置
                    StartScale = 1.8f,          // 黄色子弹发射缩放更大
                    ScaleDuration = 0.25f,
                    // 命中效果配置
                    HitEffectName = "BulletHit",
                    HitEffectDuration = 0.6f,
                    HitSoundName = "BulletHit",
                    HitStopDuration = 0.03f,    // 顿帧稍长
                    ScreenShakeIntensity = 0.15f, // 震动更强
                    ScreenShakeDuration = 0.15f,
                    // 兼容旧配置
                    EffectName = "Bullet",
                    AudioName = "Bullet",
                }
            },
            {
                4,
                new BulletConf()
                {
                    Damage = 1, // 伤害
                    Speed = 150f, // 速度
                    MaxTravelDistance = 200f, // 最大飞行距离
                    // 视觉效果配置
                    StartScale = 1.5f,
                    ScaleDuration = 0.2f,
                    // 命中效果配置
                    HitEffectName = "BulletHit",
                    HitEffectDuration = 0.5f,
                    HitSoundName = "BulletHit",
                    HitStopDuration = 0.02f,
                    ScreenShakeIntensity = 0.1f,
                    ScreenShakeDuration = 0.1f,
                    // 兼容旧配置
                    EffectName = "Bullet",
                    AudioName = "Bullet",
                }
            }
        };
        
        public BulletConf GetBulletConf(int id)
        {
            return _BulletConf.GetValueOrDefault(id);
        }

        #endregion
        
    }
}