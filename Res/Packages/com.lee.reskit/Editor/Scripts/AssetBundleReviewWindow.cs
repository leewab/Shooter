using System.IO;
using UnityEditor;
using UnityEngine;

namespace ResKit
{
    // Bundle预览窗口
    public class AssetBundlePreviewWindow : EditorWindow
    {
        private AssetBundleBuild[] builds;
        private Vector2 scrollPos;

        public void SetBuilds(AssetBundleBuild[] buildArray)
        {
            builds = buildArray;
        }

        void OnGUI()
        {
            if (builds == null || builds.Length == 0)
            {
                EditorGUILayout.HelpBox("没有可预览的Bundle", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Bundle预览 ({builds.Length}个)", EditorStyles.boldLabel);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            foreach (var build in builds)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"Bundle: {build.assetBundleName}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"包含 {build.assetNames.Length} 个资源:");

                EditorGUI.indentLevel++;
                foreach (var asset in build.assetNames)
                {
                    EditorGUILayout.LabelField($"• {Path.GetFileName(asset)}", EditorStyles.miniLabel);
                }

                EditorGUI.indentLevel--;

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
        }
    }
}