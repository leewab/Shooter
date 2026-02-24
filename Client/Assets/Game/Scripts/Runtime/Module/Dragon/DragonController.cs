using System;
using System.Collections.Generic;
using Framework.UIFramework;
using GameConfig;
using Gameplay;
using GameUI;
using ResKit;
using UnityEngine;

public class DragonController : SingletonMono<DragonController>
{
    public Action<bool> OnSuccessEvent;
        
    private PathPointData _PathData;
    private GameObject jointHead;
    private GameObject jointTail;
    private GameObject jointBody;
    
    // 所有关节
    private List<DragonJoint> _DragonBones;
    // 所有节点距离
    private List<float> _BonesDistances;
    // 所有的龙骨信息
    private List<int> _DragonBonesInfos;
    public List<int> DragonBonesInfos => _DragonBonesInfos;

    // 私有变量
    private ConfDragon _ConfDragon;
    private float tailDistance = 0f;
    private bool isMoving = false;
    private float colorTimer = 0f;
    private float _CurSpeed;
    private float _SpeedChangeTimer;
    private int _CurLevelID;

    private void Start()
    {
        if (_PathData == null) _PathData = ResourceManager.Instance.Load<PathPointData>(PathDefine.PathDataPath);
        if (jointHead == null) jointHead = ResourceManager.Instance.Load<GameObject>(PathDefine.DragonHeadPath);
        if (jointTail == null) jointTail = ResourceManager.Instance.Load<GameObject>(PathDefine.DragonTailPath);
        if (jointBody == null) jointBody = ResourceManager.Instance.Load<GameObject>(PathDefine.DragonBodyPath);
    }
    
    private void Update()
    {
        if (!isMoving || _PathData == null || !_PathData.HasData()) return;
        UpdateSpeed();
        UpdateHeadPosition();
        UpdateJointsPosition();
        // 判断游戏结果
        if (_DragonBones.Count <= 0)
        {
            OnGameSuccess();
        }
    }
    
    private void OnDestroy()
    {
        _PathData = null;
        ClearDragon();
    }

    private void UpdateSpeed()
    {
        if (_SpeedChangeTimer < _ConfDragon.MaxSpeedDurationTime)
        {
            _SpeedChangeTimer += Time.deltaTime;
            float t = _SpeedChangeTimer / _ConfDragon.MaxSpeedDurationTime;
            _CurSpeed = Mathf.Lerp(_ConfDragon.MaxMoveSpeed, _ConfDragon.NormalMoveSpeed, t);
        }
        else
        {
            _CurSpeed = _ConfDragon.NormalMoveSpeed;
        }
    }

    // 初始化龙
    private void InitializeDragon()
    {
        if (_PathData == null) return;
        _CurLevelID = LevelManager.Instance.GetCurrentLevel();
        _ConfDragon = ConfDragon.GetConf<ConfDragon>(_CurLevelID);
        _CurSpeed = _ConfDragon.MaxMoveSpeed;
        _SpeedChangeTimer = 0f;
        InitDragonBones();
        ClearBones();
        CreateBones();
        InitializeJointDistances();
    }

    // 初始化龙骨信息
    private void InitDragonBones()
    {
        if (_ConfDragon == null) return;
        int[] dragonJoints = _ConfDragon.DragonJoints;
        int dragonJointNum = dragonJoints.Length;
        _DragonBonesInfos = new List<int>(dragonJointNum);
        for (int i = 0; i < dragonJoints.Length; i++)
        {
            _DragonBonesInfos.Add(dragonJoints[i]);
        }
    }
    
    // 创建关节
    private void CreateBones()
    {
        int totalJoints = _DragonBonesInfos.Count + 2;

        if (_BonesDistances == null) _BonesDistances = new List<float>(totalJoints);
        _BonesDistances.Clear();
        
        if (_DragonBones == null) _DragonBones = new List<DragonJoint>(totalJoints);
        _DragonBones.Clear();
        
        for (int i = 0; i < totalJoints; i++)
        {
            _BonesDistances.Add(0f);
        }
        
        int index = 0;
        // _DragonBones.Add(GenerateBones(jointTail, DragonJointType.Tail, ColorType.None, 0, index++));
        _DragonBones.AddRange(CreateBody(ref index));
        // _DragonBones.Add(GenerateBones(jointHead, DragonJointType.Head, ColorType.None, 0, index++));
    }

    private List<DragonJoint> CreateBody(ref int index)
    {
        int dragonBoneNum = _DragonBonesInfos.Count;
        List<DragonJoint> bodyJoints = new List<DragonJoint>(dragonBoneNum);
        for (int i = 0; i < dragonBoneNum; i++)
        {
            bodyJoints.Add(GenerateBones(_DragonBonesInfos[i], DragonJointType.Body, index++));
        }

        return bodyJoints;
    }
    
    private DragonJoint GenerateBones(int dragonId, DragonJointType type, int index)
    {
        DragonJoint joint = DragonManager.Instance.GenerateBone(dragonId, index);

        if (type == DragonJointType.Body)
        {
            joint.OnDestroyed -= OnJointDestroyed;
            joint.OnDestroyed += OnJointDestroyed;
        }

        return joint;
    }
    
