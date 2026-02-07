// ===========================================
// 自动生成的C#配置类
// 表名称: ConfTurret
// 表描述: 炮台
// 生成时间: 2026-02-04 22:32:24
// 工具: ExcelToJsonTool
// 请勿手动修改此文件，重新生成将被覆盖
// ===========================================

using Newtonsoft.Json;
using System;

namespace GameConfig
{
    /// <summary>
    /// 炮台
    /// </summary>
    [Serializable]
    public class ConfTurret  : BaseConf
    {
        /// <summary>
        /// 图片名称
        /// </summary>
        [JsonProperty("Icon")]
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// 颜色类型
        /// </summary>
        [JsonProperty("ColorType")]
        public ColorType ColorType { get; set; } = (ColorType)0;

        /// <summary>
        /// 攻击冷却时间
        /// </summary>
        [JsonProperty("AttackCooldown")]
        public float AttackCooldown { get; set; } = 0f;

        /// <summary>
        /// 子弹ID
        /// </summary>
        [JsonProperty("BulletId")]
        public int BulletId { get; set; } = 0;

        /// <summary>
        /// 最大攻击数量
        /// </summary>
        [JsonProperty("MaxHitNum")]
        public int MaxHitNum { get; set; } = 0;

        [JsonProperty("FireSound")]
        public string FireSound { get; set; } = string.Empty;

        /// <summary>
        /// 后坐力后退距离
        /// </summary>
        [JsonProperty("RecoilDistance")]
        public float RecoilDistance { get; set; } = 0f;

        /// <summary>
        /// 后坐力复位时间
        /// </summary>
        [JsonProperty("RecoilDuration")]
        public float RecoilDuration { get; set; } = 0f;

        /// <summary>
        /// 后坐力旋转角度（枪口上抬）
        /// </summary>
        [JsonProperty("RecoilRotation")]
        public float RecoilRotation { get; set; } = 0f;

        /// <summary>
        /// 炮口特效名称
        /// </summary>
        [JsonProperty("MuzzleEffectName")]
        public string MuzzleEffectName { get; set; } = string.Empty;

        /// <summary>
        /// 炮口特效缩放
        /// </summary>
        [JsonProperty("MuzzleEffectScale")]
        public float MuzzleEffectScale { get; set; } = 0f;

        /// <summary>
        /// 返回对象的字符串表示
        /// </summary>
        public override string ToString()
        {
            return $"ConfTurret " + string.Join(", ", new string[] { $"Id={Id}", $"Icon={Icon}", $"ColorType={ColorType}", $"AttackCooldown={AttackCooldown}", $"BulletId={BulletId}", $"MaxHitNum={MaxHitNum}", $"FireSound={FireSound}", $"RecoilDistance={RecoilDistance}", $"RecoilDuration={RecoilDuration}", $"RecoilRotation={RecoilRotation}", $"MuzzleEffectName={MuzzleEffectName}", $"MuzzleEffectScale={MuzzleEffectScale}" });
        }
    }
}
