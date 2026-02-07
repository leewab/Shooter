using GameConfig;
using ResKit;
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
                    return new Color(0.9f, 0.2f, 0.2f, 1.0f);
                case ColorType.Green:
                    return new Color(0.2f, 0.8f, 0.2f, 1.0f);
                case ColorType.Blue:
                    return new Color(0.2f, 0.5f, 0.9f, 1.0f);
                case ColorType.Yellow:
                    return new Color(0.9f, 0.8f, 0.2f, 1.0f);
                case ColorType.Orange:
                    return new Color(0.5f, 0.2f, 0.016f, 1);
                case ColorType.Purple:
                    return new Color(0.5f, 0f, 0f, 1);
            }
            
            return Color.white;
        }

        
        public Sprite GetTurretSprite(string spriteName)
        {
            return ResourceManager.Instance.Load<Sprite>($"{PathDefine.PATH_RES_PRODUCT_DIR}/Game/Icon/Turrets/{spriteName}");
        }


        private static readonly string TurretPrefabPath = $"{PathDefine.PATH_RES_PRODUCT_DIR}/Game/Prefab/Turret.prefab";

        private GameObject _turretPrefab;
        public GameObject TurretPrefab
        {
            get
            {
                if (_turretPrefab == null)
                {
                    _turretPrefab = ResourceManager.Instance.Load<GameObject>(TurretPrefabPath);
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

        public bool RecycleTurret(BaseTurret turret)
        {
            return GameObjectPool<BaseTurret>.Instance.RecycleObject(TurretPrefab, turret);
        }
    }
}