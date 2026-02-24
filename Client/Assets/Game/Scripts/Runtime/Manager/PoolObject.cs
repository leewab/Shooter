using UnityEngine;

namespace Gameplay
{
    public abstract class PoolObject
    {
        public abstract void Init();
        public abstract void Recycle();
        public abstract void Destroy();
    }

    public abstract class PoolMonoObject : MonoBehaviour
    {
        public string PrefabPath { get; set; }
        public abstract void Init(params object[] parameters);
        public abstract void Recycle();
        public abstract void Destroy();
    }
}