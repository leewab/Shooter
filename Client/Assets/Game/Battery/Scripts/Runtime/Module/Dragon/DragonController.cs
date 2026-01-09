using System.Collections;
using System.Collections.Generic;
using Gameplay;
using UnityEngine;
using UnityEngine.Events;

public class DragonController : MonoBehaviour
{
    [Header("Path Settings")]
    public PathPointData pathData;
    
    [Header("Dragon Settings")] 
    public GameObject jointHead;
    public GameObject jointTail;
    public GameObject jointPrefab;
    public bool autoStart = true;
    
    // 私有变量
    private DragonConf _DragonConf;
    private List<DragonJoint> joints = new List<DragonJoint>();
    private List<float> jointDistances = new List<float>();
    private float tailDistance = 0f;
    private bool isMoving = false;
    private float colorTimer = 0f;
    private float _curSpeed;
    
    // 重新对齐相关
    private bool isRealigning = false;
    private Coroutine realignCoroutine;
    private List<int> destroyedJoints = new List<int>();  // 记录被摧毁的关节索引
    
    private void Start()
    {
        InitializeDragon();
        
        if (autoStart)
        {
            StartMoving();
        }
    }
    
    private void Update()
    {
        if (!isMoving || pathData == null || !pathData.HasData()) return;
        
        if (!isRealigning)
        {
            UpdateHeadPosition();
            UpdateJointsPosition();
        }
    }
    
    // 初始化龙
    public void InitializeDragon()
    {
        if (pathData == null) return;
        if (_DragonConf == null) _DragonConf = DragonManager.Instance.GetDragonConf(0);
        _curSpeed = _DragonConf.MaxMoveSpeed;
        ClearJoints();  // 先清理旧的关节
        CreateJoints();
        InitializeJointDistances();
        Invoke(nameof(DelayChangeSpeed), _DragonConf.MasSpeedTime);
    }

    private void DelayChangeSpeed()
    {
        _curSpeed = _DragonConf.NormalMoveSpeed;
    }
    
    // 清理关节
    private void ClearJoints()
    {
        foreach (var joint in joints)
        {
            if (joint != null && joint.gameObject != null)
            {
                if (Application.isPlaying)
                    Destroy(joint.gameObject);
                else
                    DestroyImmediate(joint.gameObject);
            }
        }
        
        joints.Clear();
        jointDistances.Clear();
        destroyedJoints.Clear();
    }
    
    // 创建关节
    private void CreateJoints()
    {
        if (_DragonConf == null) return;
        for (int i = 0; i < _DragonConf.MaxJoints; i++)
        {
            GameObject jointObj = null;
            if (i == 0)
            {
                jointObj = jointTail != null ? Instantiate(jointTail, transform) : new GameObject($"Head_{i}");
                jointObj.name = $"DragonTail_{i}";
            }
            else if (i == _DragonConf.MaxJoints - 1)
            {
                jointObj = jointHead != null ? Instantiate(jointHead, transform) : new GameObject($"Tail_{i}");
                jointObj.name = $"DragonHead_{i}";
            }
            else
            {
                jointObj = jointPrefab != null ? Instantiate(jointPrefab, transform) : new GameObject($"Joint_{i}");
                jointObj.name = $"DragonJoint_{i}";
            }
            jointObj.SetActive(false);
            
            // 添加或获取关节组件
            DragonJoint joint = jointObj.GetComponent<DragonJoint>();
            if (joint == null) joint = jointObj.AddComponent<DragonJoint>();
            
            ColorType colorType = DragonManager.Instance.GetDefaultColor(i);
            joint.onDestroyed.RemoveListener(OnJointDestroyed);
            joint.onDestroyed.AddListener(OnJointDestroyed);
            joint.SetData(new DragonJointData()
            {
                JointType = (i == 0) ? DragonJointType.Tail : ((i == _DragonConf.MaxJoints - 1) ? DragonJointType.Head : DragonJointType.Body),
                ColorType = colorType,
                MaxHealth = _DragonConf.MaxJointHealth,
                JointIndex = i
            });
            
            joints.Add(joint);
            jointDistances.Add(0f);
        }
    }
    
    // 初始化关节距离
    private void InitializeJointDistances()
    {
        tailDistance = -joints.Count * _DragonConf.JointSpacing;
        // 初始化所有关节的距离
        for (int i = 0; i < joints.Count; i++)
        {
            jointDistances[i] = Mathf.Max(0, tailDistance + i * _DragonConf.JointSpacing);
        }
    }
    
    // 更新头部位置
    private void UpdateHeadPosition()
    {
        tailDistance += _curSpeed * Time.deltaTime;
        tailDistance = Mathf.Min(tailDistance, pathData.totalLength);
    }
    
    // 更新关节位置
    private void UpdateJointsPosition()
    {
        if (joints.Count == 2)
        {
            StopMoving();
            return;
        }
        
        for (int i = 0; i < joints.Count; i++)
        {
            DragonJoint joint = joints[i];
            if (joint == null || !joint.IsAlive()) continue;
            if (!joint.gameObject.activeSelf) joint.gameObject.SetActive(true);
            // 计算目标距离
            float targetDistance = tailDistance + i * _DragonConf.JointSpacing;
            
            targetDistance = Mathf.Max(0, targetDistance);
            jointDistances[i] = Mathf.Lerp(jointDistances[i], targetDistance, _DragonConf.PositionSmoothness * Time.deltaTime);
            
            // 获取路径位置
            var result = pathData.GetPositionRotationScaleAtDistance(jointDistances[i]);
            
            // 更新位置
            joint.transform.position = Vector3.Lerp(joint.transform.position, result.position, _DragonConf.PositionSmoothness * Time.deltaTime);
            joint.transform.rotation = result.rotation;
        }
    }
    
    // 关节被摧毁时的回调
    public void OnJointDestroyed(int jointIndex)
    {
        Debug.Log($"Joint {jointIndex} destroyed");
        for (int i = 0; i < joints.Count; i++)
        {
            if (joints[i].JointIndex == jointIndex)
            {
                joints.Remove(joints[i]);
            }
        }

        UpdateJointsPosition();
    }
    
    // 公共控制方法
    public void StartMoving() => isMoving = true;
    public void StopMoving() => isMoving = false;
    public void SetSpeed(float speed) => _DragonConf.NormalMoveSpeed = speed;
    
    public void ResetDragon()
    {
        tailDistance = 0f;
        InitializeDragon();
    }
    
    // 获取存活的关节数量
    public int GetAliveJointCount()
    {
        int count = 0;
        foreach (var joint in joints)
        {
            if (joint != null && joint.IsAlive()) count++;
        }
        return count;
    }
    
    // 获取总关节数量
    public int GetTotalJointCount()
    {
        return joints.Count;
    }
    
    // 是否正在重新对齐
    public bool IsRealigning()
    {
        return isRealigning;
    }
    
    // 调试信息
    public void PrintJointsInfo()
    {
        Debug.Log($"Total joints: {joints.Count}, Alive: {GetAliveJointCount()}");
        for (int i = 0; i < joints.Count; i++)
        {
            if (joints[i] != null)
            {
                Debug.Log($"Joint {i}: Index={joints[i].JointIndex}, Alive={joints[i].IsAlive()}, Distance={jointDistances[i]:F2}");
            }
        }
    }
}