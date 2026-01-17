using System.Collections.Generic;
using GameConfig;
using ResKit;
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

        public void RecycleTurret(BaseTurret turret)
        {
            if (turret == null) return;
            GameObjectPool<BaseTurret>.Instance.RecycleObject(TurretPrefab, turret);
        }
    }
}