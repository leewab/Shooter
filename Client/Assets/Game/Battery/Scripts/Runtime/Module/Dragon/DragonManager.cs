using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class DragonManager : Singleton<DragonManager>
    {

        #region Dragon Config

        private Dictionary<int, ConfDragon>  _dragonsConf = new Dictionary<int, ConfDragon>()
        {
            {
                0,
                new ConfDragon()
                {
                    NormalMoveSpeed = 5,
                    MaxMoveSpeed = 40f,
                    MaxSpeedDurationTime = 5,
                    DragonJointSpacing = 6f,
                    PositionSmoothness = 10f,
                    DragonJoints = new []{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                    DragonJointColors = new []{0, 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 2, 2, 0},
                }
            }
        };

        public ConfDragon GetDragonConf(int id)
        {
            return _dragonsConf[id];
        }

        #endregion

        #region DragonJoints Config

        private Dictionary<int, ConfDragonJoint> _dragonJointsConf = new Dictionary<int, ConfDragonJoint>()
        {
            {
                0,
                new ConfDragonJoint()
                {
                    Id = 0,
                    Health = 1,
                }
            }
        };

        public ConfDragonJoint GetDragonJointConf(int id)
        {
            return _dragonJointsConf[id];
        }

        #endregion
        
        private DragonJoint[] _attackDragonJoints;
        public DragonJoint[]  AttackDragonJoints
        {
            get => _attackDragonJoints;
            set => _attackDragonJoints = value;
        }

        public Action<bool> OnSuccessEvent;
        
        
        /// <summary>
        /// 查询最近的龙骨节点
        /// </summary>
        /// <returns></returns>
        public DragonJoint FindNearestMatchingJoint(ColorType colorType, Vector3 firePoint)
        {
            float minDistance = float.MaxValue;
            DragonJoint nearestJoint = null;

            var allJoints = AttackDragonJoints;
            foreach (var joint in allJoints)
            {
                if (!joint.IsAlive() || joint.GetColorType() != colorType) continue;
                if (joint.IsHead() || joint.IsTail()) continue;

                float distance = Vector2.Distance(firePoint, joint.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestJoint = joint;
                }
            }

            return nearestJoint;
        }

    }
}