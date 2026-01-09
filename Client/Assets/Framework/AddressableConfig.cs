#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public static class AddressableConfig
{
    [MenuItem("Tools/Setup WeChat Addressables")]
    public static void SetupWeChatAddressables()
    {
        // 获取或创建默认设置
        var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
        if (settings == null)
        {
            Debug.LogError("Failed to create Addressable Asset Settings");
            return;
        }

        // 创建微信专用分组（带必要Schema）
        var wechatGroup = settings.FindGroup("WeChat");
        if (wechatGroup == null)
        {
            wechatGroup = settings.CreateGroup("WeChat", false, false, true, null);
            var schema = wechatGroup.AddSchema<BundledAssetGroupSchema>();
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            schema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.OnlyHash;
            schema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;
        }

        // 1.19.19版本正确的远程路径设置方式
        var profileId = settings.activeProfileId;
        var remoteBuildPath = "[UnityEngine.AddressableAssets.Addressables.BuildPath]/ServerData/[BuildTarget]";
        var remoteLoadPath = "https://your-cdn-domain.com/[BuildTarget]/{0}";

        // 设置远程路径
        settings.profileSettings.SetValue(profileId, "Remote.BuildPath", remoteBuildPath);
        settings.profileSettings.SetValue(profileId, "Remote.LoadPath", remoteLoadPath);

        // 修复：1.19.19版本正确的构建脚本设置方式
        var buildScriptType = typeof(BuildScriptPackedMode);
        foreach (var builder in settings.DataBuilders)
        {
            if (builder.GetType() == buildScriptType)
            {
                settings.ActivePlayerDataBuilderIndex = 
                    settings.DataBuilders.IndexOf(builder);
                settings.ActivePlayModeDataBuilderIndex = 
                    settings.DataBuilders.IndexOf(builder);
                break;
            }
        }

        // 保存设置
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        
        Debug.Log("WeChat Addressables setup completed for version 1.19.19");
    }
}
#endif
