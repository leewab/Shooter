#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEditor.AddressableAssets;

public class AddressableWeChatMarking : EditorWindow
{
    [MenuItem("Window/WeChat Addressable Helper")]
    public static void ShowWindow()
    {
        var window = GetWindow<AddressableWeChatMarking>();
        window.titleContent = new GUIContent("WeChat标记助手");
        window.minSize = new Vector2(400, 300);
    }

    void OnGUI()
    {
        GUILayout.Label("资源标记步骤说明", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("1. 打开项目窗口", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "在Unity编辑器顶部菜单选择: \n" +
            "Window > General > Project\n" +
            "或使用快捷键 Ctrl+7 (Windows) / Cmd+7 (Mac)",
            MessageType.Info);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("2. 选择资源文件", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "在Project窗口中进行以下操作:\n" +
            "• 单击选择单个文件\n" +
            "• 按住Ctrl/Cmd键多选文件\n" +
            "• 按住Shift键选择连续范围文件\n" +
            "支持选择的类型:\n" +
            "- Prefab预制体\n" +
            "- Texture纹理\n" +
            "- AudioClip音频\n" +
            "- ScriptableObject数据文件",
            MessageType.Info);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("3. 执行标记命令", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "右键点击已选中的资源 > WeChat > Mark as Addressable\n" +
            "或使用顶部菜单: Tools > Mark Resources for WeChat",
            MessageType.Info);
        
        EditorGUILayout.Space();
        if (GUILayout.Button("立即打开Project窗口"))
        {
            EditorApplication.ExecuteMenuItem("Window/General/Project");
        }
    }
}

[InitializeOnLoad]
public static class RightClickMenu
{
    static RightClickMenu()
    {
        EditorApplication.delayCall += Initialize;
    }

    private static void Initialize()
    {
        // 确保Addressable系统已加载
        if (!AddressableAssetSettingsDefaultObject.SettingsExists)
        {
            Debug.LogWarning("Addressable Asset Settings not initialized");
        }
    }

    [MenuItem("Assets/WeChat/Mark as Addressable")]
    private static void MarkSelectedAssets()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Asset Settings not found");
            return;
        }

        foreach (var obj in Selection.objects)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            var guid = AssetDatabase.AssetPathToGUID(path);
            settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
            Debug.Log($"Marked {obj.name} as Addressable");
        }
        
        AssetDatabase.SaveAssets();
    }
    
    [MenuItem("Assets/WeChat/Mark as Addressable", true)]
    private static bool ValidateSelection()
    {
        return Selection.objects != null && Selection.objects.Length > 0;
    }
}
#endif
