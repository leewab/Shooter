// ===========================================
// 自动生成的C#配置类
// 表名称: ConfBullet
// 表描述: 子弹
// 生成时间: 2026-02-26 22:57:09
// 工具: ExcelToJsonTool
// 请勿手动修改此文件，重新生成将被覆盖
// ===========================================

using Newtonsoft.Json;
using System;

namespace GameConfig
{
    /// <summary>
    /// 子弹
    /// </summary>
    [Serializable]
    public class ConfBullet  : BaseConf
    {
        /// <summary>
        /// 子弹名称
        /// </summary>
        [JsonProperty("BulletName")]
        public string BulletName { get; set; } = string.Empty;

        /// <summary>
        /// 伤害值
        /// </summary>
        [JsonProperty("Damage")]
        public int Damage { get; set; } = 0;

        /// <summary>
        /// 速度
        /// </summary>
        [JsonProperty("Speed")]
        public float Speed { get; set; } = 0f;

        /// <summary>
        /// 最大飞行距离
        /// </summary>
        [JsonProperty("MaxTravelDistance")]
        public float MaxTravelDistance { get; set; } = 0f;

        /// <summary>
        /// 发射时的初始缩放
        /// </summary>
        [JsonProperty("StartScale")]
        public float StartScale { get; set; } = 0f;

        /// <summary>
        /// 缩放到正常大小的持续时间
        /// </summary>
        [JsonProperty("ScaleDuration")]
        public float ScaleDuration { get; set; } = 0f;

        /// <summary>
        /// 命中特效名称
        /// </summary>
        [JsonProperty("HitEffectName")]
        public string HitEffectName { get; set; } = string.Empty;

        /// <summary>
        /// 命中音效名称
        /// </summary>
        [JsonProperty("HitSoundName")]
        public string HitSoundName { get; set; } = string.Empty;

        /// <summary>
        /// 命中顿帧时长（秒）
        /// </summary>
        [JsonProperty("HitStopDuration")]
        public float HitStopDuration { get; set; } = 0f;

        /// <summary>
        /// 屏幕震动强度
        /// </summary>
        [JsonProperty("ScreenShakeIntensity")]
        public float ScreenShakeIntensity { get; set; } = 0f;

        /// <summary>
        /// 屏幕震动持续时间
        /// </summary>
        [JsonProperty("ScreenShakeDuration")]
        public float ScreenShakeDuration { get; set; } = 0f;

        /// <summary>
        /// Miss特效名称
        /// </summary>
        [JsonProperty("MissEffectName")]
        public string MissEffectName { get; set; } = string.Empty;

        /// <summary>
        /// Miss音效名称
        /// </summary>
        [JsonProperty("MissSoundName")]
        public string MissSoundName { get; set; } = string.Empty;

        /// <summary>
        /// 返回对象的字符串表示
        /// </summary>
        public override string ToString()
        {
            return $"ConfBullet " + string.Join(", ", new string[] { $"Id={Id}", $"BulletName={BulletName}", $"Damage={Damage}", $"Speed={Speed}", $"MaxTravelDistance={MaxTravelDistance}", $"StartScale={StartScale}", $"ScaleDuration={ScaleDuration}", $"HitEffectName={HitEffectName}", $"HitSoundName={HitSoundName}", $"HitStopDuration={HitStopDuration}", $"ScreenShakeIntensity={ScreenShakeIntensity}", $"ScreenShakeDuration={ScreenShakeDuration}", $"MissEffectName={MissEffectName}", $"MissSoundName={MissSoundName}" });
        }
    }
}
