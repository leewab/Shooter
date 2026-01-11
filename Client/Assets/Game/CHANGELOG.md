# CHANGELOG - 项目变更日志

> 本文档记录项目功能的开发历程和实现状态

---

## 2025-01-10 - 游戏流程控制系统

### 实现功能
- ✅ 创建 GameController 作为游戏主控制器
- ✅ 实现完整的游戏状态管理（None, Preparing, Playing, Paused, GameOver）
- ✅ 实现游戏流程控制方法（Prepare, Start, Pause, Resume, End, Restart, Quit）
- ✅ 集成分数和时间追踪系统
- ✅ 事件驱动的游戏结束通知机制

### 核心改进
**GameController.cs** (新增文件):
- 继承 SingletonMono 模式，全局唯一实例
- 管理游戏状态流转：
  - `PrepareGame()` - 初始化游戏系统，重置分数和时间
  - `StartGame()` - 开始游戏，龙开始移动
  - `PauseGame()` / `ResumeGame()` - 暂停和恢复游戏
  - `EndGame(bool win)` - 游戏结束，显示结果面板
  - `RestartGame()` - 重启游戏
  - `QuitGame()` - 退出游戏
- 协调 DragonController 和 TurretsGrid
- 注册 DragonManager.OnSuccessEvent 事件监听游戏结果
- 提供分数系统 `AddScore(int points)` 和事件 `OnScoreChanged`
- 提供游戏时间追踪（Playing 状态下自动累加）

### 设计说明
GameController 作为游戏流程的中枢，负责：
1. 状态管理 - 统一管理游戏状态转换
2. 模块协调 - 协调龙、炮台等模块的交互
3. 事件分发 - 通过 Action 事件通知外部系统
4. 分数统计 - 记录和分发分数变化
5. UI集成 - 自动打开/关闭游戏结果面板

### 效果说明
游戏现在有完整的流程控制，可以正确处理开始、暂停、结束、重玩等状态，并集成了 UI 系统显示游戏结果。

---

## 2025-01-10 - 资源加载系统重构

### 实现功能
- ✅ 完全重构 ResourceManager，支持运行时模式切换
- ✅ 使用 PlayerPrefs 存储加载模式配置
- ✅ 添加编辑器菜单栏快捷切换（Tools/Resource Mode）
- ✅ 非编辑器模式强制使用 Addressables
- ✅ 编辑器模式支持 Addressables / AssetDatabase 切换
- ✅ AssetDatabase 模式使用同步加载（无异步开销）

### 核心改进
**ResourceManager.cs**:
- 移除 #if USE_ADDRESSABLES 宏判断，改用运行时 _useAddressables 字段
- PlayerPrefs 键 "ResourceManager_UseAddressables" 存储模式（默认1=Addressables）
- 单一 _assetCache 缓存所有资源类型
- LoadAssetAsync 在 AssetDatabase 模式下同步加载：
  ```csharp
  #if UNITY_EDITOR
  if (_useAddressables)
      LoadAssetAsyncAddressable(assetKey, onComplete);
  else
  {
      T asset = LoadAssetAtPath<T>(assetKey);
      onComplete?.Invoke(asset); // 立即回调
  }
  #endif
  ```
- SyncLoad 保持原有逻辑，两种模式都支持
- ReleaseAsset 和 ReleaseAllAssets 根据模式正确释放资源

**ResourceManagerMenu.cs** (新增文件):
- 菜单路径：Tools/Resource Mode/
- 两个切换选项：
  - Addressables - 使用 Addressables 加载
  - AssetDatabase - 使用 AssetDatabase 加载（仅编辑器）
- Show Current Mode - 显示当前模式
- 使用 Menu.SetChecked 显示当前选中状态

### 性能优化
- 编辑器下开发时切换到 AssetDatabase 模式，避免 Addressables 异步等待
- 运行时统一使用 Addressables，确保构建后的正确性
- 单一缓存字典，减少内存占用

