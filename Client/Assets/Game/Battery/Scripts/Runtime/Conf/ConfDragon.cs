namespace Gameplay
{
    public class ConfDragon
    {
        public int Id;
        public string DragonName;
        public string DragonJointPrefabName;
        public string DragonHeadPrefabName;
        public string DragonTailPrefabName;
        public float NormalMoveSpeed = 10;
        public float MaxMoveSpeed = 20f;
        public float MaxSpeedDurationTime = 2;
        public float DragonJointSpacing = 0.3f;
        public float PositionSmoothness = 10f;
        public int[] DragonJoints = new int[10];  // 关节Id分布
        public int[] DragonJointColors = new int[10];
    }

    public class ConfDragonJoint
    {
        public int Id;
        public int Health;
    }

}