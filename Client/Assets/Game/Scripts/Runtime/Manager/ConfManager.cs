using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ResKit;
using UnityEditor;
using UnityEngine;

namespace GameConfig
{
    public class ConfManager : Singleton<ConfManager>
    {
        private bool isOpenLocal = false;
        private Dictionary<string, Dictionary<int, BaseConf>> _confMaps = new Dictionary<string, Dictionary<int, BaseConf>>();
        
        private TextAsset LoadConf(string confName)
        {
            if (string.IsNullOrEmpty(confName)) return null;
            string fullName = confName;

            try
            {
                if (!confName.StartsWith("Assets/"))
                {
                    fullName = $"{PathDefine.PATH_RES_PRODUCT_DIR}/Conf/{confName}.json";
                }

                return ResourceManager.Instance.Load<TextAsset>(fullName);
            }
            catch (Exception e)
            {
                Debug.LogError($"{fullName}  配置表加载失败：" + e);
                throw;
            }
        }
        
        public T GetConfig<T>(string confName, int id) where T : BaseConf
        {
            if (_confMaps.TryGetValue(confName, out var confList))
            {
                if (confList.TryGetValue(id, out var conf))
                {
                    return conf as T;
                }
                else
                {
                    Debug.LogError($"配置表{confName}中未发现Id为 {id} 的值！");
                    return null;
                }
            }
            
            var confContent = LoadConf(confName);
            if (confContent == null || string.IsNullOrEmpty(confContent.text))
            {
                Debug.LogError($"{confName} 配置表加载为空");
                return null;
            }
            
            try
            {
                // 解析为List<T>
                var configList = JsonConvert.DeserializeObject<List<T>>(confContent.text);
                var configDic = configList.ToDictionary(item => item.Id, item => item as BaseConf);
                _confMaps.Add(confName, configDic);
                if (configDic.TryGetValue(id, out var conf))
                {
                    return conf as T;
                }
                
                Debug.LogError($"配置表{confName}中未发现Id为 {id} 的值！");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError("配置JSON格式化失败" + e.Message);
                throw;
            }
            
        }

        public Dictionary<int, BaseConf> GetAllConf(string confName)
        {
            return _confMaps.GetValueOrDefault(confName, new Dictionary<int, BaseConf>());
        }

        public void Dispose()
        {
            _confMaps.Clear();
        }

        

    }
}