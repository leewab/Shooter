using ResKit;
using UnityEngine;

namespace Gameplay
{
    public class BulletManager : Singleton<BulletManager>
    {
        #region Bullet

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

        public BaseBullet InstantiateBullet(string bulletName, Vector3 position, Quaternion rotation)
        {
            return GameObjectPool<BaseBullet>.Instance.GetObject(bulletName, BulletPool, position, rotation);
        }

        public void RecycleBullet(BaseBullet bullet)
        {
            GameObjectPool<BaseBullet>.Instance.RecycleObject(bullet);
        }

        #endregion
        
        public Sprite GetBulletSprite(string spriteName)
        {
            return ResourceManager.Instance.Load<Sprite>(spriteName);
        }
    }
}