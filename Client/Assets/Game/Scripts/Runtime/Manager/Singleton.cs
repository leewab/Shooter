using System;

/// <summary>
/// 非MonoBehaviour的基础单例类（泛型实现，可复用）
/// </summary>
/// <typeparam name="T">继承该类的具体业务类型（必须有无参数的私有构造函数）</typeparam>
public class Singleton<T> where T : class, new()
{
    // 1. 静态私有实例（volatile关键字确保多线程下实例状态的可见性，避免指令重排导致的空引用）
    private static volatile T _instance;
    
    // 2. 静态锁对象（保证多线程同步，避免并发创建实例）
    private static readonly object _lockObj = new object();
    
    // 3. 公开只读实例属性（外部唯一访问入口）
    public static T Instance
    {
        get
        {
            // 第一层检查：如果实例已创建，直接返回（避免频繁进入锁逻辑，提升性能）
            if (_instance == null)
            {
                // 锁定临界区：确保同一时间只有一个线程进入初始化逻辑
                lock (_lockObj)
                {
                    // 第二层检查：锁定后再次确认实例是否为null（避免多个线程等待锁后重复创建）
                    if (_instance == null)
                    {
                        _instance = new T();
                        // 可选：调用实例的初始化方法（如需自定义初始化逻辑）
                        (_instance as Singleton<T>)?.OnInitialize();
                    }
                }
            }
            return _instance;
        }
    }
    
    // 4. 私有构造函数（核心！禁止外部使用new关键字创建实例，保证单例唯一性）
    protected Singleton()
    {
        // 防止反射破坏单例（可选增强）
        if (_instance != null)
        {
            throw new InvalidOperationException("该类已实现单例模式，无法通过构造函数创建新实例！");
        }
    }
    
    // 5. 可选：初始化回调方法（子类可重写，实现自定义初始化逻辑）
    protected virtual void OnInitialize()
    {
        // 子类重写此方法，替代MonoBehaviour的Start()
    }
}