using UnityEngine;

namespace Gameplay
{
    public enum ColorType
    {
        Red,    // 攻击红色节点
        Green,  // 攻击绿色节点
        Blue,   // 攻击蓝色节点
        Yellow,
        Purple,
        Orange,
        End
    }

    public abstract class BaseTurret : MonoBehaviour
    {
        public abstract void SetTurret(TurretData td);
    }
}