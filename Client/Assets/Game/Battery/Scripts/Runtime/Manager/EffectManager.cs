using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gameplay
{
    public class EffectManager : Singleton<EffectManager>
    {
        #region Effect

        private const string EffectPrefabPath = "Assets/Res/Game/Effects/";

        private Transform _effectPool;
        public Transform EffectPool
        {
            get
            {
                if (_effectPool == null)
                {
                    _effectPool = new GameObject("EffectPool").transform;
                }
                return _effectPool;
            }
        }

        private Dictionary<string, GameObject> _effectPrefabs = new Dictionary<string, GameObject>();

        /// <summary>
        /// 实例化特效
        /// </summary>
        public GameObject InstantiateEffect(string effectName, Vector3 position, Quaternion rotation)
        {
            if (string.IsNullOrEmpty(effectName)) return null;

            GameObject effectPrefab = null;

            // 从缓存中获取
            if (_effectPrefabs.ContainsKey(effectName))
            {
                effectPrefab = _effectPrefabs[effectName];
            }
            else
            {
                // 加载预制体
                string fullPath = EffectPrefabPath + effectName + ".prefab";
#if UNITY_EDITOR
                effectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
#else
                effectPrefab = ResourceManager.Instance.GetPrefab(fullPath);
#endif

                if (effectPrefab != null)
                {
                    _effectPrefabs[effectName] = effectPrefab;
                }
                else
                {
                    Debug.LogWarning($"未找到特效预制体: {fullPath}");
                    return null;
                }
            }

            // 实例化特效
            return Object.Instantiate(effectPrefab, position, rotation, EffectPool);
        }

        #endregion
    }
}