#!/bin/bash

# 设置控制台编码
export LANG=en_US.UTF-8

cd "$(dirname "$0")" || exit
echo "========================================"
echo "  Excel 批量导出工具"
echo "========================================"
echo ""

# 配置参数
INPUT_DIR="."
OUTPUT_ROOT="Output"
NAMESPACE="GameConfig"
EXETOOL_DIR="bin/Release/net6.0/ExcelToJsonTool.dll"  # 注意：.NET 6通常使用.dll

# Unity项目输出路径
OUTPUT_CSHARP="../../Client/Assets/Game/Scripts/Runtime/Conf"
OUTPUT_JSON="../../Res/Assets/Product/Conf"

# 1. 检查工具是否存在
if [ ! -f "$EXETOOL_DIR" ]; then
    echo "错误: 找不到导出工具 $EXETOOL_DIR"
    echo "请确保已编译工具: dotnet publish -c Release"
    read -p "按回车键退出..."
    exit 1
fi
echo "✓ 工具就绪"

# 2. 创建输出目录
echo ""
echo "创建输出目录..."
if [ -d "$OUTPUT_ROOT/Models" ]; then
    rm -rf "$OUTPUT_ROOT/Models"
fi
if [ -d "$OUTPUT_ROOT/Json" ]; then
    rm -rf "$OUTPUT_ROOT/Json"
fi
mkdir -p "$OUTPUT_ROOT/Models"
mkdir -p "$OUTPUT_ROOT/Json"

# 3. 处理所有Excel文件
echo ""
echo "开始处理Excel文件..."
echo ""

echo "正在处理... $INPUT_DIR"
# 使用dotnet运行dll文件
dotnet "$EXETOOL_DIR" "$INPUT_DIR" "$OUTPUT_ROOT/Models" "$OUTPUT_ROOT/Json" "$NAMESPACE"
if [ $? -ne 0 ]; then
    echo "[失败] 导出失败"
else
    echo "[成功] 导出完成"
fi
echo ""

# 4. 拷贝到Unity项目
echo ""
echo "拷贝文件到Unity项目..."

# 创建Unity目录（如果不存在）
if [ -d "$OUTPUT_CSHARP" ]; then
    rm -rf "$OUTPUT_CSHARP"
fi
if [ -d "$OUTPUT_JSON" ]; then
    rm -rf "$OUTPUT_JSON"
fi
mkdir -p "$OUTPUT_CSHARP"
mkdir -p "$OUTPUT_JSON"

# 拷贝C#文件
if ls "$OUTPUT_ROOT/Models/"*.cs 1> /dev/null 2>&1; then
    echo "拷贝C#文件..."
    cp "$OUTPUT_ROOT/Models/"*.cs "$OUTPUT_CSHARP/" 2>/dev/null
    echo "✓ 已拷贝到: $OUTPUT_CSHARP/"
else
    echo "警告: 没有找到C#文件"
fi

# 拷贝JSON文件
if ls "$OUTPUT_ROOT/Json/"*.json 1> /dev/null 2>&1; then
    echo "拷贝JSON文件..."
    cp "$OUTPUT_ROOT/Json/"*.json "$OUTPUT_JSON/" 2>/dev/null
    echo "✓ 已拷贝到: $OUTPUT_JSON/"
else
    echo "警告: 没有找到JSON文件"
fi

# 5. 显示结果
echo ""
echo "========================================"
echo "最终输出位置:"
echo "========================================"

echo ""
echo "C#类文件位置:"
if ls "$OUTPUT_CSHARP/"*.cs 1> /dev/null 2>&1; then
    ls -1 "$OUTPUT_CSHARP/"*.cs
else
    echo "(无文件)"
fi

echo ""
echo "JSON数据文件位置:"
if ls "$OUTPUT_JSON/"*.json 1> /dev/null 2>&1; then
    ls -1 "$OUTPUT_JSON/"*.json
else
    echo "(无文件)"
fi

echo ""
echo "临时文件位置 (可手动清理):"
if ls "$OUTPUT_ROOT/Models/"*.cs 1> /dev/null 2>&1; then
    echo "C#: $OUTPUT_ROOT/Models/*.cs"
    ls -1 "$OUTPUT_ROOT/Models/"*.cs
fi
if ls "$OUTPUT_ROOT/Json/"*.json 1> /dev/null 2>&1; then
    echo "JSON: $OUTPUT_ROOT/Json/*.json"
    ls -1 "$OUTPUT_ROOT/Json/"*.json
fi

echo ""
read -p "按回车键退出..."