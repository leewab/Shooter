using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance => _instance;

    [SerializeField] private GameObject _uiRootPrefab;
    [SerializeField] private GameObject _resourceManagerPrefab;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        // 初始化UI根节点
        var uiRoot = Instantiate(_uiRootPrefab);
        UIManager.Instance.SetUIRoot(uiRoot.transform);
        
        UIManager.Instance.OpenPanel("UIGameOperPanel");
    }

    // 微信小游戏特定API调用
    public void CallWeChatAPI(string apiName, string jsonData)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalCall("wx." + apiName, jsonData);
#else
        Debug.Log($"Call WeChat API: {apiName} with data: {jsonData}");
#endif
    }
}