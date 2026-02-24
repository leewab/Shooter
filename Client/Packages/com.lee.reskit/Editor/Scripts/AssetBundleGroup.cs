using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ResKit
{
    [Serializable]
    public enum BundlePackingMode
    {
        SingleBundle,
        PerFolderBundle,
        PerFileBundle
    }

    [Serializable]
    public class AssetBundleGroup
    {
        public string id;
        public string name;
        public BundlePackingMode packingMode;
        public bool enabled = true;
        public List<string> assetPaths = new List<string>();

        public AssetBundleGroup()
        {
            id = Guid.NewGuid().ToString();
            name = "New Group";
            packingMode = BundlePackingMode.PerFolderBundle;
        }

        public void AddAsset(string path)
        {
            if (!string.IsNullOrEmpty(path) && !assetPaths.Contains(path))
                assetPaths.Add(path);
        }

        public void RemoveAsset(string path) => assetPaths.Remove(path);
        public void ClearAssets() => assetPaths.Clear();
    }

    [Serializable]
    public class BuildSettings
    {
        public List<AssetBundleGroup> groups = new List<AssetBundleGroup>();
        public BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
        public BuildAssetBundleOptions buildOptions = BuildAssetBundleOptions.ChunkBasedCompression;
        public bool copyToStreamingAssets = false;
        public bool addBundleSuffix = true;
    }
}