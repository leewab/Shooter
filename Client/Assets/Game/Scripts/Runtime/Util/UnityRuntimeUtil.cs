using UnityEngine;

namespace Game.Core
{
    public static class UnityRuntimeUtil
    {
        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            if (obj == null) return null;
            var comp = KeepOneComponent<T>(obj);
            if (comp == null) comp = obj.AddComponent<T>();
            return comp;
        }

        public static T KeepOneComponent<T>(this GameObject obj) where T : Component
        {
            var comps = obj.GetComponents<T>();
            if (comps == null || comps.Length == 0) return null;
            if (comps.Length > 1)
            {
                for (int i = 1; i < comps.Length; i++)
                {
                    if (comps[i]) Object.DestroyImmediate(comps[i]);
                }
            }

            return comps[0];
        }

    }

}