## 任务目标：


### 重要说明：

1. 生成代码时不要写注释，代码要求简洁明了
2. 不要写重复代码，最忌花里胡哨的无用输出
3. 对于变量名修改的优化等等不需要管
4. 先构思好想法再开始写代码


### 项目结构：

1. Scripts/Editor 是游戏编辑器代码
2. Scripts/Runtime下是游戏运行时代码
3. Scripts/Runtime/Conf下是配置表结构
4. Scripts/Runtime/Manager下是管理器类
5. Scripts/Runtime/Module下是各个游戏模块
6. Scripts/Runtime/Module/Base 是部分基类
7. Scripts/Runtime/Module/Bullet 是子弹模块
8. Scripts/Runtime/Module/Dragon 是boss龙模块
9. Scripts/Runtime/Module/Turret 是炮台模块


### 项目概述：

1. 检索项目中的代码可以知道，我要写一个小游戏，当前目录中是小游戏的代码部分，你需要完成代码部分的补充和完善
2. 我需要的游戏效果是，上方的Boss龙会在游戏开始按照轨迹进入游戏场景，下方的放置的炮台会自动攻击龙的骨骼，且只有对应颜色的炮台可以击毁龙的骨骼，击毁龙的骨骼之后，龙向后对齐，目前已经实现效果了
3. Turret池子中生成之后，可以点击第一排的炮TurretEntity，并将其放入炮台，也就是激活TurretEntity
