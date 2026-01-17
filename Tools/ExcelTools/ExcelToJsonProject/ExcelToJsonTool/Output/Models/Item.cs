// ===========================================
// 自动生成的C#配置类
// 源文件：Item.xlsx
// 生成时间：2026-01-12 19:18:16
// 工具：ExcelToJsonTool
// 请勿手动修改此文件，重新生成将被覆盖
// ===========================================

using Newtonsoft.Json;
using System;

namespace GameConfig
{
    [Serializable]
    public class Item
    {
        /// <summary>
        /// 道具ID
        /// </summary>
        [JsonProperty("id")]
        public int id { get; set; } = 0;

        /// <summary>
        /// 道具名称
        /// </summary>
        [JsonProperty("name")]
        public string name { get; set; } = string.Empty;

        /// <summary>
        /// 价格
        /// </summary>
        [JsonProperty("price")]
        public float price { get; set; } = 0f;

        /// <summary>
        /// 是否可使用
        /// </summary>
        [JsonProperty("canUse")]
        public bool canUse { get; set; } = false;

        /// <summary>
        /// 返回对象的字符串表示
        /// </summary>
        public override string ToString()
        {
            return $"Item " + string.Join(", ", new string[] { $"id={id}", $"name={name}", $"price={price}", $"canUse={canUse}" });
        }
    }
}
