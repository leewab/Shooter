@echo off
chcp 65001 >nul
cd /d "%~dp0"
title Excel 批量导出工具

echo ========================================
echo   Excel 批量导出工具
echo ========================================
echo.

REM 配置参数
set INPUT_DIR=.
set OUTPUT_ROOT=Output
set NAMESPACE=GameConfig
set EXETOOL_DIR=bin\Release\net6.0\ExcelToJsonTool.exe

REM Unity项目输出路径
set OUTPUT_CSHARP=..\..\Client\Assets\Game\Scripts\Runtime\Conf
set OUTPUT_JSON=..\..\Res\Assets\Product\Conf

REM 1. 检查工具是否存在
if not exist "%EXETOOL_DIR%" (
    echo 错误: 找不到导出工具 %EXETOOL_DIR%
    pause
    exit /b 1
)
echo ✓ 工具就绪

REM 2. 创建输出目录
echo.
echo 创建输出目录...
if exist "%OUTPUT_ROOT%\Models" rmdir "%OUTPUT_ROOT%\Models" /s /q
if exist "%OUTPUT_ROOT%\Json" rmdir "%OUTPUT_ROOT%\Json" /s /q
mkdir "%OUTPUT_ROOT%\Models"
mkdir "%OUTPUT_ROOT%\Json"

REM 3. 处理所有Excel文件
echo.
echo 开始处理Excel文件...
echo.

echo 正在处理... %INPUT_DIR%
"%EXETOOL_DIR%" "%INPUT_DIR%" "%OUTPUT_ROOT%\Models" "%OUTPUT_ROOT%\Json" "%NAMESPACE%"
if errorlevel 1 (
	echo [失败] %%~nxf
) else (
	echo [成功] %%~nxf
)
echo.

REM 4. 拷贝到Unity项目
echo.
echo 拷贝文件到Unity项目...

REM 创建Unity目录（如果不存在）
if exist "%OUTPUT_CSHARP%" rmdir "%OUTPUT_CSHARP%" /s /q
if exist "%OUTPUT_JSON%" rmdir "%OUTPUT_JSON%" /s /q
mkdir "%OUTPUT_CSHARP%"
mkdir "%OUTPUT_JSON%"

REM 拷贝C#文件
if exist "%OUTPUT_ROOT%\Models\*.cs" (
    echo 拷贝C#文件...
    copy "%OUTPUT_ROOT%\Models\*.cs" "%OUTPUT_CSHARP%\" >nul
    echo ✓ 已拷贝到: %OUTPUT_CSHARP%\
) else (
    echo 警告: 没有找到C#文件
)

REM 拷贝JSON文件
if exist "%OUTPUT_ROOT%\Json\*.json" (
    echo 拷贝JSON文件...
    copy "%OUTPUT_ROOT%\Json\*.json" "%OUTPUT_JSON%\" >nul
    echo ✓ 已拷贝到: %OUTPUT_JSON%\
) else (
    echo 警告: 没有找到JSON文件
)

REM 5. 显示结果
echo.
echo ========================================
echo 最终输出位置:
echo ========================================

echo.
echo C#类文件位置:
dir /b "%OUTPUT_CSHARP%\*.cs" 2>nul || echo (无文件)

echo.
echo JSON数据文件位置:
dir /b "%OUTPUT_JSON%\*.json" 2>nul || echo (无文件)

echo.
echo 临时文件位置 (可手动清理):
dir /b "%OUTPUT_ROOT%\Models\*.cs" 2>nul && echo C#: %OUTPUT_ROOT%\Models\*.cs
dir /b "%OUTPUT_ROOT%\Json\*.json" 2>nul && echo JSON: %OUTPUT_ROOT%\Json\*.json

echo.
pause