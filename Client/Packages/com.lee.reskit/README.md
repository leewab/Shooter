# AssetBundle Toolkit

一个集成化的AssetBundle打包工具，支持资源标记、批量管理和详细的Manifest生成。

## 功能特性

- 🚀 **一体化界面**：所有功能集成在一个窗口中
- 📁 **智能资源选择**：支持文件夹扫描和批量标记
- 🔧 **灵活的Bundle管理**：支持独立Bundle和文件夹结构
- 📊 **详细Manifest**：生成包含依赖关系的完整Manifest
- ⚡ **快速操作**：一键标记、清除和打包

## 安装

1. 将整个`Editor/AssetBundleToolkit`文件夹复制到项目的`Assets/Editor`目录下
2. 或者在Unity的Package Manager中添加本地包

## 使用方法

1. 打开窗口：`Window -> AssetBundle Toolkit`
2. 选择资源并标记Bundle名称
3. 配置打包选项
4. 点击"开始打包"

### 快速操作

- **标记选中资源**：选中资源后，开启"选择即标记"并输入Bundle名称
- **标记文件夹**：选中文件夹后点击"标记选中文件夹"
- **独立Bundle**：选中资源后点击"标记为独立Bundle"
- **批量清除**：点击"清除所有标记"

## Manifest文件结构

打包后生成的Manifest包含：
- 构建时间和目标平台
- 所有Bundle的详细信息（名称、Hash、大小）
- 每个资源的完整依赖链
- 资源到Bundle的映射关系

## 支持的Unity版本

Unity 2020.3 或更高版本

## 许可证

MIT License