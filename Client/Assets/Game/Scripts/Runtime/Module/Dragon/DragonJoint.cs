using GameConfig;
using UnityEngine;
using UnityEngine.Events;
using Gameplay;

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
    public ConfDragonJoint ConfDragonJoint;
    public int JointIndex;
}

public class DragonJoint : MonoBehaviour
{
    public int JointIndex = 0;
    public UnityEvent<int> onDestroyed;
    public UnityEvent<float> onHealthChanged;

    private GameObject destroyEffect;
    private GameObject damageEffect;
    private AudioClip damageSound;
    private AudioClip destroySound;

    // 组件
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private MeshRenderer meshRender;
    
    
    private AudioSource audioSource;
    
    // 私有变量
    private float _currentHealth = 0;
    private Color _currentColor;
    private ColorType _colorType;
    private bool _isAlive = true;
    private DragonJointData _jointData;
    private ConfDragonJoint _confDragonJoint;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        gameObject.tag = "DragonJoint";
        gameObject.layer = LayerMask.NameToLayer("Game");
    }

    public void SetData(DragonJointData jointData)
    {
        _isAlive = true;
        _jointData = jointData;
        _confDragonJoint = jointData.ConfDragonJoint;
        _currentHealth = _confDragonJoint?.Health ?? 0;
        JointIndex = jointData.JointIndex;
        SetColorType(jointData.ColorType);
    }
    
    // 设置颜色类型
    public void SetColorType(ColorType newType)
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
            meshRender.material.SetColor("_FresnelColor", Color.white); 
            meshRender.material.SetFloat("_FresnelPower", 0.862f);
            meshRender.material.SetFloat("_FresnelIntensity", 0.641f);
        }
    }
    
    // 受到伤害
    public void TakeDamage(int damage = 1)
    {
        if (!_isAlive) return;
        
        _currentHealth -= damage;
        onHealthChanged?.Invoke(_currentHealth);
        
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
        
        if (damageEffect != null)
        {
            Instantiate(damageEffect, transform.position, Quaternion.identity);
        }
        
        if (_currentHealth <= 0)
        {
            DestroyJoint();
        }
    }
    
    // 摧毁关节
    public void DestroyJoint()
    {
        if (!_isAlive) return;
        
        _isAlive = false;
        
        if (audioSource != null && destroySound != null)
        {
            audioSource.PlayOneShot(destroySound);
        }
        
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }
        
        onDestroyed?.Invoke(JointIndex);
        
        // 稍微延迟销毁，确保控制器有足够时间处理
        Invoke(nameof(ActuallyDestroy), 0.05f);
    }
    
    private void ActuallyDestroy()
    {
        Destroy(gameObject);
    }
    
    // 获取颜色类型
    public ColorType GetColorType()
    {
        return _colorType;
    }
    
    // 是否存活
    public bool IsAlive()
    {
        return _currentHealth > 0;
    }
    
    // 是否为头部
    public bool IsHead()
    {
        return _jointData.JointType == DragonJointType.Head;
    }

    // 是否为尾部
    public bool IsTail()
    {
        return _jointData.JointType == DragonJointType.Tail;
    }
}