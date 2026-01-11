using UnityEditor;
using UnityEngine;

namespace Framework.Editor
{
    public class ResourceManagerMenu
    {
        private const string MENU_PATH = "Tools/Resource Mode/";
        private const string USE_ADDRESSABLES_KEY = "ResourceManager_UseAddressables";
        private const string MENU_ADDRESSABLES = "Tools/Resource Mode/Addressables";
        private const string MENU_ASSETDATABASE = "Tools/Resource Mode/AssetDatabase";

        [MenuItem(MENU_ADDRESSABLES, false, 1)]
        private static void SetUseAddressables()
        {
            PlayerPrefs.SetInt(USE_ADDRESSABLES_KEY, 1);
            PlayerPrefs.Save();
            Debug.Log("[ResourceManager] 已切换至 Addressables 模式");
            Menu.SetChecked(MENU_ADDRESSABLES, true);
            Menu.SetChecked(MENU_ASSETDATABASE, false);
        }

        [MenuItem(MENU_ADDRESSABLES, true)]
        private static bool ValidateUseAddressables()
        {
            Menu.SetChecked(MENU_ADDRESSABLES, PlayerPrefs.GetInt(USE_ADDRESSABLES_KEY, 1) == 1);
            return true;
        }

        [MenuItem(MENU_ASSETDATABASE, false, 2)]
        private static void SetUseAssetDatabase()
        {
            PlayerPrefs.SetInt(USE_ADDRESSABLES_KEY, 0);
            PlayerPrefs.Save();
            Debug.Log("[ResourceManager] 已切换至 AssetDatabase 模式（仅编辑器）");
            Menu.SetChecked(MENU_ADDRESSABLES, false);
            Menu.SetChecked(MENU_ASSETDATABASE, true);
        }

        [MenuItem(MENU_ASSETDATABASE, true)]
        private static bool ValidateUseAssetDatabase()
        {
            Menu.SetChecked(MENU_ASSETDATABASE, PlayerPrefs.GetInt(USE_ADDRESSABLES_KEY, 1) == 0);
            return true;
        }

        [MenuItem("Tools/Resource Mode/Show Current Mode", false, 10)]
        private static void ShowCurrentMode()
        {
            bool useAddressables = PlayerPrefs.GetInt(USE_ADDRESSABLES_KEY, 1) == 1;
            string mode = useAddressables ? "Addressables" : "AssetDatabase (Editor Only)";
            Debug.Log($"[ResourceManager] 当前资源加载模式: {mode}");
            EditorUtility.DisplayDialog("Resource Mode", $"当前资源加载模式:\n{mode}", "确定");
        }
    }
}