    // 清理关节
    private void ClearBones()
    {
        if (_DragonBones != null)
        {
            foreach (var joint in _DragonBones)
            {
                if (joint != null && joint.gameObject != null)
                {
                    if (Application.isPlaying)
                        Destroy(joint.gameObject);
                    else
                        DestroyImmediate(joint.gameObject);
                }
            }
            _DragonBones.Clear();
        }

        if (_BonesDistances != null)
        {
            _BonesDistances.Clear();
            _BonesDistances = null;
        }
    }
    
    // 初始化关节距离
    private void InitializeJointDistances()
    {
        tailDistance = -_DragonBones.Count * _ConfDragon.DragonJointSpacing;
        
        // 初始化所有关节的距离
        for (int i = 0; i < _BonesDistances.Count; i++)
        {
            _BonesDistances[i] = Mathf.Max(0, tailDistance + i * _ConfDragon.DragonJointSpacing);
        }

        // 设置初始位置到远处
        for (int i = 0; i < _DragonBones.Count; i++)
        {
            var result = _PathData.GetPositionRotationScaleAtDistance(_BonesDistances[i]);
            _DragonBones[i].transform.position = result.position;
            _DragonBones[i].transform.rotation = result.rotation;
        }
    }
    
    // 更新头部位置
    private void UpdateHeadPosition()
    {
        tailDistance += _CurSpeed * Time.deltaTime;
        tailDistance = Mathf.Min(tailDistance, _PathData.totalLength);
    }
    
    // 更新关节位置
    private void UpdateJointsPosition()
    {
        if (_DragonBones == null) return;
        
        for (int i = 0; i < _DragonBones.Count; i++)
        {
            DragonJoint joint = _DragonBones[i];
            if (joint == null) continue;
            if (!joint.gameObject.activeSelf)
            {
                joint.gameObject.SetActive(true);
                joint.ActiveAlive();
            }
            // 计算目标距离
            float targetDistance = tailDistance + i * _ConfDragon.DragonJointSpacing;
            
            targetDistance = Mathf.Max(0, targetDistance);
            _BonesDistances[i] = Mathf.Lerp(_BonesDistances[i], targetDistance, _ConfDragon.PositionSmoothness * Time.deltaTime);
            
            // 获取路径位置
            var result = _PathData.GetPositionRotationScaleAtDistance(_BonesDistances[i]);
            
            // 到终点游戏结束
            if (joint.IsHead() && targetDistance >= _PathData.totalLength - 0.1f)
            {
                OnGameOver();
                return;
            }
            
            // 更新位置
            joint.transform.position = Vector3.Lerp(joint.transform.position, result.position, _ConfDragon.PositionSmoothness * Time.deltaTime);
            if (joint.IsHead() || joint.IsTail())
            {
                joint.transform.rotation = result.rotation;
            }
        }
    }
    
    // 关节被摧毁时的回调
    private void OnJointDestroyed(int bonesIndex)
    {
        int removeIndex = -1;
        for (int i = 0; i < _DragonBones.Count; i++)
        {
            if (_DragonBones[i].BonesIndex == bonesIndex)
            {
                removeIndex = i;
                break;
            }
        }

        if (removeIndex >= 0)
        {
            _DragonBones.RemoveAt(removeIndex);
            _BonesDistances.RemoveAt(removeIndex);
        }

        UpdateJointsPosition();
    }

    private void OnGameOver()
    {
        Debug.Log("GameOver");
        OnSuccessEvent?.Invoke(false);
        UIManager.Open<UIGameFailedPanel>();
        StopMoving();
        ClearDragon();
    }

    private void OnGameSuccess()
    {
        Debug.Log("OnGameSuccess");
        OnSuccessEvent?.Invoke(true);
        UIManager.Open<UIGameSuccessPanel>();
        StopMoving();
        ClearDragon();
    }


    #region 公共控制方法
    
    public void InitDragon()
    {
        InitializeDragon();
    }

    public void ResetDragon()
    {
        StopMoving();
        InitializeDragon();
    }

    public void ClearDragon()
    {
        ClearBones();
        if (_DragonBonesInfos != null)
        {
            _DragonBonesInfos.Clear();
        }
    }
    
    public void StartMoving() => isMoving = true;
    public void StopMoving() => isMoving = false;
    public void SetSpeed(float speed) => _CurSpeed = speed;

    #endregion

    #region 公共数据

    /// <summary>
    /// 查询最近的龙骨节点（不考虑遮挡）
    /// </summary>
    /// <returns></returns>
    public List<(float, DragonJoint)> FindNearestMatchingJoint(ColorType colorType, Vector3 firePoint)
    {
        if (_DragonBones == null) return null;
        float minDistance = float.MaxValue;
        var allJoints = _DragonBones;
        List<(float, DragonJoint)> nearestJoints = new List<(float, DragonJoint)>(allJoints.Count / 3);
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

    #endregion
 
}