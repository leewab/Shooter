using System.Collections.Generic;
using Framework.UIFramework;

namespace GameUI
{
    public class UIInfo
    {
        public string UIPath;
        public PanelType PanelType;
    }
    
    public class UIDefine
    {
        public static Dictionary<string, UIInfo> UIInfos = new Dictionary<string, UIInfo>()
        {
            {
                nameof(UIGameResultPanel),
                new UIInfo()
                {
                    UIPath = "UIGameResultPanel",
                    PanelType = PanelType.Popup,
                }
            },
            
        };

        public static UIInfo GetUIInfo(string panelName)
        {
            if (!UIInfos.ContainsKey(panelName))
            {
                throw new KeyNotFoundException($"No UIInfo with name {panelName}");
            }
            
            return UIInfos[panelName];
        }

        public static UIInfo GetUIInfo<T>() where T : UIPanel
        {
            return GetUIInfo(typeof(T).Name);
        }
    }
}