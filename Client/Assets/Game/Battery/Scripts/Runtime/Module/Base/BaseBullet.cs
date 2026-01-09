using UnityEngine;

namespace Gameplay
{
    public abstract class BaseBullet : MonoBehaviour
    {
        public abstract void SetupBullet(int id, ColorType colorType, Vector2 direction);

    }
}