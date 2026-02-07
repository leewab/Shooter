using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ResKit
{
    public static class AssetBundleUtility
    {
        private const string SETTINGS_PATH = "ProjectSettings/AssetBundleToolkit.json";

        public static void SaveSettings(BuildSettings settings)
        {
            try
            {
                File.WriteAllText(SETTINGS_PATH, JsonUtility.ToJson(settings, true));
            }
            catch (Exception e)
            {
                Debug.LogError($"保存失败: {e.Message}");
            }
        }

        public static BuildSettings LoadSettings()
        {
            if (File.Exists(SETTINGS_PATH))
            {
                try
                {
                    return JsonUtility.FromJson<BuildSettings>(File.ReadAllText(SETTINGS_PATH));
                }
                catch (Exception e)
                {
                    Debug.LogError($"加载失败: {e.Message}");
                }
            }

            return new BuildSettings();
        }

        public static AssetBundleBuild[] GenerateBuilds(BuildSettings settings)
        {
            var allBuilds = new List<AssetBundleBuild>();

            foreach (var group in settings.groups)
            {
                if (!group.enabled) continue;

                var groupFiles = GetGroupFiles(group);
                if (groupFiles.Count == 0) continue;

                switch (group.packingMode)
                {
                    case BundlePackingMode.SingleBundle:
                        allBuilds.Add(new AssetBundleBuild
                        {
                            assetBundleName = GetBundleNameForFile(group, "", settings.addBundleSuffix).Replace("\\", "/"),
                            assetNames = groupFiles.ToArray()
                        });
                        break;

                    case BundlePackingMode.PerFolderBundle:
                        var folderGroups = GroupFilesByFolder(groupFiles);
                        foreach (var kvp in folderGroups)
                        {
                            allBuilds.Add(new AssetBundleBuild
                            {
                                assetBundleName = GetBundleNameForFile(group, kvp.Key, settings.addBundleSuffix),
                                assetNames = kvp.Value.ToArray()
                            });
                        }

                        break;

                    case BundlePackingMode.PerFileBundle:
                        foreach (var file in groupFiles)
                        {
                            allBuilds.Add(new AssetBundleBuild
                            {
                                assetBundleName = GetBundleNameForFile(group, file, settings.addBundleSuffix),
                                assetNames = new[] { file }
                            });
                        }

                        break;
                }
            }

            return allBuilds.ToArray();
        }
        
        public static AssetManifest GenerateManifest(AssetBundleBuild[] builds)
        {
            Dictionary<string, AssetInfo> assetInfosMap = new Dictionary<string, AssetInfo>();
            foreach (var assetBundleBuild in builds)
            {
                var assetBundleName = assetBundleBuild.assetBundleName;
                foreach (var assetPath in assetBundleBuild.assetNames)
                {
                    if (!assetInfosMap.TryGetValue(assetPath, out var info))
                    {
                        var dependencies = AssetDatabase.GetDependencies(assetPath, false);
                        var depsList = new  List<string>(dependencies.Length);
                        foreach (var dep in dependencies)
                        {
                            if (dep.EndsWith(".cs") || dep.EndsWith(".shader")) continue;
                            depsList.Add(dep);
                        }
                        assetInfosMap.Add(assetPath, new AssetInfo()
                        {
                            AssetPath = assetPath,
                            BundleName = assetBundleName,
                            Dependencies = depsList.ToArray()
                        });
                    }
                    else
                    {
                        Debug.LogError("存在重复AssetPath的资源 不允许！" + assetPath);
                    }
                }
            }

            AssetManifest manifest = new AssetManifest();
            manifest.BuildTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            manifest.TotalBundles = builds.Length;
            manifest.Assets = new List<AssetInfo>();
            foreach (var assetInfoKV in assetInfosMap)
            {
                manifest.Assets.Add(assetInfoKV.Value);
            }
            
            return manifest;
        }

        private static List<string> GetGroupFiles(AssetBundleGroup group)
        {
            var files = new List<string>();
            foreach (var path in group.assetPaths)
            {
                if (Directory.Exists(path))
                {
                    foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                    {
                        if (!file.EndsWith(".meta") && !file.EndsWith(".cs"))
                        {
                            files.Add(file.Replace("\\", "/"));
                        }
                    }
                }
                else if (File.Exists(path) && !path.EndsWith(".meta"))
                {
                    files.Add(path.Replace("\\", "/"));
                }
            }

            return files;
        }

        private static Dictionary<string, List<string>> GroupFilesByFolder(List<string> files)
        {
            var groups = new Dictionary<string, List<string>>();
            foreach (var file in files)
            {
                var folder = GetFolderFromPath(file);
                if (!groups.ContainsKey(folder))
                    groups[folder] = new List<string>();
                groups[folder].Add(file);
            }

            return groups;
        }

        private static string GetFolderFromPath(string path)
        {
            var relative = path.StartsWith("Assets/") ? path.Substring("Assets/".Length) : path;
            var folder = Path.GetDirectoryName(relative);
            return string.IsNullOrEmpty(folder) ? "" : folder.Replace("\\", "/");
        }

        private static string GetBundleNameForFile(AssetBundleGroup group, string filePath, bool addSuffix)
        {
            var relative = filePath.StartsWith("Assets/") ? filePath.Substring("Assets/".Length) : filePath;
            var fileName = Path.GetFileNameWithoutExtension(relative);
            var folder = Path.GetDirectoryName(relative);

            var fullName = $"{group.name}_{fileName}".ToLower();
            return addSuffix ? $"{fullName}.ab" : fullName;
        }

        public static void Build(string outputPath, AssetBundleBuild[] builds,
            BuildAssetBundleOptions options, BuildTarget target)
        {
            Directory.CreateDirectory(outputPath);
            BuildPipeline.BuildAssetBundles(outputPath, builds, options, target);
        }

        public static void CopyToStreamingAssets(string sourcePath, BuildTarget target)
        {
            var targetPath = Path.Combine(Application.streamingAssetsPath, "AssetBundles", target.ToString());

            if (Directory.Exists(targetPath))
                Directory.Delete(targetPath, true);

            CopyDirectory(sourcePath, targetPath);
            Debug.Log($"已拷贝到: {targetPath}");
        }

        private static void CopyDirectory(string source, string target)
        {
            Directory.CreateDirectory(target);
            foreach (var file in Directory.GetFiles(source))
            {
                File.Copy(file, Path.Combine(target, Path.GetFileName(file)), true);
            }

            foreach (var dir in Directory.GetDirectories(source))
            {
                CopyDirectory(dir, Path.Combine(target, Path.GetFileName(dir)));
            }
        }
    }
}