### 效果说明
开发者可以在编辑器下通过菜单快速切换资源加载模式，AssetDatabase 模式下资源加载立即完成，大幅提升开发迭代效率。

---

## 2025-01-10 - UI框架PanelType修复

### 实现功能
- ✅ 修复UIManager中PanelType被硬编码为Normal的问题
- ✅ 修复LoadPanelAsync从预制体读取PanelType
- ✅ 修复PreloadPanel支持PanelType参数传递

### 核心改进
**UIManager.cs**:
- 修改 LoadPanelAsync 方法：
  ```csharp
  PanelType panelType = tempPanel != null ? tempPanel.PanelType : PanelType.Normal;
  Transform parentLayer = uiRoot.GetLayerByPanelType(panelType);
  ```
- 从预制体实例读取 PanelType 属性，而不是硬编码 Normal

**UIManager.cs (扩展方法)**:
- 扩展 UIMgr 的 Open() 方法支持多种参数组合
- 所有 Open 方法重载都支持 PanelType 参数
- 统一接口风格，提升易用性

### 效果说明
UI 面板现在可以正确使用自身定义的 PanelType，不再全部默认为 Normal 层级。

---

## 2025-01-10 - 龙的移动和速度优化

### 实现功能
- ✅ 实现龙速度平滑过渡（从MaxMoveSpeed渐变到NormalMoveSpeed）
- ✅ 修复龙初始生成位置（在屏幕外远处）
- ✅ 修复龙到达终点检测逻辑

### 核心改进
**DragonController.cs**:
- 添加 `UpdateSpeed()` 方法，使用 Mathf.Lerp 平滑过渡速度：
  ```csharp
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
  ```
- 修改 `InitializeJointDistances()`，将 tailDistance 初始值设为负值，使龙在屏幕外生成
- 修改 `UpdateJointsPosition()`，在初始化时立即设置位置：
  ```csharp
  if (firstUpdate)
  {
      joint.transform.SetPositionAndRotation(pos, rot);
      firstUpdate = false;
  }
  ```
- 修复到达终点检测，改为检查 `targetDistance` 而非 tailDistance：
  ```csharp
  if (joint.IsHead() && targetDistance >= _PathData.totalLength - 0.1f)
  {
      OnGameOver();
      return;
  }
  ```
- 移除 `Invoke(nameof(OnGameSuccess), 2f)` 避免重复调用

### 效果说明
- 龙进入场景时从最大速度平滑过渡到正常速度，视觉更自然
- 龙从屏幕外远处生成，不会突然出现在场景中
- 龙到达终点时正确触发游戏结束

---

## 2025-01-09 - 炮台补位动画优化

### 实现功能
- ✅ 炮台补位时添加弹性移动动画，增加冲击感
- ✅ 优化炮台实例管理，使用字典缓存避免重复创建

### 核心改进
**TurretsGrid.cs**:
- 使用 `_TurretEntitiesMap` 字典缓存炮台实例（key = turretData.Id）
- 补位时从缓存获取炮台实例，避免重复从对象池获取
- 使用 `DOMove` + `Ease.OutBack` 播放0.3秒弹性补位动画
- 计算目标位置并移动到新位置

### 效果说明
炮台补位时使用 OutBack 缓动，先快速移动再弹性回弹，视觉冲击感更强。

---

## 2025-01-09 - 重构炮台攻击算法（性能优化）

### 实现功能
- ✅ 将炮台攻击系统从射线检测改为距离优先算法
- ✅ 移除低效的多射线检测（20条射线/帧）
- ✅ 实现智能目标选择，自动攻击最近的颜色匹配关节

### 核心改进
**DragonManager**:
- 新增 `SetCurrentDragon()` 方法，注册当前龙实例
- 新增 `GetAllAliveJoints()` 方法，获取所有存活关节

**DragonController**:
- 初始化时自动注册到 DragonManager

