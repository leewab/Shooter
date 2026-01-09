using UnityEngine;

public class WeChatAdapter : MonoBehaviour
{
    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // 初始化微信小游戏环境
        Application.ExternalEval(@"
            wx.onShow(function(res) {
                SendMessage('WeChatAdapter', 'OnWeChatShow', JSON.stringify(res));
            });
            
            wx.onHide(function() {
                SendMessage('WeChatAdapter', 'OnWeChatHide', '');
            });
        ");
#endif
    }

    // 微信小游戏显示回调
    private void OnWeChatShow(string jsonData)
    {
        Debug.Log("WeChat game shown: " + jsonData);
        // 恢复游戏逻辑
        Time.timeScale = 1;
    }

    // 微信小游戏隐藏回调
    private void OnWeChatHide()
    {
        Debug.Log("WeChat game hidden");
        // 暂停游戏逻辑
        Time.timeScale = 0;
    }

    // 分享游戏
    public void ShareGame(string title, string imageUrl)
    {
        GameManager.Instance.CallWeChatAPI("shareAppMessage", $"{{\"title\":\"{title}\",\"imageUrl\":\"{imageUrl}\"}}");
    }
}