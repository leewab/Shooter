using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ResKit
{
    /// <summary>
    /// 资源路径类型
    /// </summary>
    public enum PathType
    {
        /// <summary>
        /// StreamingAssets路径
        /// </summary>
        StreamingAssets,

        /// <summary>
        /// 持久化数据路径
        /// </summary>
        PersistentData,

        /// <summary>
        /// 临时缓存路径
        /// </summary>
        TemporaryCache,

        /// <summary>
        /// AssetBundle输出路径
        /// </summary>
        AssetBundleOutput,

        /// <summary>
        /// 游戏数据路径
        /// </summary>
        GameData
    }

    /// <summary>
    /// 路径管理类，用于处理各平台下的资源路径
    /// </summary>
    public static class PathManager
    {
        /// <summary>
        /// 各平台路径缓存
        /// </summary>
        private static readonly Dictionary<PathType, string> _pathCache = new Dictionary<PathType, string>();

        /// <summary>
        /// 获取指定类型的路径
        /// </summary>
        /// <param name="pathType">路径类型</param>
        /// <returns>完整路径字符串</returns>
        public static string GetPath(PathType pathType)
        {
            if (_pathCache.TryGetValue(pathType, out string cachedPath))
            {
                return cachedPath;
            }

            string path = string.Empty;

            switch (pathType)
            {
                case PathType.StreamingAssets:
                    path = GetStreamingAssetsPath();
                    break;
                case PathType.PersistentData:
                    path = GetPersistentDataPath();
                    break;
                case PathType.TemporaryCache:
                    path = GetTemporaryCachePath();
                    break;
                case PathType.AssetBundleOutput:
                    path = GetAssetBundleOutputPath();
                    break;
                case PathType.GameData:
                    path = GetGameDataPath();
                    break;
            }

            // 标准化路径，确保使用统一的分隔符
            path = NormalizePath(path);

            // 缓存路径
            _pathCache[pathType] = path;

            return path;
        }
        
        /// <summary>
        /// 获取本地Bundle文件的路径
        /// </summary>
        /// <param name="filePath">Bundle名称</param>
        /// <returns>Bundle路径</returns>
        public static string GetLocalBundleFilePath(PathType basePathType, string filePath = "")
        {
            string platformFolder = GetPlatformFolder();
            return PathManager.Combine(basePathType, "AssetBundles", platformFolder, filePath);
        }

        /// <summary>
        /// 获取StreamingAssets路径
        /// </summary>
        /// <returns>StreamingAssets完整路径</returns>
        private static string GetStreamingAssetsPath()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            return Application.streamingAssetsPath;
#elif UNITY_ANDROID
            return Application.streamingAssetsPath;
#elif UNITY_IOS
            return Application.streamingAssetsPath;
#elif UNITY_WEBGL
            return Application.streamingAssetsPath;
#else
            return Application.streamingAssetsPath;
#endif
        }

        /// <summary>
        /// 获取持久化数据路径
        /// </summary>
        /// <returns>持久化数据完整路径</returns>
        private static string GetPersistentDataPath()
        {
            return Application.persistentDataPath;
        }

        /// <summary>
        /// 获取临时缓存路径
        /// </summary>
        /// <returns>临时缓存完整路径</returns>
        private static string GetTemporaryCachePath()
        {
            return Application.temporaryCachePath;
        }

        /// <summary>
        /// 获取AssetBundle输出路径
        /// </summary>
        /// <returns>AssetBundle输出完整路径</returns>
        private static string GetAssetBundleOutputPath()
        {
            string platformFolder = GetPlatformFolder();

#if UNITY_EDITOR
            // 编辑器下输出到项目根目录的AssetBundles文件夹
            return Path.Combine(Application.dataPath, "..", "AssetBundles", platformFolder);
#elif UNITY_STANDALONE_WIN
            // Windows平台输出到StreamingAssets下的AssetBundles文件夹
            return Path.Combine(Application.streamingAssetsPath, "AssetBundles", platformFolder);
#elif UNITY_ANDROID
            // Android平台输出到PersistentDataPath下的AssetBundles文件夹
            return Path.Combine(Application.persistentDataPath, "AssetBundles", platformFolder);
#elif UNITY_IOS
            // iOS平台输出到PersistentDataPath下的AssetBundles文件夹
            return Path.Combine(Application.persistentDataPath, "AssetBundles", platformFolder);
#elif UNITY_WEBGL
            // WebGL平台输出到StreamingAssets下的AssetBundles文件夹
            return Path.Combine(Application.streamingAssetsPath, "AssetBundles", platformFolder);
#else
            return Path.Combine(Application.streamingAssetsPath, "AssetBundles", platformFolder);
#endif
        }

        /// <summary>
        /// 获取游戏数据路径
        /// </summary>
        /// <returns>游戏数据完整路径</returns>
        private static string GetGameDataPath()
        {
#if UNITY_EDITOR
            // 编辑器下从Assets路径加载
            return Application.dataPath;
#elif UNITY_STANDALONE_WIN
            // Windows平台从StreamingAssets或本地缓存加载
            return Path.Combine(Application.streamingAssetsPath, "GameData");
#elif UNITY_ANDROID
            // Android平台从PersistentDataPath加载
            return Path.Combine(Application.persistentDataPath, "GameData");
#elif UNITY_IOS
            // iOS平台从PersistentDataPath加载
            return Path.Combine(Application.persistentDataPath, "GameData");
#elif UNITY_WEBGL
            // WebGL平台从StreamingAssets加载
            return Path.Combine(Application.streamingAssetsPath, "GameData");
#else
            return Path.Combine(Application.streamingAssetsPath, "GameData");
#endif
        }

        /// <summary>
        /// 获取当前平台对应的文件夹名称
        /// </summary>
        /// <returns>平台文件夹名称</returns>
        private static string GetPlatformFolder()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return "StandaloneWindows64";
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    return "StandaloneOSX";
                case RuntimePlatform.LinuxPlayer:
                    return "StandaloneLinux64";
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "IOS";
                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";
                // case RuntimePlatform.WeChatGame:
                //     return "WeChatGame";
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// 标准化路径，使用统一的分隔符
        /// </summary>
        /// <param name="path">原始路径</param>
        /// <returns>标准化后的路径</returns>
        private static string NormalizePath(string path)
        {
            // 将反斜杠替换为正斜杠
            path = path.Replace('\\', '/');

            // 确保路径末尾没有斜杠
            if (path.EndsWith("/"))
            {
                path = path.Substring(0, path.Length - 1);
            }

            return path;
        }

        /// <summary>
        /// 组合路径
        /// </summary>
        /// <param name="pathType">基础路径类型</param>
        /// <param name="subPaths">子路径</param>
        /// <returns>组合后的完整路径</returns>
        public static string Combine(PathType pathType, params string[] subPaths)
        {
            string basePath = GetPath(pathType);
            return Combine(basePath, subPaths);
        }

        /// <summary>
        /// 组合路径
        /// </summary>
        /// <param name="basePath">基础路径</param>
        /// <param name="subPaths">子路径</param>
        /// <returns>组合后的完整路径</returns>
        public static string Combine(string basePath, params string[] subPaths)
        {
            string combinedPath = basePath;

            foreach (string subPath in subPaths)
            {
                combinedPath = Path.Combine(combinedPath, subPath);
            }

            return NormalizePath(combinedPath);
        }

        /// <summary>
        /// 获取资源文件的完整路径
        /// </summary>
        /// <param name="pathType">路径类型</param>
        /// <param name="assetPath">资源相对路径</param>
        /// <returns>资源完整路径</returns>
        public static string GetAssetPath(PathType pathType, string assetPath)
        {
            string basePath = GetPath(pathType);
            return Combine(basePath, assetPath);
        }

        /// <summary>
        /// 检查路径是否存在
        /// </summary>
        /// <param name="pathType">路径类型</param>
        /// <param name="subPath">子路径</param>
        /// <returns>路径是否存在</returns>
        public static bool Exists(PathType pathType, string subPath = null)
        {
            string fullPath = subPath == null ? GetPath(pathType) : Combine(pathType, subPath);
            return Directory.Exists(fullPath) || File.Exists(fullPath);
        }

        /// <summary>
        /// 创建目录（如果不存在）
        /// </summary>
        /// <param name="pathType">路径类型</param>
        /// <param name="subPath">子路径</param>
        public static void CreateDirectory(PathType pathType, string subPath = null)
        {
            string fullPath = subPath == null ? GetPath(pathType) : Combine(pathType, subPath);

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="pathType">路径类型</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件大小（字节），如果文件不存在则返回-1</returns>
        public static long GetFileSize(PathType pathType, string filePath)
        {
            string fullPath = Combine(pathType, filePath);

            if (File.Exists(fullPath))
            {
                return new FileInfo(fullPath).Length;
            }

            return -1;
        }

        /// <summary>
        /// 获取文件最后修改时间
        /// </summary>
        /// <param name="pathType">路径类型</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>最后修改时间，如果文件不存在则返回DateTime.MinValue</returns>
        public static DateTime GetLastWriteTime(PathType pathType, string filePath)
        {
            string fullPath = Combine(pathType, filePath);

            if (File.Exists(fullPath))
            {
                return File.GetLastWriteTime(fullPath);
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// 清除路径缓存
        /// </summary>
        public static void ClearCache()
        {
            _pathCache.Clear();
        }

        /// <summary>
        /// 打印各平台路径信息（调试用）
        /// </summary>
        public static void LogPaths()
        {
            Debug.Log("=== PathManager 路径信息 ===");
            Debug.Log($"当前平台: {Application.platform}");
            Debug.Log($"StreamingAssets: {GetPath(PathType.StreamingAssets)}");
            Debug.Log($"PersistentData: {GetPath(PathType.PersistentData)}");
            Debug.Log($"TemporaryCache: {GetPath(PathType.TemporaryCache)}");
            Debug.Log($"AssetBundleOutput: {GetPath(PathType.AssetBundleOutput)}");
            Debug.Log($"GameData: {GetPath(PathType.GameData)}");
            Debug.Log("============================");
        }
    }
}