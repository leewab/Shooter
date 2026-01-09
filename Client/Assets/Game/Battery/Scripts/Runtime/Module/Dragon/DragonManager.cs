using System.Collections.Generic;

namespace Gameplay
{
    public class DragonManager : Singleton<DragonManager>
    {
        private Dictionary<int, DragonConf>  _dragonsConf = new Dictionary<int, DragonConf>()
        {
            {
                0, 
                new DragonConf()
                {
                    NormalMoveSpeed = 5,
                    MaxMoveSpeed = 25f,
                    MasSpeedTime = 5,
                    JointSpacing = 6f,
                    MaxJoints = 20,
                    PositionSmoothness = 10f,
                    RealignSpeed = 5f,  // 重新对齐的速度
                }
            }
        };

        public DragonConf GetDragonConf(int id)
        {
            return _dragonsConf[id];
        }
        
        public ColorType GetDefaultColor(int id)
        {
            int num = (int)ColorType.End - 1;
            int idx = id % num;
            return (ColorType)idx;
        }
        
    }
}