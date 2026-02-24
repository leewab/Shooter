using System;
using GameConfig;
using UnityEngine;
using Gameplay;
using UnityEngine.UI;

public enum DragonJointType
{
    Head,
    Tail,
    Body,
}

public class DragonJointData
{
    public DragonJointType JointType;
    public ColorType ColorType;
    public int JointHealth;
    public int JointIndex;
    public int JointId;
}

public class DragonJoint : MonoBehaviour
{
    public int BonesIndex;
    public Action<int> OnDestroyed;
    public Action<float> OnHealthChanged;

    // 组件
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private MeshRenderer meshRender;
    [SerializeField] private Text txtHUD;

    // 旋转滚动参数
    [Header("旋转滚动参数")]
    [SerializeField] private bool enableRolling = true;
    [SerializeField] private float rollingSpeed = 1.0f;
    [SerializeField] private bool autoCalculateAxis = true;
    [SerializeField] private Vector3 rotationAxis = Vector3.forward;
    [SerializeField] private float rotationOffset = 0f;
    
    // 私有变量
    private float _CurrentHealth = 0;
    private Color _currentColor;
    private ColorType _colorType;
    private bool _isAlive = true;
    private DragonJointData _BoneData;
    private ConfDragonJoint _ConfigBone;
    
    // 旋转相关
    private Vector3 _lastPosition;
    private float _totalRotation = 0f;
    
    private void Awake()
    {
        gameObject.tag = "DragonJoint";
        gameObject.layer = LayerMask.NameToLayer("Game");
        _lastPosition = transform.position;
    }
    
    private void Update()
    {
        if (!_isAlive || !enableRolling) return;
        
        //UpdateRollingRotation();
    }
    
    private void UpdateRollingRotation()
    {
        Vector3 currentPosition = transform.position;
        Vector3 movementDelta = currentPosition - _lastPosition;
        float movementDistance = movementDelta.magnitude;
        
        if (movementDistance > 0.001f)
        {
            Vector3 currentRotationAxis;
            
            if (autoCalculateAxis)
            {
                Vector3 movementDirection = movementDelta.normalized;
                currentRotationAxis = Vector3.Cross(Vector3.forward, movementDirection);
                if (currentRotationAxis.magnitude < 0.01f)
                {
                    currentRotationAxis = Vector3.forward;
                }
            }
            else
            {
                currentRotationAxis = rotationAxis;
            }
            
            float rotationAngle = movementDistance * rollingSpeed * Mathf.Rad2Deg;
            _totalRotation += rotationAngle;
            
            Quaternion rotation = Quaternion.AngleAxis(rotationAngle, currentRotationAxis);
            transform.rotation = rotation * transform.rotation;
            
            _lastPosition = currentPosition;
        }
    }
    
    // 设置颜色类型
    private void InitColorType(ColorType newType)
    {
        _colorType = newType;
        _currentColor = TurretManager.Instance.GetColor(_colorType);
        if (spriteRenderer != null)
        {
            spriteRenderer.color = _currentColor;
        }

        if (meshRender != null)
        {
            // 在代码中动态修改主颜色
            meshRender.material.SetColor("_Color", _currentColor); // 设置红色主色调
        }
    }

    private void UpdateHUD()
    {
        if (txtHUD)
        {
            txtHUD.text = _CurrentHealth.ToString();
        }
    }
    
    // 摧毁关节
    private void DestroyJoint()
    {
        if (!_isAlive) return;
        
        _isAlive = false;
        
        if (string.IsNullOrEmpty(_ConfigBone.DestroyAudio))
        {
            AudioManager.Instance.PlaySound(_ConfigBone.DestroyAudio);
        }
        
        if (string.IsNullOrEmpty(_ConfigBone.DestroyEffect))
        {
            EffectManager.Instance.InstantiateEffect(_ConfigBone.DestroyEffect, transform.position,
                Quaternion.identity);
        }
        
        OnDestroyed?.Invoke(BonesIndex);
        
        // 稍微延迟销毁，确保控制器有足够时间处理
        Invoke(nameof(ActuallyDestroy), 0.05f);
    }
    
    private void ActuallyDestroy()
    {
        _BoneData = null;
        _ConfigBone = null;
        OnDestroyed = null;
        OnHealthChanged = null;
        Destroy(gameObject);
    }
    
    // 受到伤害
    public void TakeDamage(int damage = 1)
    {
        if (!_isAlive) return;
        
        _CurrentHealth -= damage;
        OnHealthChanged?.Invoke(_CurrentHealth);

        UpdateHUD();

        if (string.IsNullOrEmpty(_ConfigBone.DamageAudio))
        {
            AudioManager.Instance.PlaySound(_ConfigBone.DamageAudio);
        }
        
        if (string.IsNullOrEmpty(_ConfigBone.DamageEffect))
        {
            EffectManager.Instance.InstantiateEffect(_ConfigBone.DamageEffect, transform.position,
                Quaternion.identity);
        }
        
        if (_CurrentHealth <= 0)
        {
            DestroyJoint();
        }
    }
    
    // 获取颜色类型
    public ColorType GetColorType()
    {
        return _colorType;
    }
    
    // 是否存活
    public bool IsAlive()
    {
        return _CurrentHealth > 0;
    }
    
    // 是否为头部
    public bool IsHead()
    {
        return _BoneData.JointType == DragonJointType.Head;
    }

    // 是否为尾部
    public bool IsTail()
    {
        return _BoneData.JointType == DragonJointType.Tail;
    }

    public bool IsBody()
    {
        return _BoneData.JointType == DragonJointType.Body;
    }
    
    public void ActiveAlive()
    {
        _isAlive = true;
    }
    
    public void InitDragonJoint(DragonJointData jointData)
    {
        gameObject.SetActive(false);
        _isAlive = false;
        _BoneData = jointData;
        BonesIndex = jointData.JointIndex;
        _CurrentHealth = jointData.JointHealth;
        _ConfigBone = ConfDragonJoint.GetConf<ConfDragonJoint>(jointData.JointId);
        InitColorType(jointData.ColorType);
        UpdateHUD();

        _lastPosition = transform.position;
        _totalRotation = 0f;
    }
    
    // 设置旋转滚动参数
    public void SetRollingParameters(bool enable, float speed, Vector3 axis, bool autoCalcAxis = true)
    {
        enableRolling = enable;
        rollingSpeed = speed;
        rotationAxis = axis.normalized;
        autoCalculateAxis = autoCalcAxis;
    }
    
    public void SetRollingEnabled(bool enable)
    {
        enableRolling = enable;
    }
    
    public void SetRollingSpeed(float speed)
    {
        rollingSpeed = speed;
    }
    
    public void SetRotationAxis(Vector3 axis)
    {
        rotationAxis = axis.normalized;
    }
    
    public void SetAutoCalculateAxis(bool autoCalc)
    {
        autoCalculateAxis = autoCalc;
    }
    
    public void ResetRotation()
    {
        transform.rotation = Quaternion.identity;
        _totalRotation = 0f;
    }
    
    public float GetTotalRotation()
    {
        return _totalRotation;
    }
}