**TurretEntity**:
- 完全重构攻击逻辑：`PerformRaycastDetection()` → `PerformAttack()`
- 新增 `FindNearestMatchingJoint()` 方法，智能选择最近目标
- 移除所有射线相关代码（rayPoints, rayHits, hitJoints, rayLine, UpdateRayVisualization等）
- 简化Update逻辑，提升代码可读性

### 性能提升
- **检测次数**：从20次物理射线检测/帧 → 20次距离计算/帧
- **计算复杂度**：O(射线数 × 碰撞检测) → O(关节数量)
- **代码量**：减少约60行冗余代码
- **内存优化**：移除多个List和LineRenderer组件

### 效果说明
炮台现在会智能选择最近的颜色匹配龙关节进行攻击，性能大幅提升，代码更简洁易维护。

---

## 2025-01-09 - 优化炮台后坐力效果

### 实现功能
- ✅ 修改炮台后坐力方向，使其沿子弹飞出方向向后抖动

### 核心改进
- 修改 `TurretEntity.AttackJoint()` 方法，在发射子弹时记录射击方向
- 修改 `PlayRecoilAnimation()` 方法参数，接收射击方向
- 后坐力计算改为：`originalPosition - fireDirection * recoilDistance`
- 移除固定沿炮台向上的后坐力逻辑

### 效果说明
炮台发射子弹时，后坐力会沿着子弹飞行的反方向产生，使打击感更加真实和自然。

---

## 2025-01-09 - 项目初始化和文档创建

### 实现功能
- ✅ 创建 CHANGELOG 变更日志文档
- ✅ 建立功能模块说明书，记录项目整体架构

### 核心功能状态
- ✅ 龙路径移动系统
- ✅ 龙关节管理和摧毁
- ✅ 龙关节向后对齐
- ✅ 3×n 炮台网格系统
- ✅ 炮台消除和补位算法
- ✅ 炮台射线检测和自动攻击
- ✅ 颜色匹配系统
- ✅ 子弹发射和移动
- ✅ 子弹命中和伤害
- ✅ 对象池系统
- ✅ 后坐力动画和特效

### 待完善功能
- ⏳ 音效系统集成
- ⏳ 特效资源完善
- ⏳ 关卡系统
- ⏳ 分数系统
- ⏳ UI 界面
- ⏳ 游戏流程控制（开始/暂停/结束）
- ⏳ 龙的多种行为模式
- ⏳ 炮台升级系统

---

# 功能模块说明书

## 项目概述

### 游戏类型
Unity 2D 塔防射击游戏

### 核心玩法
- 上方 Boss 龙按轨迹进入场景并移动
- 下方炮台自动攻击龙的骨骼关节
- 只有对应颜色的炮台才能击毁对应颜色的龙骨骼
- 击毁龙骨骼后，龙向后对齐
- 玩家点击第一排炮台并放置到炮台座位上激活

### 当前状态
核心功能已实现，处于代码补充和完善阶段

---

## 目录结构

```
Assets/Game/
├── Battery/                    # 主游戏代码目录
│   ├── Scripts/
│   │   ├── Editor/            # 编辑器扩展代码
│   │   └── Runtime/           # 运行时代码
│   │       ├── Conf/          # 配置数据结构
│   │       ├── Manager/       # 管理器类
│   │       ├── Module/        # 游戏功能模块
│   │       │   ├── Base/      # 基础类
│   │       │   ├── Bullet/    # 子弹模块
│   │       │   ├── Dragon/    # 龙模块
│   │       │   └── Turret/    # 炮台模块
│   │       └── Util/          # 工具类
│   ├── README.md              # 开发说明文档
│   └── TASK.md                # 任务清单
└── 功能模块说明书.md            # 本文档
```

---

## 核心模块详解

### 1. Dragon（龙/Boss）模块

**功能职责**
- 管理龙的整体移动和生命周期
- 控制龙关节的创建、销毁和对齐
- 处理关节被摧毁后的重新对齐逻辑

