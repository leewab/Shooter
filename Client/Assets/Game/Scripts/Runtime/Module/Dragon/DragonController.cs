using System.Collections.Generic;
using Framework.UIFramework;
using GameConfig;
using Gameplay;
using GameUI;
using ResKit;
using UnityEngine;
using UnityEngine.Serialization;

public class DragonController : MonoBehaviour
{
    [FormerlySerializedAs("pathData")] [Header("Path Settings")]
    public PathPointData _PathData;
    
    [Header("Dragon Settings")] 
    public GameObject jointHead;
    public GameObject jointTail;
    public GameObject jointBody;
    
    private const string PathDataPath = "Product/Game/PathData/Game.asset";
    private const string DragonHeadPath = "Product/Game/Prefab/DragonHead.prefab";
    private const string DragonTailPath = "Product/Game/Prefab/DragonTail.prefab";
    private const string DragonBodyPath = "Product/Game/Prefab/DragonPart.prefab";
    
    // 所有关节
    private List<DragonJoint> allDragonJoints;
    // 记录被摧毁的关节索引
    private List<int> destroyedJoints;  
    // 所有节点距离
    private List<float> jointDistances;
    
    // 私有变量
    private ConfDragon _ConfDragon;
    private float tailDistance = 0f;
    private bool isMoving = false;
    private float colorTimer = 0f;
    private float _CurSpeed;
    private float _SpeedChangeTimer;

    void Start()
    {
        if (_PathData == null) _PathData = ResourceManager.Instance.Load<PathPointData>(PathDataPath);
        if (jointHead == null) jointHead = ResourceManager.Instance.Load<GameObject>(DragonHeadPath);
        if (jointTail == null) jointTail = ResourceManager.Instance.Load<GameObject>(DragonTailPath);
        if (jointBody == null) jointBody = ResourceManager.Instance.Load<GameObject>(DragonBodyPath);
    }
    
