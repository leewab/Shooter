using System;
using System.Collections.Generic;
using GameConfig;
using UnityEngine;

namespace Gameplay
{
    public class DragonManager : Singleton<DragonManager>
    {
        public List<DragonJoint> AttackDragonJoints;
        public Action<bool> OnSuccessEvent;
        
        /// <summary>
        /// 查询最近的龙骨节点（不考虑遮挡）
        /// </summary>
        /// <returns></returns>
        public List<(float, DragonJoint)> FindNearestMatchingJoint(ColorType colorType, Vector3 firePoint)
        {
            if (AttackDragonJoints == null) return null;
            float minDistance = float.MaxValue;
            var allJoints = AttackDragonJoints;
            List<(float, DragonJoint)> nearestJoints = new List<(float, DragonJoint)>(allJoints.Count/3);
            foreach (var joint in allJoints)
            {
                if (!joint.IsAlive() || joint.GetColorType() != colorType) continue;
                if (joint.IsHead() || joint.IsTail()) continue;

                float distance = Vector2.Distance(firePoint, joint.transform.position);
                nearestJoints.Add((distance, joint));
            }
            
            return nearestJoints;
        }

        /// <summary>
        /// 查询最近的且未被遮挡的龙骨节点（性能优化版本）
        /// </summary>
        /// <param name="colorType">炮台颜色类型</param>
        /// <param name="firePoint">发射点位置</param>
        /// <returns>最近的未被遮挡的龙骨节点，如果被遮挡则返回null</returns>
        public DragonJoint FindNearestUnblockedMatchingJoint(ColorType colorType, Vector3 firePoint)
        {
            // 步骤1：找到最近的目标（无射线检测）
            List<(float, DragonJoint)> nearestJoints = FindNearestMatchingJoint(colorType, firePoint);
            if (nearestJoints == null || nearestJoints.Count <= 0) return null;
            nearestJoints.Sort((item1, item2) => item1.Item1 < item2.Item1 ? -1 : 1);
            
            foreach (var nearestJointKV in nearestJoints)
            {
                var distance = nearestJointKV.Item1;
                var nearestJoint = nearestJointKV.Item2;
                Vector2 direction = (nearestJoint.transform.position - firePoint).normalized;

                bool isHit = Physics.Raycast(firePoint, direction, out var hit, distance, LayerMask.GetMask("Game"));
                if (isHit && hit.collider != null)
                {
                    DragonJoint hitJoint = hit.collider.GetComponent<DragonJoint>();
                    // 首个碰撞就是该目标 → 未被遮挡
                    if (hitJoint == nearestJoint)
                    {
                        return nearestJoint;
                    }
                }
            }

            return null;
        }

    }
}