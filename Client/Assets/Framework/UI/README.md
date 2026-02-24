# UIFramework 使用指南

## 简介

一个基于 Unity Addressable 的轻量级 UGUI 框架，提供完整的 UI 面板管理功能。

## 核心功能

- ✅ 基于 Addressable 的异步资源加载
- ✅ UI 面板的加载、打开、关闭管理
- ✅ UI 层级管理（Background、Normal、Popup、Toast、Top）
- ✅ 面板生命周期管理
- ✅ 单例/多例面板支持
- ✅ 面板栈管理（自动处理暂停/恢复）
- ✅ 预加载和资源释放

## 目录结构

```
Framework/UIFramework/
├── UIPanel.cs                 # UI面板基类
├── UILayer.cs                 # UI层级管理
├── UIFramework.cs             # UI框架核心
├── UIFrameworkExtensions.cs   # 扩展方法和辅助工具
├── ExamplePanel.cs            # 示例面板
└── README.md                  # 本文档
```

## 快速开始

### 1. 初始化框架

在游戏启动时初始化 UIFramework：

```csharp
void Start()
{
    UIFramework.Instance.Initialize();
}
```

### 2. 创建 UI 面板

创建一个继承自 `UIPanel` 的面板类：

```csharp
public class MainMenuPanel : UIPanel
{
    protected override void OnAwake()
    {
        // 绑定UI组件
    }

    protected override void OnOpen(object args)
    {
        // 打开逻辑
    }

    protected override void OnClose()
    {
        // 关闭逻辑
    }
}
```

### 3. 设置 Addressable

将 UI Prefab 标记为 Addressable：
- 在 Project 窗口选中 Prefab
- 右键 → Addressables → Add to Addressables
- 设置 Address（例如：`UI/MainMenuPanel`）

### 4. 打开/关闭面板

```csharp
// 方式1：使用扩展方法（推荐）
"UI/MainMenuPanel".Open();
"UI/MainMenuPanel".Open(userData);
"UI/MainMenuPanel".Close();

// 方式2：使用 UIFramework
UIFramework.Instance.OpenPanel("UI/MainMenuPanel", userData);
UIFramework.Instance.ClosePanel("UI/MainMenuPanel");

// 方式3：在面板内提供静态方法
public class MainMenuPanel : UIPanel
{
    public static void Show(object args = null)
    {
        "UI/MainMenuPanel".Open(args);
    }
}

// 使用
MainMenuPanel.Show();
```

## 面板生命周期

```
Awake → Start → Initialize → Open → Pause/Resume → Close → Destroy
```

| 方法 | 调用时机 | 说明 |
|------|---------|------|
| `OnAwake()` | Unity Awake | 初始化组件引用 |
| `OnStart()` | Unity Start | 启动逻辑 |
| `OnInitialize()` | 首次打开 | 只调用一次，用于数据初始化 |
| `OnOpen(object args)` | 每次打开 | 传递参数，显示UI |
| `OnPause()` | 被覆盖 | 当其他面板打开在上方时 |
| `OnResume()` | 恢复显示 | 当上方面板关闭后 |
| `OnClose()` | 关闭面板 | 隐藏UI，保存状态 |
| `OnDestroyed()` | Unity OnDestroy | 清理资源 |

## 面板类型

```csharp
public enum PanelType
{
    Normal,  // 普通面板
    Fixed,   // 固定面板（如主界面）
    Popup,   // 弹窗面板
    Toast    // 提示面板
}

public enum PanelShowMode
{
    Single,   // 单例模式（缓存复用）
    Multiple  // 多例模式（每次新建）
}
```

配置示例：

```csharp
public class MyPanel : UIPanel
{
    public override PanelType PanelType => PanelType.Popup;
    public override PanelShowMode ShowMode => PanelShowMode.Single;
}
```

## UI 层级

框架自动创建以下层级（从低到高）：

1. **BackgroundLayer** (0) - 固定UI（主界面）
2. **NormalLayer** (100) - 普通UI
3. **PopupLayer** (200) - 弹窗UI
4. **ToastLayer** (300) - 提示UI
5. **TopLayer** (400) - 最高层级（引导、Loading）

根据面板的 `PanelType` 自动分配到对应层级。

## 常用功能

### 预加载面板

```csharp
UIFramework.Instance.PreloadPanel("UI/BattlePanel", () =>
{
    Debug.Log("预加载完成");
});
```

### 获取缓存面板

```csharp
UIPanel panel = UIFramework.Instance.GetCachedPanel("UI/MainMenuPanel");
if (panel != null)
{
    // 面板已加载
}
```

### 关闭当前面板

```csharp
UIFramework.Instance.CloseCurrentPanel();
```

### 卸载面板

```csharp
// 从缓存中移除并销毁
UIFramework.Instance.UnloadPanel("UI/MainMenuPanel");
```

### 场景切换时清空

```csharp
UIFramework.Instance.ClearAllPanels();
```

## 扩展方法

```csharp
// 绑定按钮事件
closeButton.SetOnClick(() => Debug.Log("Clicked"));

// 安全设置文本
titleText.SetTextSafe("Hello");

// 安全设置图片
iconImage.SetImageSafe(sprite);

// 安全激活/禁用
panelObj.SetActiveSafe(true);
```

## 动画辅助

```csharp
// 淡入
StartCoroutine(UIAnimationHelper.FadeIn(canvasGroup, 0.3f));

// 淡出
StartCoroutine(UIAnimationHelper.FadeOut(canvasGroup, 0.2f, () =>
{
    Debug.Log("淡出完成");
}));

// 缩放
StartCoroutine(UIAnimationHelper.ScaleAnimation(
    transform,
    Vector3.zero,
    Vector3.one,
    0.3f
));
```

## 最佳实践

1. **面板命名规范**
   - Prefab Address: `UI/PanelName`
   - 类名: `PanelNamePanel`

2. **资源路径规范**
   ```
   Assets/Res/UI/
   ├── Panels/          # 面板Prefab
   ├── Atlas/          # 图集
   └── Textures/       # 纹理
   ```

3. **生命周期选择**
   - 单次初始化用 `OnInitialize()`
   - 每次打开刷新用 `OnOpen()`
   - 频繁变化的用 `OnPause/OnResume()`

4. **性能优化**
   - 常用面板设置为单例并预加载
   - 不常用面板使用多例模式
   - 及时卸载不需要的面板
   - 使用对象池处理频繁创建的UI

5. **注意事项**
   - 确保面板 Prefab 已标记为 Addressable
   - 面板必须有 UIPanel 组件（会自动添加）
   - ResourceManager 需要先初始化

## 完整示例

```csharp
// SettingsPanel.cs
public class SettingsPanel : UIPanel
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Button closeButton;

    public override PanelType PanelType => PanelType.Popup;
    public override PanelShowMode ShowMode => PanelShowMode.Single;

    protected override void OnAwake()
    {
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        closeButton.SetOnClick(OnCloseClick);
    }

    protected override void OnOpen(object args)
    {
        // 加载设置
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
    }

    private void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
    }

    private void OnCloseClick()
    {
        Close();
    }

    // 静态方法供外部调用
    public static void Show()
    {
        "UI/SettingsPanel".Open();
    }
}

// 使用示例
SettingsPanel.Show();
```

## 依赖关系

- Unity Addressables
- UnityEngine.UI
- Framework.ResourceManager

## 更新日志

- v1.0.0 - 初始版本
  - 基础面板管理
  - Addressable 集成
  - 层级系统
  - 生命周期管理