**核心文件**

#### DragonController.cs
- **路径**: `Scripts/Runtime/Module/Dragon/DragonController.cs`
- **功能**: 龙的主控制器
- **关键方法**:
  - `InitializeDragon()` - 初始化龙和所有关节
  - `UpdateHeadPosition()` - 更新头部位置沿路径移动
  - `UpdateJointsPosition()` - 更新所有关节位置
  - `OnJointDestroyed(int jointIndex)` - 关节被摧毁回调
  - `StartMoving()` / `StopMoving()` - 控制移动状态

#### DragonJoint.cs
- **路径**: `Scripts/Runtime/Module/Dragon/DragonJoint.cs`
- **功能**: 单个龙关节实体
- **关键属性**:
  - `JointIndex` - 关节索引
  - `ColorType` - 颜色类型（红/绿/蓝/黄/橙/紫）
  - `MaxHealth` / `CurrentHealth` - 生命值
  - `JointType` - 关节类型（Head/Tail/Body）
- **关键方法**:
  - `SetData(DragonJointData)` - 设置关节数据
  - `TakeDamage(int damage)` - 受到伤害
  - `DestroyJoint()` - 摧毁关节
  - `IsAlive()` / `IsHead()` / `IsTail()` - 状态查询

#### DragonManager.cs
- **路径**: `Scripts/Runtime/Module/Dragon/DragonManager.cs`
- **功能**: 龙的管理器，单例模式
- **职责**: 配置管理、颜色管理

**配置文件**
- DragonConf.cs - 龙的配置参数
  - 移动速度、关节数量、关节间距、位置平滑度等

---

### 2. Turret（炮台）模块

**功能职责**
- 管理 3×n 炮台网格系统
- 处理炮台的放置、激活、移除和补位
- 自动检测并攻击龙关节
- 炮台对象池管理

**核心文件**

#### TurretHandler.cs
- **路径**: `Scripts/Runtime/Module/Turret/TurretHandler.cs`
- **功能**: 3×n 炮台网格管理器（核心算法）
- **关键方法**:
  - `InitTurretGrid(int rowCount)` - 初始化 3×n 网格
  - `EliminateTurret(TurretData)` - 消除炮台（核心消除算法）
  - `AutoFillColumn(int column)` - 列自动补位（核心补位算法）
  - `GetFrontTurret(int column)` - 获取列最前置炮台
- **数据结构**:
  - `_turretGrid`: `List<List<TurretData>>` - 3列网格数据
  - 每列独立管理，positionIndex 0 为最前置

#### TurretEntity.cs
- **路径**: `Scripts/Runtime/Module/Turret/TurretEntity.cs`
- **功能**: 炮台实体，继承自 BaseTurret
- **关键功能**:
  - 射线检测龙关节（多射线覆盖）
  - 颜色匹配验证
  - 发射子弹攻击
  - 后坐力动画（DOTween）
  - 炮口闪光特效
- **关键方法**:
  - `SetTurret(TurretData td)` - 设置炮台数据
  - `SetActive(bool active)` - 激活/停用炮台
  - `PerformRaycastDetection()` - 执行射线检测
  - `AttackJoint(DragonJoint joint)` - 攻击关节
  - `PlayRecoilAnimation()` - 播放后坐力动画
  - `UpdateRayVisualization()` - 更新射线可视化

#### TurretsGrid.cs
- **路径**: `Scripts/Runtime/Module/Turret/TurretsGrid.cs`
- **功能**: 炮台网格视图，负责视觉层实例化
- **关键方法**:
  - `InitTurretGrid()` - 初始化炮台网格视图
  - `UpdateTurretGrid(int column)` - 更新指定列的视觉表现
  - `GenerateTurret(TurretData)` - 生成炮台对象

#### TurretSeat.cs
- **路径**: `Scripts/Runtime/Module/Turret/TurretSeat.cs`
- **功能**: 炮台座位，用于放置激活的炮台
- **状态**: 锁定/解锁，占用/空闲

