using System.Text;
using ClosedXML.Excel;
using Newtonsoft.Json;

namespace ExcelToJsonTool
{
    public class ExcelConverter
    {
        public class FieldDefinition
        {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = "string";
            public string Comment { get; set; } = string.Empty;
        }

        public class SheetInfo
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public IXLWorksheet Worksheet { get; set; }
        }
        
        public void Convert(string excelPath, string csOutputDir, string jsonOutputDir, string namespaceStr)
        {
            if (!File.Exists(excelPath))
                throw new FileNotFoundException($"Excel文件不存在：{excelPath}");
            
            // 1. 读取所有包含|分割符的Sheet表
            List<SheetInfo> sheetInfos = new List<SheetInfo>();
            
            using (var workbook = new XLWorkbook(excelPath))
            {
                foreach (var worksheet in workbook.Worksheets)
                {
                    string sheetName = worksheet.Name;
                    
                    // 检查工作表名称是否包含|分割符
                    if (sheetName.Contains('|'))
                    {
                        var parts = sheetName.Split('|', 2);
                        if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[0]))
                        {
                            sheetInfos.Add(new SheetInfo
                            {
                                Name = parts[0].Trim(),
                                Description = parts[1].Trim(),
                                Worksheet = worksheet
                            });
                        }
                    }
                }
                
                if (sheetInfos.Count == 0)
                    throw new Exception("未找到包含|分割符的工作表");
                
