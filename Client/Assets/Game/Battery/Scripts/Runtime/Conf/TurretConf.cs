namespace Gameplay
{
    public class TurretConf
    {
        public int Id;
        public ColorType ColorType = ColorType.Red;
        public float AttackCooldown = 1f;            // 攻击冷却时间
        public int DamagePerShot = 1;                // 每次攻击伤害
        public int MaxHitNum = 20;                   // 最大攻击数量
        public float BulletSpeed = 10f;              // 子弹速度
        public string FireSound = "Fire";
        public string BulletName = "Bullet";
        public int BulletId = 0;

        // 发射效果配置
        public float RecoilDistance = 0.1f;          // 后坐力后退距离
        public float RecoilDuration = 0.15f;         // 后坐力复位时间
        public float RecoilRotation = 5f;            // 后坐力旋转角度（枪口上抬）
        public float MuzzleFlashDuration = 0.05f;    // 炮口闪光持续时间
        public string MuzzleEffectName = "MuzzleFlash"; // 炮口特效名称
        public float MuzzleEffectScale = 1f;         // 炮口特效缩放
    }
}