    private void Update()
    {
        if (!isMoving || _PathData == null || !_PathData.HasData()) return;

        UpdateSpeed();
        UpdateHeadPosition();
        UpdateJointsPosition();
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

    private void OnDestroy()
    {
        _PathData = null;
        ClearJoints();
    }

    // 初始化龙
    private void InitializeDragon()
    {
        if (_PathData == null) return;
        _ConfDragon = ConfDragon.GetConf<ConfDragon>(0);
        _CurSpeed = _ConfDragon.MaxMoveSpeed;
        _SpeedChangeTimer = 0f;
        ClearJoints();
        CreateJoints();
        InitializeJointDistances();
    }
    
    // 创建关节
    private void CreateJoints()
    {
        if (_ConfDragon == null) return;
        int[] dragonJoints = _ConfDragon.DragonJoints;
        int[] dragonJointColors = _ConfDragon.DragonJointColors;
        if (dragonJoints == null || dragonJointColors == null) return;
        int dragonJointNum = dragonJoints.Length;
        if (dragonJointNum != dragonJointColors.Length)
        {
            Debug.LogError("龙关节数量和颜色数量不匹配");
            return;
        }

        int totalJoints = dragonJointNum + 2;

        if (destroyedJoints == null) destroyedJoints = new List<int>(totalJoints);
        destroyedJoints.Clear();

        if (jointDistances == null) jointDistances = new List<float>(totalJoints);
        jointDistances.Clear();
        for (int i = 0; i < totalJoints; i++)
        {
            jointDistances.Add(0f);
        }
        
        int index = 0;
        if (allDragonJoints == null) allDragonJoints = new List<DragonJoint>(totalJoints);
        allDragonJoints.Clear();
        allDragonJoints.Add(CreateJoint(jointTail, DragonJointType.Tail, ColorType.None, null, index++));
        allDragonJoints.AddRange(CreateBody(dragonJointNum, dragonJointColors, dragonJoints, ref index));
        allDragonJoints.Add(CreateJoint(jointHead, DragonJointType.Head, ColorType.None, null, index++));
        DragonManager.Instance.AttackDragonJoints = allDragonJoints;
    }

    private DragonJoint CreateJoint(GameObject prefab, DragonJointType type, ColorType colorType, ConfDragonJoint conf, int index)
    {
        GameObject jointObj = prefab != null ? Instantiate(prefab, transform) : new GameObject($"DragonJoint_{type}_{index}");
        DragonJoint joint = jointObj.GetComponent<DragonJoint>() ?? jointObj.AddComponent<DragonJoint>();

        if (type == DragonJointType.Body)
        {
            joint.onDestroyed.RemoveListener(OnJointDestroyed);
            joint.onDestroyed.AddListener(OnJointDestroyed);
        }

        joint.SetData(new DragonJointData()
        {
            JointType = type,
            ColorType = colorType,
            ConfDragonJoint = conf,
            JointIndex = index,
        });

        return joint;
    }

    private List<DragonJoint> CreateBody(int jointNum, int[] colorTypes, int[] jointIds, ref int index)
    {
        List<DragonJoint> bodyJoints = new List<DragonJoint>(jointNum);

        for (int i = 0; i < jointNum; i++)
        {
            ColorType colorType = (ColorType)colorTypes[i];
            ConfDragonJoint conf = ConfDragonJoint.GetConf<ConfDragonJoint>(jointIds[i]);
            bodyJoints.Add(CreateJoint(jointBody, DragonJointType.Body, colorType, conf, index++));
        }

        return bodyJoints;
    }
    
    // 清理关节
    private void ClearJoints()
    {
        if (allDragonJoints != null)
        {
            foreach (var joint in allDragonJoints)
            {
                if (joint != null && joint.gameObject != null)
                {
                    if (Application.isPlaying)
                        Destroy(joint.gameObject);
                    else
                        DestroyImmediate(joint.gameObject);
                }
            }
            allDragonJoints.Clear();
        }

        if (jointDistances != null)
        {
            jointDistances.Clear();
            jointDistances = null;
        }

        if (destroyedJoints != null)
        {
            destroyedJoints.Clear();
            destroyedJoints = null;
        }
    }
    
    // 初始化关节距离
    private void InitializeJointDistances()
    {
        tailDistance = -allDragonJoints.Count * _ConfDragon.DragonJointSpacing;
        // 初始化所有关节的距离
        for (int i = 0; i < allDragonJoints.Count; i++)
        {
            jointDistances[i] = Mathf.Max(0, tailDistance + i * _ConfDragon.DragonJointSpacing);
        }

        // 设置初始位置到远处
        for (int i = 0; i < allDragonJoints.Count; i++)
        {
            var result = _PathData.GetPositionRotationScaleAtDistance(jointDistances[i]);
            allDragonJoints[i].transform.position = result.position;
            allDragonJoints[i].transform.rotation = result.rotation;
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
        if (allDragonJoints == null) return;
        
        for (int i = 0; i < allDragonJoints.Count; i++)
        {
            DragonJoint joint = allDragonJoints[i];
            if (joint == null) continue;
            if (!joint.gameObject.activeSelf) joint.gameObject.SetActive(true);
            // 计算目标距离
            float targetDistance = tailDistance + i * _ConfDragon.DragonJointSpacing;
            
            targetDistance = Mathf.Max(0, targetDistance);
            jointDistances[i] = Mathf.Lerp(jointDistances[i], targetDistance, _ConfDragon.PositionSmoothness * Time.deltaTime);
            
            // 获取路径位置
            var result = _PathData.GetPositionRotationScaleAtDistance(jointDistances[i]);
            
            // 到终点游戏结束
            if (joint.IsHead() && targetDistance >= _PathData.totalLength - 0.1f)
            {
                OnGameOver();
                return;
            }
            
            // 更新位置
            joint.transform.position = Vector3.Lerp(joint.transform.position, result.position, _ConfDragon.PositionSmoothness * Time.deltaTime);
            joint.transform.rotation = result.rotation;
        }
        
        if (allDragonJoints.Count <= 2)
        {
            OnGameSuccess();
        }
    }
    
    // 关节被摧毁时的回调
    private void OnJointDestroyed(int jointIndex)
    {
        int removeIndex = -1;
        for (int i = 0; i < allDragonJoints.Count; i++)
        {
            if (allDragonJoints[i].JointIndex == jointIndex)
            {
                removeIndex = i;
                break;
            }
        }

        if (removeIndex >= 0)
        {
            allDragonJoints.RemoveAt(removeIndex);
            jointDistances.RemoveAt(removeIndex);
        }

        UpdateJointsPosition();
    }

    private void OnGameOver()
    {
        Debug.Log("GameOver");
        DragonManager.Instance.OnSuccessEvent?.Invoke(false);
        UIManager.Open<UIGameResultPanel>("GameOverPanel");
        StopMoving();
    }

    private void OnGameSuccess()
    {
        Debug.Log("OnGameSuccess");
        DragonManager.Instance.OnSuccessEvent?.Invoke(true);
        UIManager.Open<UIGameResultPanel>("OnGameSuccess");
        StopMoving();
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
    
    public void StartMoving() => isMoving = true;
    public void StopMoving() => isMoving = false;
    public void SetSpeed(float speed) => _CurSpeed = speed;

    #endregion
 
}