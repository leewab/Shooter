using UnityEngine;

namespace Gameplay
{
    public enum ColorType
    {
        Red    = 0,   
        Green  = 1,   
        Blue   = 2,   
        Yellow = 3,
        Purple = 4,
        Orange = 5,
        None
    }

    public abstract class BaseTurret : PoolMonoObject
    {
        public abstract void SetupTurret(Transform parent);
    }
}