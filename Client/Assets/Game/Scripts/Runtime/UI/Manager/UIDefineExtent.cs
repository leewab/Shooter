using System.Collections.Generic;
using Framework.UIFramework;

namespace GameUI
{
    public partial class UIDefine
    {
        public UIDefine()
        {
            UIInfos.Add(
                nameof(UIGameResultPanel), 
                new UIInfo()
                {
                    UIPath = "UIGameResultPanel",
                    PanelType = PanelType.Popup,
                }
            );
            
            // UIInfos.Add(
            //     nameof(UIGameResultPanel), 
            //     new UIInfo()
            //     {
            //         UIPath = "UIGameResultPanel",
            //         PanelType = PanelType.Popup,
            //     }
            // );
        }

    }
}