                // 2. 处理每个Sheet表
                foreach (var sheetInfo in sheetInfos)
                {
                    Console.WriteLine($"正在处理工作表: {sheetInfo.Name} ({sheetInfo.Description})");
                    
                    List<FieldDefinition> fieldDefs = new List<FieldDefinition>();
                    List<Dictionary<string, object>> dataRows = new List<Dictionary<string, object>>();
                    
                    var worksheet = sheetInfo.Worksheet;
                    
                    // 检查工作表是否有数据
                    if (worksheet.IsEmpty())
                    {
                        Console.WriteLine($"警告: 工作表 '{sheetInfo.Name}' 为空，跳过");
                        continue;
                    }
                    
                    // 读取字段定义（前3行）
                    int lastColumn = worksheet.LastColumnUsed().ColumnNumber();
                    
                    for (int col = 1; col <= lastColumn; col++)
                    {
                        string fieldName = worksheet.Cell(1, col).GetString().Trim();
                        if (string.IsNullOrEmpty(fieldName))
                            break; // 遇到空列停止
                        
                        string fieldType = worksheet.Cell(2, col).GetString().Trim();
                        if (string.IsNullOrEmpty(fieldType))
                            fieldType = "string";
                        
                        string fieldComment = worksheet.Cell(3, col).GetString().Trim();
                        
                        fieldDefs.Add(new FieldDefinition
                        {
                            Name = fieldName,
                            Type = fieldType,
                            Comment = fieldComment
                        });
                    }
                    
                    if (fieldDefs.Count == 0)
                    {
                        Console.WriteLine($"警告: 工作表 '{sheetInfo.Name}' 未读取到任何字段定义，跳过");
                        continue;
                    }
                    
                    // 读取数据行（从第4行开始）
                    int lastRow = worksheet.LastRowUsed().RowNumber();
                    
                    for (int row = 4; row <= lastRow; row++)
                    {
                        var rowData = new Dictionary<string, object>();
                        bool hasData = false;
                        
                        for (int i = 0; i < fieldDefs.Count; i++)
                        {
                            string cellValue = worksheet.Cell(row, i + 1).GetString().Trim();
                            object typedValue = ConvertToType(cellValue, fieldDefs[i].Type);
                            
                            if (!string.IsNullOrEmpty(cellValue))
                                hasData = true;
                            
                            rowData[fieldDefs[i].Name] = typedValue;
                        }
                        
                        if (hasData)
                            dataRows.Add(rowData);
                    }
                    
                    // 3. 生成C#类文件
                    string className = $"Conf{sheetInfo.Name}";
                    string csContent = GenerateCSharpClass(className, sheetInfo.Description, fieldDefs, namespaceStr);
                    string csFilePath = Path.Combine(csOutputDir, $"{className}.cs");
                    Directory.CreateDirectory(csOutputDir);
                    File.WriteAllText(csFilePath, csContent, Encoding.UTF8);
                    
                    // 4. 生成JSON文件
                    string jsonContent = JsonConvert.SerializeObject(dataRows, Formatting.Indented);
                    string jsonFilePath = Path.Combine(jsonOutputDir, $"{className}.json");
                    Directory.CreateDirectory(jsonOutputDir);
                    File.WriteAllText(jsonFilePath, jsonContent, Encoding.UTF8);
                    
                    Console.WriteLine($"  ✓ 生成 {className}: {fieldDefs.Count} 个字段, {dataRows.Count} 行数据");
                }
            }
        }
        
        private object ConvertToType(string value, string type)
        {
            if (string.IsNullOrEmpty(value))
            {
                return GetDefaultValueForType(type);
            }
            
            try
            {
                // 检查是否为数组类型
                if (type.EndsWith("[]"))
                {
                    string elementType = type.Substring(0, type.Length - 2);
                    return ConvertToArray(value, elementType);
                }
                
                // 检查是否为枚举类型
                if (type.StartsWith("enum(") && type.EndsWith(")"))
                {
                    // 枚举值直接存储为int，将在C#类中进行强转
                    return ConvertToEnumValue(value);
                }
                
                // 基本类型转换
                switch (type.ToLower())
                {
                    case "int":
                    case "int32":
                    case "integer":
                        return int.TryParse(value, out int intResult) ? intResult : 0;
                        
                    case "long":
                    case "int64":
                        return long.TryParse(value, out long longResult) ? longResult : 0L;
                        
                    case "float":
                    case "single":
                        return float.TryParse(value, out float floatResult) ? floatResult : 0f;
                        
                    case "double":
                        return double.TryParse(value, out double doubleResult) ? doubleResult : 0.0;
                        
                    case "bool":
                    case "boolean":
                        string lowerValue = value.ToLower();
                        if (lowerValue == "true" || lowerValue == "1" || lowerValue == "是" || lowerValue == "yes")
                            return true;
                        if (lowerValue == "false" || lowerValue == "0" || lowerValue == "否" || lowerValue == "no")
                            return false;
                        return bool.TryParse(lowerValue, out bool boolResult) && boolResult;
                        
                    default:
                        return value;
                }
            }
            catch
            {
                return GetDefaultValueForType(type);
            }
        }
        
        private object ConvertToArray(string value, string elementType)
        {
            if (string.IsNullOrEmpty(value))
                return Array.CreateInstance(GetElementType(elementType), 0);
            
            // 分割字符串，支持逗号、分号或空格分隔
            string[] parts = value.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Array array = Array.CreateInstance(GetElementType(elementType), parts.Length);
            
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i].Trim();
                array.SetValue(ConvertArrayElement(part, elementType), i);
            }
            
            return array;
        }
        
        private object ConvertArrayElement(string value, string elementType)
        {
            if (string.IsNullOrEmpty(value))
            {
                return elementType.ToLower() switch
                {
                    "int" or "int32" or "integer" => 0,
                    "long" or "int64" => 0L,
                    "float" or "single" => 0f,
                    "double" => 0.0,
                    "bool" or "boolean" => false,
                    _ => string.Empty
                };
            }
            
            switch (elementType.ToLower())
            {
                case "int":
                case "int32":
                case "integer":
                    return int.TryParse(value, out int intResult) ? intResult : 0;
                    
                case "long":
                case "int64":
                    return long.TryParse(value, out long longResult) ? longResult : 0L;
                    
                case "float":
                case "single":
                    return float.TryParse(value, out float floatResult) ? floatResult : 0f;
                    
                case "double":
                    return double.TryParse(value, out double doubleResult) ? doubleResult : 0.0;
                    
                case "bool":
                case "boolean":
                    string lowerValue = value.ToLower();
                    if (lowerValue == "true" || lowerValue == "1" || lowerValue == "是" || lowerValue == "yes")
                        return true;
                    if (lowerValue == "false" || lowerValue == "0" || lowerValue == "否" || lowerValue == "no")
                        return false;
                    return bool.TryParse(lowerValue, out bool boolResult) && boolResult;
                    
                default:
                    return value;
            }
        }
        
        private object ConvertToEnumValue(string value)
        {
            // 枚举值直接转换为int，将在C#类中强转为枚举类型
            if (int.TryParse(value, out int intValue))
                return intValue;
            
            // 如果无法解析为int，返回0
            return 0;
        }
        
        private Type GetElementType(string elementType)
        {
            return elementType.ToLower() switch
            {
                "int" or "int32" or "integer" => typeof(int),
                "long" or "int64" => typeof(long),
                "float" or "single" => typeof(float),
                "double" => typeof(double),
                "bool" or "boolean" => typeof(bool),
                _ => typeof(string)
            };
        }
        
        private object GetDefaultValueForType(string type)
        {
            // 检查是否为数组类型
            if (type.EndsWith("[]"))
            {
                string elementType = type.Substring(0, type.Length - 2);
                return Array.CreateInstance(GetElementType(elementType), 0);
            }
            
            // 检查是否为枚举类型
            if (type.StartsWith("enum(") && type.EndsWith(")"))
            {
                return 0; // 枚举默认值为0
            }
            
            return type.ToLower() switch
            {
                "int" or "int32" or "integer" => 0,
                "long" or "int64" => 0L,
                "float" or "single" => 0f,
                "double" => 0.0,
                "bool" or "boolean" => false,
                _ => string.Empty
            };
        }
        
        private string GenerateCSharpClass(string className, string description, List<FieldDefinition> fields, string namespaceStr)
        {
            StringBuilder sb = new StringBuilder();
            
            // 文件头注释
            sb.AppendLine("// ===========================================");
            sb.AppendLine("// 自动生成的C#配置类");
            sb.AppendLine($"// 表名称: {className}");
            sb.AppendLine($"// 表描述: {description}");
            sb.AppendLine($"// 生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("// 工具: ExcelToJsonTool");
            sb.AppendLine("// 请勿手动修改此文件，重新生成将被覆盖");
            sb.AppendLine("// ===========================================");
            sb.AppendLine();
            
            // using语句
            sb.AppendLine("using Newtonsoft.Json;");
            sb.AppendLine("using System;");
            sb.AppendLine();
            
            // 命名空间和类
            sb.AppendLine($"namespace {namespaceStr}");
            sb.AppendLine("{");
            
            // 添加表描述注释
            if (!string.IsNullOrEmpty(description))
            {
                sb.AppendLine("    /// <summary>");
                sb.AppendLine($"    /// {description}");
                sb.AppendLine("    /// </summary>");
            }
            
            sb.AppendLine($"    [Serializable]");
            sb.AppendLine($"    public class {className}  : BaseConf");
            sb.AppendLine("    {");
            
            // 生成属性字段
            foreach (var field in fields)
            {
                if (field.Name == "Id") continue;
                
                string csharpType = GetCSharpType(field.Type);
                string defaultValue = GetDefaultCSharpValue(field.Type);
                
                // 字段注释
                if (!string.IsNullOrEmpty(field.Comment))
                {
                    sb.AppendLine("        /// <summary>");
                    sb.AppendLine($"        /// {field.Comment}");
                    sb.AppendLine("        /// </summary>");
                }
                
                // JSON序列化属性和C#属性
                sb.AppendLine($"        [JsonProperty(\"{field.Name}\")]");
                
                // 对于枚举类型，需要添加JsonConverter
                if (field.Type.StartsWith("enum(") && field.Type.EndsWith(")"))
                {
                    string enumType = field.Type.Substring(5, field.Type.Length - 6);
                    sb.AppendLine($"        public {enumType} {field.Name} {{ get; set; }} = ({enumType}){defaultValue};");
                }
                else
                {
                    sb.AppendLine($"        public {csharpType} {field.Name} {{ get; set; }} = {defaultValue};");
                }
                
                // 每个属性后空一行
                sb.AppendLine();
            }
            
            // ToString方法用于调试
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 返回对象的字符串表示");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public override string ToString()");
            sb.AppendLine("        {");
            
            if (fields.Count > 0)
            {
                List<string> propertyStrings = new List<string>();
                foreach (var field in fields)
                {
                    propertyStrings.Add($"$\"{field.Name}={{{field.Name}}}\"");
                }
                sb.AppendLine($"            return $\"{className} \" + string.Join(\", \", new string[] {{ {string.Join(", ", propertyStrings)} }});");
            }
            else
            {
                sb.AppendLine("            return $\"{className} (无字段)\";");
            }
            
            sb.AppendLine("        }");
            
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            return sb.ToString();
        }
        
        private string GetCSharpType(string excelType)
        {
            // 检查是否为数组类型
            if (excelType.EndsWith("[]"))
            {
                string elementType = excelType.Substring(0, excelType.Length - 2);
                string csharpElementType = GetCSharpType(elementType);
                return $"{csharpElementType}[]";
            }
            
            // 检查是否为枚举类型
            if (excelType.StartsWith("enum(") && excelType.EndsWith(")"))
            {
                return excelType.Substring(5, excelType.Length - 6);
            }
            
            return excelType.ToLower() switch
            {
                "int" or "int32" or "integer" => "int",
                "long" or "int64" => "long",
                "float" or "single" => "float",
                "double" => "double",
                "bool" or "boolean" => "bool",
                _ => "string"
            };
        }
        
        private string GetDefaultCSharpValue(string excelType)
        {
            // 检查是否为数组类型
            if (excelType.EndsWith("[]"))
            {
                return "new " + GetCSharpType(excelType) + " { }";
            }
            
            // 检查是否为枚举类型
            if (excelType.StartsWith("enum(") && excelType.EndsWith(")"))
            {
                return "0";
            }
            
            return excelType.ToLower() switch
            {
                "int" or "int32" or "integer" => "0",
                "long" or "int64" => "0L",
                "float" or "single" => "0f",
                "double" => "0.0",
                "bool" or "boolean" => "false",
                _ => "string.Empty"
            };
        }
    }
}