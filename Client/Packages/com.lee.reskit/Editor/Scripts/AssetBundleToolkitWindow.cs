using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace ResKit
{
    public class AssetBundleToolkitWindow : EditorWindow
    {
        private BuildSettings settings;
        private int selectedGroup = -1;
        private Vector2 leftScrollPos;
        private Vector2 rightScrollPos;

        [MenuItem("Tools/AssetBundle Toolkit")]
        static void ShowWindow()
        {
            var window = GetWindow<AssetBundleToolkitWindow>("AssetBundle Toolkit");
            window.minSize = new Vector2(800, 600);
        }

        void OnEnable() => settings = AssetBundleUtility.LoadSettings();

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));

            // 左侧：分组列表
            DrawLeftPanel();

            // 右侧：分组详情
            DrawRightPanel();

            EditorGUILayout.EndHorizontal();

            // 底部：打包设置
            DrawBottomPanel();
        }

        void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(250), GUILayout.ExpandHeight(true));
            EditorGUILayout.LabelField("打包分组", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            leftScrollPos = EditorGUILayout.BeginScrollView(leftScrollPos, GUILayout.ExpandHeight(true));

            for (int i = 0; i < settings.groups.Count; i++)
            {
                DrawGroupListItem(settings.groups[i], i);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);

            if (GUILayout.Button("+ 新建分组", GUILayout.Height(30)))
            {
                settings.groups.Add(new AssetBundleGroup());
                selectedGroup = settings.groups.Count - 1;
                AssetBundleUtility.SaveSettings(settings);
            }

            EditorGUILayout.EndVertical();
        }

        void DrawGroupListItem(AssetBundleGroup group, int index)
        {
            bool isSelected = selectedGroup == index;

            EditorGUILayout.BeginHorizontal();

            // 启用开关
            EditorGUI.BeginChangeCheck();
            group.enabled = EditorGUILayout.Toggle(group.enabled, GUILayout.Width(20));
            if (EditorGUI.EndChangeCheck()) AssetBundleUtility.SaveSettings(settings);

            // 分组按钮
            GUIStyle buttonStyle = isSelected
                ? new GUIStyle("Button") { fontStyle = FontStyle.Bold }
                : new GUIStyle("Button");

            if (GUILayout.Button($"{group.name} ({group.assetPaths.Count})", buttonStyle,
                    GUILayout.ExpandWidth(true)))
            {
                selectedGroup = index;
            }

            // 删除按钮
            if (GUILayout.Button("×", GUILayout.Width(25)))
            {
                if (EditorUtility.DisplayDialog("确认删除",
                        $"确定要删除分组 '{group.name}' 吗？\n该操作不可撤销。", "删除", "取消"))
                {
                    settings.groups.RemoveAt(index);
                    AssetBundleUtility.SaveSettings(settings);

                    if (selectedGroup == index)
                        selectedGroup = -1;
                    else if (selectedGroup > index)
                        selectedGroup--;
                }
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(2);
        }

        void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            if (selectedGroup >= 0 && selectedGroup < settings.groups.Count)
            {
                var group = settings.groups[selectedGroup];

                // 分组设置
                EditorGUILayout.LabelField($"分组设置: {group.name}", EditorStyles.boldLabel);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.BeginChangeCheck();
                group.name = EditorGUILayout.TextField("分组名称", group.name);
                group.packingMode = (BundlePackingMode)EditorGUILayout.EnumPopup("打包模式", group.packingMode);
                if (EditorGUI.EndChangeCheck()) AssetBundleUtility.SaveSettings(settings);
                EditorGUILayout.EndVertical();

                // 资源管理
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("资源管理", EditorStyles.boldLabel);

                DrawAssetManagement(group);
            }
            else
            {
                EditorGUILayout.HelpBox("请选择一个分组或创建新分组", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        void DrawAssetManagement(AssetBundleGroup group)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 操作按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("添加文件", GUILayout.Height(25)))
            {
                AddFileToGroup(group);
            }

            if (GUILayout.Button("添加文件夹", GUILayout.Height(25)))
            {
                AddFolderToGroup(group);
            }

            if (GUILayout.Button("扫描文件夹内容", GUILayout.Height(25)))
            {
                ScanFolderForAssets(group);
            }

            if (GUILayout.Button("清空列表", GUILayout.Height(25)) && group.assetPaths.Count > 0)
            {
                if (EditorUtility.DisplayDialog("确认", "清空当前分组的所有资源？", "清空", "取消"))
                {
                    group.ClearAssets();
                    AssetBundleUtility.SaveSettings(settings);
                }
            }

            EditorGUILayout.EndHorizontal();

            // 拖拽区域
            Rect dragRect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
            GUI.Box(dragRect, "拖拽资源或文件夹到此区域添加", EditorStyles.helpBox);
            HandleDragDrop(dragRect, group);

            // 资源列表
            if (group.assetPaths.Count > 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"资源列表 ({group.assetPaths.Count}):", EditorStyles.miniBoldLabel);

                rightScrollPos = EditorGUILayout.BeginScrollView(rightScrollPos, GUILayout.ExpandHeight(true));

                for (int i = 0; i < group.assetPaths.Count; i++)
                {
                    DrawAssetListItem(group.assetPaths[i], i, group);
                }

                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.Space(20);
                EditorGUILayout.LabelField("暂无资源", EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.EndVertical();
        }

        void DrawAssetListItem(string path, int index, AssetBundleGroup group)
        {
            EditorGUILayout.BeginHorizontal();

            // 图标
            Texture icon = AssetDatabase.GetCachedIcon(path);
            if (icon != null)
            {
                GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));
            }

            // 路径显示
            EditorGUILayout.LabelField(Path.GetFileName(path), GUILayout.Width(150));
            EditorGUILayout.LabelField(path, EditorStyles.miniLabel, GUILayout.ExpandWidth(true));

            // 预览按钮
            if (GUILayout.Button(" * ", GUILayout.Width(25)))
            {
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (asset != null) EditorGUIUtility.PingObject(asset);
            }

            // 移除按钮
            if (GUILayout.Button(" × ", GUILayout.Width(25)))
            {
                group.RemoveAsset(path);
                AssetBundleUtility.SaveSettings(settings);
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawBottomPanel()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("构建设置", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 构建设置
            settings.buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("目标平台", settings.buildTarget);
            settings.buildOptions =
                (BuildAssetBundleOptions)EditorGUILayout.EnumFlagsField("打包选项", settings.buildOptions);

            // 额外选项
            EditorGUILayout.Space(5);
            settings.addBundleSuffix = EditorGUILayout.ToggleLeft(" 添加 .ab 后缀", settings.addBundleSuffix);
            settings.copyToStreamingAssets =
                EditorGUILayout.ToggleLeft(" 拷贝到 StreamingAssets", settings.copyToStreamingAssets);

            EditorGUILayout.EndVertical();

            // 操作按钮
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("保存设置", GUILayout.Height(25)))
            {
                AssetBundleUtility.SaveSettings(settings);
                EditorUtility.DisplayDialog("提示", "设置已保存", "确定");
            }

            GUILayout.FlexibleSpace();

            // // 预览和打包
            // var builds = AssetBundleUtility.GenerateBuilds(settings);
            // EditorGUILayout.LabelField($"将生成 {builds.Length} 个Bundle", EditorStyles.miniLabel, GUILayout.Width(120));

            if (GUILayout.Button("预览Bundle", GUILayout.Height(25), GUILayout.Width(100)))
            {
                var builds = AssetBundleUtility.GenerateBuilds(settings);
                PreviewBundles(builds);
            }

            if (GUILayout.Button("开始打包", GUILayout.Height(40), GUILayout.Width(150)))
            {
                BuildAssetBundles();
            }

            EditorGUILayout.EndHorizontal();
        }

        void AddFileToGroup(AssetBundleGroup group)
        {
            string selectedPath = EditorUtility.OpenFilePanel("选择文件", "Assets", "*");

            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    string relativePath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    group.AddAsset(relativePath);
                    AssetBundleUtility.SaveSettings(settings);
                    Debug.Log($"添加文件: {relativePath}");
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "请选择项目内的文件", "确定");
                }
            }
        }

        void AddFolderToGroup(AssetBundleGroup group)
        {
            string path = EditorUtility.OpenFolderPanel("选择文件夹", "Assets", "");

            if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
            {
                string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
                group.AddAsset(relativePath);
                AssetBundleUtility.SaveSettings(settings);
                Debug.Log($"添加文件夹: {relativePath}");
            }
        }

        void ScanFolderForAssets(AssetBundleGroup group)
        {
            string folderPath = EditorUtility.OpenFolderPanel("选择要扫描的文件夹", "Assets", "");

            if (!string.IsNullOrEmpty(folderPath) && folderPath.StartsWith(Application.dataPath))
            {
                string relativeFolder = "Assets" + folderPath.Substring(Application.dataPath.Length);

                // 扫描文件夹内所有文件
                string[] files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
                int added = 0;

                foreach (string file in files)
                {
                    if (!file.EndsWith(".meta") && !file.EndsWith(".cs"))
                    {
                        string relativePath = "Assets" + file.Substring(Application.dataPath.Length);
                        group.AddAsset(relativePath);
                        added++;
                    }
                }

                if (added > 0)
                {
                    AssetBundleUtility.SaveSettings(settings);
                    Debug.Log($"扫描并添加了 {added} 个文件");
                }
            }
        }

        void HandleDragDrop(Rect rect, AssetBundleGroup group)
        {
            Event evt = Event.current;
            if (!rect.Contains(evt.mousePosition)) return;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    evt.Use();
                    break;

                case EventType.DragPerform:
                    DragAndDrop.AcceptDrag();
                    evt.Use();

                    int added = 0;
                    foreach (string path in DragAndDrop.paths)
                    {
                        if (!string.IsNullOrEmpty(path))
                        {
                            group.AddAsset(path);
                            added++;
                        }
                    }

                    if (added > 0)
                    {
                        AssetBundleUtility.SaveSettings(settings);
                        Debug.Log($"通过拖拽添加了 {added} 个资源");
                    }

                    break;
            }
        }

        void PreviewBundles(AssetBundleBuild[] builds)
        {
            var previewWindow = GetWindow<AssetBundlePreviewWindow>("Bundle预览");
            previewWindow.Show();
            previewWindow.SetBuilds(builds);
        }

        void BuildAssetBundles()
        {
            try
            {
                // 生成打包配置
                var builds = AssetBundleUtility.GenerateBuilds(settings);

                if (builds.Length == 0)
                {
                    EditorUtility.DisplayDialog("提示", "没有可打包的资源", "确定");
                    return;
                }

                Debug.LogError(settings.buildTarget.ToString());
                string platformFolder = PathManager.GetBuildTargetPlatformFolder(settings.buildTarget);
                string outputPath = PathManager.GetLocalBundleFilePath(PathType.AssetBundleOutput, platformFolder, "");

                // 打包
                EditorUtility.DisplayProgressBar("打包中", "正在构建AssetBundles...", 0.3f);
                AssetBundleUtility.Build(
                    outputPath, 
                    builds, 
                    settings.buildOptions 
                            | BuildAssetBundleOptions.StrictMode 
                            | BuildAssetBundleOptions.DisableLoadAssetByFileName 
                            | BuildAssetBundleOptions.AssetBundleStripUnityVersion,
                    settings.buildTarget);

                // 生成Manifest
                EditorUtility.DisplayProgressBar("打包中", "生成Manifest文件...", 0.6f);
                GenerateManifest(outputPath, builds);

                // 拷贝到StreamingAssets
                if (settings.copyToStreamingAssets)
                {
                    EditorUtility.DisplayProgressBar("打包中", "拷贝到StreamingAssets...", 0.8f);
                    AssetBundleUtility.CopyToStreamingAssets(outputPath, settings.buildTarget);
                }

                EditorUtility.ClearProgressBar();

                // 显示结果
                string message = $"打包完成！\n" +
                                 $"输出目录: {outputPath}\n" +
                                 $"Bundle数量: {builds.Length}\n" +
                                 $"{(settings.copyToStreamingAssets ? "已拷贝到StreamingAssets" : "")}";

                if (EditorUtility.DisplayDialog("成功", message, "打开目录", "确定"))
                {
                    EditorUtility.RevealInFinder(outputPath);
                }
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("错误", $"打包失败: {e.Message}", "确定");
                Debug.LogError($"打包失败: {e}");
            }
        }

        void GenerateManifest(string outputPath, AssetBundleBuild[] builds)
        {
            var manifest = AssetBundleUtility.GenerateManifest(builds);
            string json = JsonUtility.ToJson(manifest, true);
            string manifestPath = Path.Combine(outputPath, ResourceManager.ASSET_MANIFEST_NAME);
            File.WriteAllText(manifestPath, json);

            Debug.Log($"Manifest文件已生成: {manifestPath}");
        }
    }
}