#### TurretManager.cs
- **路径**: `Scripts/Runtime/Module/Turret/TurretManager.cs`
- **功能**: 炮台管理器，单例模式
- **职责**:
  - 炮台配置管理（TurretConf 字典）
  - 颜色类型转 Color 映射
  - 炮台预制体加载
  - 对象池接口封装

**配置文件**
- TurretConf.cs - 炮台配置参数
  - Id, ColorType, AttackCooldown, MaxHitNum
  - 后坐力参数、炮口闪光参数等

---

### 3. Bullet（子弹）模块

**功能职责**
- 子弹发射和移动
- 碰撞检测和伤害处理
- 命中特效和音效
- Hit Stop 顿帧效果
- 屏幕震动

**核心文件**

#### BulletEntity.cs
- **路径**: `Scripts/Runtime/Module/Bullet/BulletEntity.cs`
- **功能**: 子弹实体，继承自 BaseBullet
- **关键功能**:
  - 固定方向直线移动
  - 最大飞行距离限制
  - 碰撞检测（障碍物/龙关节）
  - 颜色匹配验证
  - 发射缩放动画（DOTween）
  - 命中特效、音效、屏幕震动、Hit Stop
- **关键方法**:
  - `SetupBullet(int id, ColorType colorType, Vector2 direction)` - 设置子弹
  - `PlayLaunchScaleAnimation()` - 发射缩放动画
  - `HandleHit(GameObject hitObject)` - 处理命中
  - `PlayHitEffects()` - 播放命中效果
  - `DoHitStop(float duration)` - 命中顿帧

#### BulletManager.cs
- **路径**: `Scripts/Runtime/Module/Bullet/BulletManager.cs`
- **功能**: 子弹管理器，单例模式
- **职责**: 子弹配置管理、对象池管理

**配置文件**
- BulletConf.cs - 子弹配置参数

---

### 4. Base（基础类）模块

**核心文件**

#### BaseTurret.cs
- **路径**: `Scripts/Runtime/Module/Base/BaseTurret.cs`
- **功能**: 炮台基类
- **职责**: 定义炮台通用接口

#### BaseBullet.cs
- **路径**: `Scripts/Runtime/Module/Base/BaseBullet.cs`
- **功能**: 子弹基类
- **职责**: 定义子弹通用接口

#### PathPointData.cs
- **路径**: `Scripts/Runtime/Module/Base/PathPointData.cs`
- **功能**: 路径点数据
- **职责**: 存储和查询龙的移动路径
- **关键方法**:
  - `GetPositionRotationScaleAtDistance(float distance)` - 根据距离获取位置、旋转、缩放
  - `HasData()` - 是否有路径数据

---

### 5. Manager（管理器）模块

**核心文件**

#### GameObjectPool.cs
- **路径**: `Scripts/Runtime/Manager/GameObjectPool.cs`
- **功能**: 通用对象池
- **职责**: 减少实例化开销，复用游戏对象

#### AudioManager.cs
- **路径**: `Scripts/Runtime/Manager/AudioManager.cs`
- **功能**: 音频管理器，单例模式
- **职责**: 播放音效、背景音乐

#### EffectManager.cs
- **路径**: `Scripts/Runtime/Manager/EffectManager.cs`
- **功能**: 特效管理器，单例模式
- **职责**: 实例化和管理特效对象

#### Singleton.cs / SingletonMono.cs
- **路径**: `Scripts/Runtime/Manager/`
- **功能**: 单例基类
- **职责**: 提供单例模式实现

---

### 6. Util（工具类）模块

**核心文件**

#### ScreenTopDivider.cs
- **路径**: `Scripts/Runtime/Util/ScreenTopDivider.cs`
- **功能**: 屏幕顶部区域划分
- **关键方法**:
  - `GetCoverageAreaPositions(...)` - 获取覆盖区域的点位列表
- **用途**: 为炮台射线检测提供目标点位

