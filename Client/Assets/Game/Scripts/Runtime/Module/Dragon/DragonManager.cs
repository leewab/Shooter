using Game.Core;
using GameConfig;
using ResKit;
using UnityEngine;

namespace Gameplay
{
    public class DragonManager : Singleton<DragonManager>
    {
        
        public DragonJoint GenerateBone(int id, int index)
        {
            var confDragonJoint = ConfDragonJoint.GetConf<ConfDragonJoint>(id);
            var bonePrefab = ResourceManager.Instance.Load<GameObject>(confDragonJoint.Prefab);
            var boneGameObject = GameObject.Instantiate(bonePrefab);
            if (boneGameObject != null)
            {
                var joint = boneGameObject.GetOrAddComponent<DragonJoint>();
                joint.InitDragonJoint(new DragonJointData()
                {
                    JointType = DragonJointType.Body,
                    ColorType = (ColorType)confDragonJoint.Type,
                    JointHealth = confDragonJoint.Health,
                    JointIndex = index,
                    JointId = id
                });
                return joint;
            }

            Debug.LogError("未加载到DragonJoint");
            return null;
        }
        
    }
}