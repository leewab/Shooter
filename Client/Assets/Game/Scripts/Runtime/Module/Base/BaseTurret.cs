using Unity.VisualScripting;
using UnityEngine;

namespace Gameplay
{
    public abstract class BaseTurret : PoolMonoObject
    {
        public abstract void SetupTurret(Transform parent);
    }
}