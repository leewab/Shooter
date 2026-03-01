using DG.Tweening.Plugins.Core.PathCore;
using Game.Core;
using GameConfig;
using ResKit;
using UnityEngine;

namespace Gameplay
{
    public class DragonManager : Singleton<DragonManager>
    {
        
        public DragonJoint GenerateBone(int id, int index, DragonJointType type, Transform parent)
        {
            if (type == DragonJointType.Body)
            {
                var confDragonJoint = ConfDragonJoint.GetConf<ConfDragonJoint>(id);
                var bonePrefab = ResourceManager.Instance.Load<GameObject>(confDragonJoint.Prefab);
                var boneGameObject = GameObject.Instantiate(bonePrefab, parent);
                if (boneGameObject != null)
                {
                    boneGameObject.name = $"DragonBone_{index}";
                    var joint = boneGameObject.GetOrAddComponent<DragonJoint>();
                    joint.InitDragonJoint(new DragonJointData()
                    {
                        JointType = type,
                        ColorType = (ColorType)confDragonJoint.Type,
                        JointHealth = confDragonJoint.Health,
                        JointIndex = index,
                        JointId = id
                    });
                    return joint;
                } 
            }
            else
            {
                var prefabName = type == DragonJointType.Head ? PathDefine.DragonHeadPath : PathDefine.DragonTailPath;
                var bonePrefab = ResourceManager.Instance.Load<GameObject>(prefabName);
                var boneGameObject = GameObject.Instantiate(bonePrefab, parent);
                if (boneGameObject != null)
                {
                    boneGameObject.name = prefabName;
                    var joint = boneGameObject.GetOrAddComponent<DragonJoint>();
                    joint.InitDragonJoint(new DragonJointData()
                    {
                        JointType = type,
                        ColorType = ColorType.None,
                        JointHealth = 0,
                        JointIndex = index,
                        JointId = id
                    });
                    return joint;
                } 
            }

            Debug.LogError("未加载到DragonJoint");
            return null;
        }
        
    }
}