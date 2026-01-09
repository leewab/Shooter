using UnityEngine;
using UnityEngine.Events;
using Gameplay;
using UnityEngine.Serialization;

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
    public float MaxHealth;
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
    public SpriteRenderer spriteRenderer;
    private Collider2D jointCollider;
    private AudioSource audioSource;
    
    // 私有变量
    private float _currentHealth = 0;
    private Color currentColor;
    private ColorType colorType;
    private bool _isAlive = true;
    private DragonController controller;
    private DragonJointData _jointData;
    
    private void Awake()
    {
        jointCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        // 查找控制器
        controller = GetComponentInParent<DragonController>();
        gameObject.tag = "DragonJoint";
        gameObject.layer = LayerMask.NameToLayer("Game");
    }

    public void SetData(DragonJointData jointData)
    {
        _isAlive = true;
        _jointData = jointData;
        _currentHealth = jointData.MaxHealth;
        JointIndex = jointData.JointIndex;
        SetColorType(jointData.ColorType);
    }
    
    // 设置颜色类型
    public void SetColorType(ColorType newType)
    {
        colorType = newType;
        currentColor = TurretManager.Instance.GetColor(colorType);
        if (spriteRenderer != null)
        {
            spriteRenderer.color = currentColor;
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
        
        if (jointCollider != null) jointCollider.enabled = false;
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        
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
        return colorType;
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