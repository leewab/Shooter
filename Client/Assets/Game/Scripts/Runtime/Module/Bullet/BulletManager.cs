using System.Collections.Generic;
using GameConfig;
using ResKit;
using UnityEditor;
using UnityEngine;

namespace Gameplay
{
    public class BulletManager : Singleton<BulletManager>
    {
        #region Bullet

        private static readonly string BulletPrefabPath = $"{PathDefine.PATH_RES_PRODUCT_DIR}/Game/Prefab/Bullet.prefab";

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
                    _bulletPrefab = ResourceManager.Instance.Load<GameObject>(BulletPrefabPath);
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

        public void RecycleBullet(BaseBullet bullet)
        {
            GameObjectPool<BaseBullet>.Instance.RecycleObject(BulletPrefab, bullet);
        }

        #endregion
    }
}