#### CameraShake.cs
- **路径**: `Scripts/Runtime/Util/CameraShake.cs`
- **功能**: 屏幕震动效果
- **关键方法**:
  - `Shake(float duration, float intensity)` - 触发震动

---

### 7. Conf（配置）模块

**配置文件列表**

#### TurretConf.cs
- Id - 炮台ID
- ColorType - 颜色类型
- AttackCooldown - 攻击冷却时间
- DamagePerShot - 每次攻击伤害
- MaxHitNum - 最大攻击次数
- BulletName / BulletId - 子弹配置
- RecoilDistance / RecoilDuration / RecoilRotation - 后坐力参数
- MuzzleFlashDuration / MuzzleEffectName / MuzzleEffectScale - 炮口闪光参数

#### DragonConf.cs
- NormalMoveSpeed - 正常移动速度
- MaxMoveSpeed - 最大移动速度
- MasSpeedTime - 最大速度持续时间
- JointSpacing - 关节间距
- MaxJoints - 最大关节数量
- PositionSmoothness - 位置平滑度
- RealignSpeed - 重新对齐速度
- MaxJointHealth - 最大关节生命值

#### BulletConf.cs
- Speed - 子弹速度
- MaxTravelDistance - 最大飞行距离
- StartScale / ScaleDuration - 发射缩放参数
- Damage - 伤害
- HitEffectName / HitEffectDuration - 命中特效参数
- HitSoundName - 命中音效
- ScreenShakeDuration / ScreenShakeIntensity - 屏幕震动参数
- HitStopDuration - 命中顿帧时长

---

## 数据流和关键算法

### 1. 炮台消除和补位算法（TurretHandler）

**消除算法**
```
1. 校验目标炮台是否存活
2. 校验目标炮台是否为列的最前置（前方无存活炮台）
3. 标记炮台为已消除（IsAlive = false）
4. 触发列自动补位
```

**补位算法**
```
1. 筛选列中所有存活炮台
2. 重置列中所有炮台状态
3. 存活炮台向前填充（从 positionIndex = 0 开始）
4. 更新炮台的 PositionIndex
5. 触发视觉更新事件
```

### 2. 龙关节对齐算法

**关节位置更新**
```
1. 计算 tailDistance（尾部沿路径的距离）
2. 每个关节目标距离 = tailDistance + 关节索引 × 关节间距
3. Lerp 插值平滑移动到目标距离
4. 从路径数据获取对应距离的位置和旋转
5. 更新关节 Transform
```

**关节被摧毁后对齐**
```
1. 从关节列表移除已摧毁关节
2. 剩余关节自动向前填充
3. 保持关节间距
4. 无需手动移动，位置更新逻辑自动处理
```

### 3. 炮台攻击流程

```
1. 炮台激活（SetActive(true)）
2. 初始化射线方向点（ScreenTopDivider）
3. 每帧执行射线检测：
   a. 遍历所有射线方向
   b. Physics2D.RaycastAll 检测
   c. 过滤标签 "DragonJoint"
   d. 获取 DragonJoint 组件
   e. 验证颜色匹配
   f. 记录命中关节
4. 攻击冷却结束后：
   a. 遍历命中关节
   b. 发射子弹（BulletManager.InstantiateBullet）
   c. 播放后坐力动画
   d. 播放炮口闪光
   e. 扣减攻击次数
5. 攻击次数耗尽：
   a. 销毁炮台
   b. 回收到对象池
```

### 4. 子弹命中处理

```
1. OnTriggerEnter2D / OnCollisionEnter2D
2. HandleHit(GameObject):
   a. 忽略子弹自身碰撞
   b. 检查障碍物层 → 播放命中效果 → 销毁
   c. 检查龙关节：
      - 获取 DragonJoint 组件
      - 验证颜色匹配
      - 验证非头部/尾部
      - 造成伤害（TakeDamage）
      - 播放命中效果
      - 销毁子弹
```

