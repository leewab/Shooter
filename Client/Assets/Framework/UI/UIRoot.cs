using UnityEngine;

namespace Framework.UIFramework
{
    /// <summary>
    /// UI层级定义
    /// </summary>
    public class UIRoot : MonoBehaviour
    {
        public Transform BackgroundLayer { get; set; }
        public Transform NormalLayer { get; set; }
        public Transform PopupLayer { get; set; }
        public Transform ToastLayer { get; set; }
        public Transform TopLayer { get; set; }
        
        private static Camera _UICamera;
        public static Camera UICamera 
        { 
            get
            {
                if (_UICamera == null)
                {
                    _UICamera = GameObject.Find("UIRoot/UICamera").GetComponent<Camera>();
                }
                return _UICamera;
            }
        }

        /// <summary>
        /// 根据面板类型获取对应的层级Transform
        /// </summary>
        public Transform GetLayerByPanelType(PanelType panelType)
        {
            return panelType switch
            {
                PanelType.Normal => NormalLayer,
                PanelType.Fixed => BackgroundLayer,
                PanelType.Popup => PopupLayer,
                PanelType.Toast => ToastLayer,
                _ => NormalLayer
            };
        }

        /// <summary>
        /// 获取最高层级（用于特殊UI，如引导、Loading等）
        /// </summary>
        public Transform GetTopLayer()
        {
            return TopLayer;
        }
    }
}
