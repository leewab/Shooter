using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class UIManager
{
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null) 
            {
                _instance = new UIManager();
            }
            return _instance;
        }
    }
    
    private Transform _uiRoot;
    private Dictionary<string, GameObject> _panelDict = new Dictionary<string, GameObject>();
    
    public void SetUIRoot(Transform root)
    {
        _uiRoot = root;
        Debug.Log($"UI Root set to: {root.name}");
    }
    
    // 打开UI面板
    public void OpenPanel(string panelName)
    {
        if (!panelName.StartsWith("Assets/"))
        {
            panelName = $"Assets/Res/UI/{panelName}.prefab";
        }
        if (_panelDict.ContainsKey(panelName))
        {
            _panelDict[panelName].SetActive(true);
            return;
        }
        
        // Addressable加载示例
        Addressables.LoadAssetAsync<GameObject>(panelName).Completed += handle => {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var panelPrefab = handle.Result;
                var panelObj = Object.Instantiate(panelPrefab, _uiRoot);
                _panelDict.Add(panelName, panelObj);
                Debug.Log($"Panel {panelName} loaded successfully");
            }
            else
            {
                Debug.LogError($"Failed to load panel {panelName}: {handle.OperationException}");
            }
        };
    }
    
    // 关闭UI面板
    public void ClosePanel(string panelName)
    {
        if (_panelDict.TryGetValue(panelName, out var panel))
        {
            panel.SetActive(false);
        }
    }
    
    // 完全移除UI面板
    public void RemovePanel(string panelName)
    {
        if (_panelDict.TryGetValue(panelName, out var panel))
        {
            Object.Destroy(panel);
            _panelDict.Remove(panelName);
            Addressables.Release(panel); // 释放Addressable资源
        }
    }
}
