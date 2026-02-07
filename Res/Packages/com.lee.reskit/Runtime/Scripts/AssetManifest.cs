using System.Collections.Generic;

namespace ResKit
{
    /// <summary>
    /// AssetManifest数据结构
    /// </summary>
    [System.Serializable]
    public class AssetManifest
    {
        public string BuildTime;
        public int TotalAssets;
        public int TotalBundles;
        public List<AssetInfo> Assets;
    }

    /// <summary>
    /// Asset信息
    /// </summary>
    [System.Serializable]
    public class AssetInfo
    {
        public string AssetPath;
        public string BundleName;
        public string[] Dependencies;
    }

    /// <summary>
    /// Bundle信息
    /// </summary>
    [System.Serializable]
    public class BundleInfo
    {
        public string BundleName;
        public string[] Assets;
    }
}