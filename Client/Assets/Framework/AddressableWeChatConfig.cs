#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public static class AddressableWeChatConfig
{
    [MenuItem("Tools/Mark Resources for WeChat")]
    public static void MarkResourcesForWeChat()
    {
        // 1. 获取或创建Addressable设置
        var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
        if (settings == null)
        {
            Debug.LogError("Failed to get Addressable settings");
            return;
        }

        // 2. 创建WeChat分组（如果不存在）
        var wechatGroup = settings.FindGroup("WeChat");
        if (wechatGroup == null)
        {
            wechatGroup = settings.CreateGroup("WeChat", false, false, true, null);
            
            // 添加必要Schema
            var schema = wechatGroup.AddSchema<BundledAssetGroupSchema>();
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            schema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;
            
            Debug.Log("Created WeChat group with LZ4 compression");
        }

        // 3. 获取当前选中的资源
        var selectedObjects = Selection.objects;
        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            Debug.LogWarning("No assets selected in Project window");
            return;
        }

        // 4. 将选中资源标记到WeChat分组
        foreach (var obj in selectedObjects)
        {
            var assetPath = AssetDatabase.GetAssetPath(obj);
            var guid = AssetDatabase.AssetPathToGUID(assetPath);

            // 检查是否已标记
            var entry = settings.FindAssetEntry(guid);
            if (entry == null)
            {
                // 创建新的Addressable条目
                entry = settings.CreateOrMoveEntry(guid, wechatGroup);
                Debug.Log($"Marked {obj.name} as Addressable in WeChat group");
            }
            else if (entry.parentGroup != wechatGroup)
            {
                // 移动到WeChat分组
                settings.MoveEntry(entry, wechatGroup);
                Debug.Log($"Moved {obj.name} to WeChat group");
            }
        }

        AssetDatabase.SaveAssets();
    }
}
#endif
