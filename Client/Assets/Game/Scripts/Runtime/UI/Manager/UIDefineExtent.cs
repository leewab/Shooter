using Framework.UIFramework;

namespace GameUI
{
    public partial class UIDefine
    {
        public static void Init()
        {
            UIInfos.Add(
                nameof(UIGameFailedPanel), 
                new UIInfo()
                {
                    UIPath = "UIGameFailedPanel",
                    PanelType = PanelType.Popup,
                }
            );
            
            UIInfos.Add(
                nameof(UIGameSuccessPanel), 
                new UIInfo()
                {
                    UIPath = "UIGameSuccessPanel",
                    PanelType = PanelType.Popup,
                }
            );

            UIInfos.Add(
             nameof(UIGameMainPanel),
             new UIInfo()
             {
                 UIPath = "UIGameMainPanel",
                 PanelType = PanelType.Normal,
             }
            );
        }


    }
}