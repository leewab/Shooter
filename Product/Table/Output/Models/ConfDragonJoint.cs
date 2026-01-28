// ===========================================
// 自动生成的C#配置类
// 表名称: ConfDragonJoint
// 表描述: 龙骨
// 生成时间: 2026-01-27 23:23:00
// 工具: ExcelToJsonTool
// 请勿手动修改此文件，重新生成将被覆盖
// ===========================================

using Newtonsoft.Json;
using System;

namespace GameConfig
{
    /// <summary>
    /// 龙骨
    /// </summary>
    [Serializable]
    public class ConfDragonJoint  : BaseConf
    {
        /// <summary>
        /// 节点生命
        /// </summary>
        [JsonProperty("Health")]
        public int Health { get; set; } = 0;

        /// <summary>
        /// 返回对象的字符串表示
        /// </summary>
        public override string ToString()
        {
            return $"ConfDragonJoint " + string.Join(", ", new string[] { $"Id={Id}", $"Health={Health}" });
        }
    }
}
