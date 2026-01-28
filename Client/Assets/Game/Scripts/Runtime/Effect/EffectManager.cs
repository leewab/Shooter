using System.Collections.Generic;
using ResKit;
using UnityEngine;

namespace Gameplay
{
    public class EffectManager : SingletonMono<EffectManager>
    {
        #region Effect

        private static readonly string EffectPrefabPath = $"{PathDefine.PATH_RES_PRODUCT_DIR}/Effects/";

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
                string fullPath = $"{EffectPrefabPath}{effectName}/{effectName}.prefab";
                effectPrefab = ResourceManager.Instance.Load<GameObject>(fullPath);
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

            return Instantiate(effectPrefab, position, rotation, transform);
        }
 
        #endregion
    }
}