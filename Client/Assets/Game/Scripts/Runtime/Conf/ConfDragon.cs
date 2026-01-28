// ===========================================
// 自动生成的C#配置类
// 表名称: ConfDragon
// 表描述: 龙
// 生成时间: 2026-01-27 23:23:00
// 工具: ExcelToJsonTool
// 请勿手动修改此文件，重新生成将被覆盖
// ===========================================

using Newtonsoft.Json;
using System;

namespace GameConfig
{
    /// <summary>
    /// 龙
    /// </summary>
    [Serializable]
    public class ConfDragon  : BaseConf
    {
        /// <summary>
        /// 龙名称
        /// </summary>
        [JsonProperty("DraongName")]
        public string DraongName { get; set; } = string.Empty;

        /// <summary>
        /// 正常速度
        /// </summary>
        [JsonProperty("NormalMoveSpeed")]
        public int NormalMoveSpeed { get; set; } = 0;

        /// <summary>
        /// 最大速度
        /// </summary>
        [JsonProperty("MaxMoveSpeed")]
        public int MaxMoveSpeed { get; set; } = 0;

        /// <summary>
        /// 最大速度持续时间
        /// </summary>
        [JsonProperty("MaxSpeedDurationTime")]
        public float MaxSpeedDurationTime { get; set; } = 0f;

        /// <summary>
        /// 节点之间的间距
        /// </summary>
        [JsonProperty("DragonJointSpacing")]
        public float DragonJointSpacing { get; set; } = 0f;

        [JsonProperty("PositionSmoothness")]
        public float PositionSmoothness { get; set; } = 0f;

        [JsonProperty("DragonJoints")]
        public int[] DragonJoints { get; set; } = new int[] { };

        [JsonProperty("DragonJointColors")]
        public int[] DragonJointColors { get; set; } = new int[] { };

        /// <summary>
        /// 返回对象的字符串表示
        /// </summary>
        public override string ToString()
        {
            return $"ConfDragon " + string.Join(", ", new string[] { $"Id={Id}", $"DraongName={DraongName}", $"NormalMoveSpeed={NormalMoveSpeed}", $"MaxMoveSpeed={MaxMoveSpeed}", $"MaxSpeedDurationTime={MaxSpeedDurationTime}", $"DragonJointSpacing={DragonJointSpacing}", $"PositionSmoothness={PositionSmoothness}", $"DragonJoints={DragonJoints}", $"DragonJointColors={DragonJointColors}" });
        }
    }
}