---

## 游戏标签和层级

### 标签（Tags）
- `DragonJoint` - 龙关节
- `Turret` - 炮台
- `Bullet` - 子弹

### 层级（Layers）
- `Game` - 游戏主要对象（龙关节、子弹）
- `Obstacle` - 障碍物
- `Ground` - 地面
- `Default` - 默认

---

## 颜色系统

### ColorType 枚举
```csharp
Red, Green, Blue, Yellow, Orange, Purple
```

### 颜色映射（TurretManager）
- Red → Color.red
- Green → Color.green
- Blue → Color.blue
- Yellow → Color.yellow
- Orange → RGB(0.5, 0.2, 0.016)
- Purple → RGB(0.5, 0, 0)

### 匹配规则
- 炮台颜色 == 关节颜色 → 可以击毁
- 炮台颜色 != 关节颜色 → 无法击毁

---

## 对象池使用

### 池化对象
- Turret（炮台）
- Bullet（子弹）
- Effect（特效）

### 使用模式
```csharp
// 获取
GameObjectPool<BaseTurret>.Instance.GetObject(prefab, parent, position, rotation)

// 回收
GameObjectPool<BaseTurret>.Instance.RecycleObject(prefab, obj)
```

---

## 第三方库

### DOTween (DG.Tweening)
- 用途：动画系统
- 使用场景：
  - 炮台后坐力动画
  - 子弹发射缩放动画
  - Hit Stop 顿帧

---

## 开发规范

### 代码风格
1. 不写注释，代码自解释
2. 避免重复代码
3. 简洁明了，不花哨
4. 先构思再编码

### 命名规范
- 私有字段：`_camelCase`
- 公共属性：`PascalCase`
- 方法：`PascalCase`
- 事件：`PascalCase` + `Event` 后缀（可选）

### 设计模式
- 单例模式：Manager 类
- 对象池模式：游戏对象复用
- 事件模式：UnityEvent
- 组件模式：Unity MonoBehaviour

---

## 当前实现状态

### 已完成 ✅
- 龙路径移动系统
- 龙关节管理和摧毁
- 龙关节向后对齐
- 3×n 炮台网格系统
- 炮台消除和补位算法
- 炮台射线检测
- 颜色匹配系统
- 子弹发射和移动
- 子弹命中和伤害
- 对象池系统
- 后坐力动画
- 炮口闪光特效
- 命中顿帧效果
- 屏幕震动

### 待完善 📋
- 音效系统集成
- 特效资源完善
- 关卡系统
- UI 界面完善
- 龙的多种行为模式
- 炮台升级系统
- GameController 场景集成
- 分数系统完善（当前已实现基础框架）

---

## 快速索引

### 想要修改龙的移动参数
→ `DragonConf.cs` 修改速度、间距等参数

### 想要修改炮台攻击力
→ `TurretConf.cs` 修改 `DamagePerShot` 或 `MaxHitNum`

### 想要修改子弹伤害
→ `BulletConf.cs` 修改 `Damage`

### 想要调整炮台网格大小
→ `TurretHandler.InitTurretGrid(rowCount)` 修改行数

### 想要修改射线覆盖区域
→ `TurretEntity.InitializeRayDirections()` 调整 `ScreenTopDivider.GetCoverageAreaPositions()` 参数

### 想要添加新的炮台类型
→ `TurretManager._TurretConf` 字典添加新配置

### 想要添加新的龙关节类型
→ `DragonJointType` 枚举添加新类型

---

## 更新日志

### 2025-XX-XX
- 初始版本
- 完成核心战斗系统
- 完成对象池和基础管理器
- 完成文档编写

---

## 联系和参考

- 项目路径：`Assets/Game/Battery/`
- README：`Assets/Game/Battery/README.md`
- 任务清单：`Assets/Game/Battery/TASK.md`

---

**文档版本**: 1.1
**最后更新**: 2025-01-10
**维护者**: Claude Code
