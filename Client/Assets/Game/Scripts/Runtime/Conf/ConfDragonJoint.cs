// ===========================================
// 自动生成的C#配置类
// 表名称: ConfDragonJoint
// 表描述: 龙骨
// 生成时间: 2026-02-20 15:11:08
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
        /// 资源
        /// </summary>
        [JsonProperty("Prefab")]
        public string Prefab { get; set; } = string.Empty;

        /// <summary>
        /// 节点生命
        /// </summary>
        [JsonProperty("Health")]
        public int Health { get; set; } = 0;

        /// <summary>
        /// 节点类型
        /// </summary>
        [JsonProperty("Type")]
        public int Type { get; set; } = 0;

        /// <summary>
        /// 受击特效
        /// </summary>
        [JsonProperty("DamageEffect")]
        public string DamageEffect { get; set; } = string.Empty;

        /// <summary>
        /// 受击音效
        /// </summary>
        [JsonProperty("DamageAudio")]
        public string DamageAudio { get; set; } = string.Empty;

        /// <summary>
        /// 击毁特效
        /// </summary>
        [JsonProperty("DestroyEffect")]
        public string DestroyEffect { get; set; } = string.Empty;

        /// <summary>
        /// 击毁音效
        /// </summary>
        [JsonProperty("DestroyAudio")]
        public string DestroyAudio { get; set; } = string.Empty;

        /// <summary>
        /// 返回对象的字符串表示
        /// </summary>
        public override string ToString()
        {
            return $"ConfDragonJoint " + string.Join(", ", new string[] { $"Id={Id}", $"Prefab={Prefab}", $"Health={Health}", $"Type={Type}", $"DamageEffect={DamageEffect}", $"DamageAudio={DamageAudio}", $"DestroyEffect={DestroyEffect}", $"DestroyAudio={DestroyAudio}" });
        }
    }
}
