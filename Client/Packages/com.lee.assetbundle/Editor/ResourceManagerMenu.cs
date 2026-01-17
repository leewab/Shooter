using UnityEditor;
using UnityEngine;

namespace Framework.Editor
{
    public class ResourceManagerMenu
    {
        private const string MENU_PATH = "Tools/Resource Mode/";
        private const string MENU_ASSETBUNDLE = "Tools/Resource Mode/AssetBundle";
        private const string MENU_ASSETDATABASE = "Tools/Resource Mode/AssetDatabase";
        private const string MENU_WEIXINPATH = "Tools/Resource Mode/微信加载";

        [MenuItem(MENU_ASSETBUNDLE, false, 1)]
        private static void SetUseAssetBundle()
        {
            PlayerPrefs.SetInt(ResourceManager.USE_ADDRESSABLES_KEY, (int)LoadMode.Bundle);
            PlayerPrefs.Save();
            Debug.Log("[ResourceManager] 已切换至 AssetBundle 模式");
            Menu.SetChecked(MENU_ASSETBUNDLE, true);
            Menu.SetChecked(MENU_ASSETDATABASE, false);
            Menu.SetChecked(MENU_WEIXINPATH, false);
        }

        [MenuItem(MENU_ASSETBUNDLE, true)]
        private static bool ValidateUseAddressables()
        {
            Menu.SetChecked(MENU_ASSETBUNDLE, ResourceManager.GetLoadMode() == LoadMode.Bundle);
            return true;
        }

        [MenuItem(MENU_ASSETDATABASE, false, 2)]
        private static void SetUseAssetDatabase()
        {
            PlayerPrefs.SetInt(ResourceManager.USE_ADDRESSABLES_KEY, (int)LoadMode.Editor);
            PlayerPrefs.Save();
            Debug.Log("[ResourceManager] 已切换至 AssetDatabase 模式（仅编辑器）");
            Menu.SetChecked(MENU_ASSETBUNDLE, false);
            Menu.SetChecked(MENU_ASSETDATABASE, true);
            Menu.SetChecked(MENU_WEIXINPATH, false);
        }

        [MenuItem(MENU_ASSETDATABASE, true)]
        private static bool ValidateUseAssetDatabase()
        {
            Menu.SetChecked(MENU_ASSETDATABASE, ResourceManager.GetLoadMode() == LoadMode.Editor);
            return true;
        }
        
        [MenuItem(MENU_WEIXINPATH, false, 2)]
        private static void SetUseWeiXinPath()
        {
            PlayerPrefs.SetInt(ResourceManager.USE_ADDRESSABLES_KEY, (int)LoadMode.WeChat);
            PlayerPrefs.Save();
            Debug.Log("[ResourceManager] 已切换至 微信加载 模式（仅编辑器）");
            Menu.SetChecked(MENU_ASSETBUNDLE, false);
            Menu.SetChecked(MENU_ASSETDATABASE, false);
            Menu.SetChecked(MENU_WEIXINPATH, true);
        }

        [MenuItem(MENU_WEIXINPATH, true)]
        private static bool ValidateWeiXinPath()
        {
            Menu.SetChecked(MENU_WEIXINPATH, ResourceManager.GetLoadMode() == LoadMode.WeChat);
            return true;
        }

    }
}
