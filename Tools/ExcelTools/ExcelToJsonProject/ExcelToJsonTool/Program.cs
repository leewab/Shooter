using System;
using System.IO;

namespace ExcelToJsonTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("    Excel 转 C#/JSON 工具");
            Console.WriteLine("========================================\n");

            try
            {
                // 处理命令行参数
                string inputPath = args.Length > 0 ? args[0] : "配置表.xlsx";
                string outputCsDir = args.Length > 1 ? args[1] : "Output/Models";
                string outputJsonDir = args.Length > 2 ? args[2] : "Output/Json";
                string namespaceStr = args.Length > 3 ? args[3] : "GameConfig";

                if (File.Exists(inputPath) && Path.GetExtension(inputPath).ToLower() == ".xlsx")
                {
                    ExportExcelFile(inputPath, outputCsDir, outputJsonDir, namespaceStr);
                }
                else
                {
                    ExportExcelDir(inputPath, outputCsDir, outputJsonDir, namespaceStr);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ 程序执行出错：{ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"详细原因：{ex.InnerException.Message}");
                }
            }
        }

        private static void ExportExcelDir(string excelDirPath, string outputCsDir, string outputJsonDir, string namespaceStr)
        {
            Console.WriteLine("参数配置:");
            Console.WriteLine($"  Excel文件夹: {excelDirPath}");
            Console.WriteLine($"  C#输出目录: {outputCsDir}");
            Console.WriteLine($"  JSON输出目录: {outputJsonDir}");
            Console.WriteLine($"  命名空间: {namespaceStr}");
            Console.WriteLine();

            if (!Directory.Exists(excelDirPath))
            {
                Console.WriteLine($"错误：找不到Excel文件夹 '{excelDirPath}'");
                WaitForExit();
                return;
            }
            
            // 创建输出目录
            Directory.CreateDirectory(outputCsDir);
            Directory.CreateDirectory(outputJsonDir);
            
            string[] excelFiles = Directory.GetFiles(excelDirPath, "*.xlsx");
            foreach (string excelFile in excelFiles)
            {
                Console.WriteLine($"\n已导出文件：{excelFile}");
                ExportExcelFile(excelFile, outputCsDir, outputJsonDir, namespaceStr);
            }
        }

        private static void ExportExcelFile(string excelFilePath, string outputCsDir, string outputJsonDir, string namespaceStr)
        {
            // 检查Excel文件
            if (!File.Exists(excelFilePath))
            {
                Console.WriteLine($"错误：找不到Excel文件 '{excelFilePath}'");
                Console.WriteLine("\n请按以下格式准备Excel文件：");
                Console.WriteLine("  第1行：字段名称（如 id, name, price）");
                Console.WriteLine("  第2行：字段类型（如 int, string, float, bool）");
                Console.WriteLine("  第3行：字段描述（注释）");
                Console.WriteLine("  第4行开始：数据行");
                Console.WriteLine("\n或将Excel文件放在程序同级目录，命名为：配置表.xlsx");
                WaitForExit();
                return;
            }

            // 执行转换
            ExcelConverter converter = new ExcelConverter();
            converter.Convert(excelFilePath, outputCsDir, outputJsonDir, namespaceStr);
        }
        

        static void WaitForExit()
        {
            Console.WriteLine("\n按任意键退出程序...");
            Console.ReadKey();
        }
    }
}