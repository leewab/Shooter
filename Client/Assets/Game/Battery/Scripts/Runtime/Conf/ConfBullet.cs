namespace Gameplay
{
    public class ConfBullet
    {
        public int Damage = 1; // 伤害
        public float Speed = 220f; // 速度
        public float MaxTravelDistance = 200f; // 最大飞行距离

        // 视觉效果配置
        public float StartScale = 1.5f; // 发射时的初始缩放
        public float ScaleDuration = 0.2f; // 缩放到正常大小的持续时间

        // 命中效果配置
        public string HitEffectName = "BulletHit"; // 命中特效名称
        public float HitEffectDuration = 0.5f; // 命中特效持续时间
        public string HitSoundName = "BulletHit"; // 命中音效名称
        public float HitStopDuration = 0.02f; // 命中顿帧时长（秒）
        public float ScreenShakeIntensity = 0.1f; // 屏幕震动强度
        public float ScreenShakeDuration = 0.1f; // 屏幕震动持续时间

        // 兼容旧配置
        public string EffectName = "Bullet";
        public string AudioName = "Bullet";
    }

}