#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class AddressableBuilder
{
    [MenuItem("Tools/Build Addressables - WeChat")]
    public static void BuildAddressablesForWeChat()
    {
        // 1. 获取设置
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Asset Settings not found");
            return;
        }

        // 2. 获取当前激活的构建器
        var builder = settings.ActivePlayerDataBuilder;
        if (builder == null)
        {
            Debug.LogError("No active data builder found");
            return;
        }

        // 3. 执行构建
        EditorUtility.DisplayProgressBar("Addressables", "Building...", 0.3f);
        try
        {
            // 修复：使用正确的构建方法
            var context = new AddressablesDataBuilderInput(settings);
            var result = builder.BuildData<AddressablesPlayerBuildResult>(context);
            
            if (!string.IsNullOrEmpty(result.Error))
            {
                Debug.LogError($"Build failed: {result.Error}");
                return;
            }

            Debug.Log("Addressables build completed successfully!");
            
            // 4. 复制到微信小游戏目录
            string sourcePath = Addressables.BuildPath;
            string targetPath = Path.GetFullPath(Path.Combine(
                Application.dataPath, 
                "../WeChatBuild/ServerData"));

            CopyAddressablesToWeChat(sourcePath, targetPath);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private static void CopyAddressablesToWeChat(string sourcePath, string targetPath)
    {
        try
        {
            // 确保目录存在
            if (!Directory.Exists(sourcePath))
            {
                Debug.LogError($"Source directory not found: {sourcePath}");
                return;
            }

            // 删除旧目录（如果存在）
            if (Directory.Exists(targetPath))
            {
                Debug.Log($"Cleaning existing directory: {targetPath}");
                Directory.Delete(targetPath, true);
                System.Threading.Thread.Sleep(500);
            }

            // 创建目标目录
            Directory.CreateDirectory(targetPath);

            // 复制文件
            Debug.Log($"Copying from {sourcePath} to {targetPath}");
            foreach (string file in Directory.GetFiles(sourcePath))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(targetPath, fileName);
                File.Copy(file, destFile, true);
            }

            // 复制子目录
            foreach (string subDir in Directory.GetDirectories(sourcePath))
            {
                string dirName = Path.GetFileName(subDir);
                string destDir = Path.Combine(targetPath, dirName);
                Directory.CreateDirectory(destDir);
                
                foreach (string file in Directory.GetFiles(subDir))
                {
                    string fileName = Path.GetFileName(file);
                    string destFile = Path.Combine(destDir, fileName);
                    File.Copy(file, destFile, true);
                }
            }

            Debug.Log($"Successfully copied to: {targetPath}");
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Copy failed: {e.GetType().Name} - {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
    }
}
#endif
