using System;
using System.Collections.Generic;
using System.Linq;
using GameConfig;
using UnityEngine;
using Random = System.Random;

public class TurretMatrixGenerator
{
    private Random random;
    
    public TurretMatrixGenerator(int seed = 0)
    {
        random = seed == 0 ? new Random() : new Random(seed);
    }
    
    public TurretInfo[,] GenerateTurretMatrix(DragonBoneInfo[] dragonBones, float difficulty, int totalRows = 6)
    {
        int cols = 3;
        int[,] typeMatrix = new int[totalRows, cols];
        
        // 1. 分析龙骨信息（考虑生命值）
        var boneAnalysis = AnalyzeBones(dragonBones);
        
        // 2. 根据难度生成类型矩阵
        GenerateTypeMatrix(typeMatrix, boneAnalysis, difficulty);
        
        // 3. 为每种类型炮台分配攻击次数（考虑攻击力和龙骨生命值）
        var turretMatrix = AssignAttackCounts(typeMatrix, boneAnalysis, difficulty);
        
        return turretMatrix;
    }
    
    // 分析龙骨信息，考虑生命值
    private BoneAnalysis AnalyzeBones(DragonBoneInfo[] dragonBones)
    {
        var analysis = new BoneAnalysis();
        
        foreach (var bone in dragonBones)
        {
            int type = bone.Type;
            int health = bone.Health;
            
            if (!analysis.TypeHealthSums.ContainsKey(type))
            {
                analysis.TypeHealthSums[type] = 0;
                analysis.TypeCounts[type] = 0;
                analysis.BoneTypes.Add(type);
            }
            
            analysis.TypeHealthSums[type] += health;  // 累加生命值
            analysis.TypeCounts[type]++;              // 累加数量
            analysis.TotalHealth += health;           // 总生命值
        }
        
        // 找出生命值最多的类型（最需要攻击的类型）
        analysis.MostNeededType = analysis.TypeHealthSums
            .OrderByDescending(kv => kv.Value)
            .First().Key;
        
        // 找出生命值最少的类型
        analysis.LeastNeededType = analysis.TypeHealthSums
            .OrderBy(kv => kv.Value)
            .First().Key;
        
        return analysis;
    }
    
    // 生成类型矩阵
    private void GenerateTypeMatrix(int[,] matrix, BoneAnalysis boneAnalysis, float difficulty)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        int visibleRows = 3;
        
        // 策略：简单难度确保类型覆盖，高难度制造困难
        if (difficulty < 0.3f)
        {
            // 简单难度：确保每种类型都有炮台
            GenerateForEasyDifficulty(matrix, boneAnalysis);
        }
        else if (difficulty < 0.6f)
        {
            // 中等难度：按比例生成
            GenerateForMediumDifficulty(matrix, boneAnalysis, difficulty);
        }
        else if (difficulty < 0.8f)
        {
            // 困难难度：偏向稀有类型
            GenerateForHardDifficulty(matrix, boneAnalysis, difficulty);
        }
        else
        {
            // 极难难度：对抗性生成
            GenerateForExtremeDifficulty(matrix, boneAnalysis, difficulty);
        }
        
        // 简单难度下，二次检查确保覆盖
        if (difficulty < 0.3f)
        {
            EnsureTypeCoverage(matrix, boneAnalysis);
        }
    }
    
    // 简单难度：专业化列
    private void GenerateForEasyDifficulty(int[,] matrix, BoneAnalysis boneAnalysis)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        
        // 为每列分配一个主要类型
        var typesByHealth = boneAnalysis.TypeHealthSums
            .OrderByDescending(kv => kv.Value)
            .Select(kv => kv.Key)
            .ToList();
        
        for (int col = 0; col < cols; col++)
        {
            int mainType = col < typesByHealth.Count ? typesByHealth[col] : typesByHealth[0];
            
            // 该列至少60%的炮台是主要类型
            for (int row = 0; row < rows; row++)
            {
                if (row < rows * 0.6f || random.NextDouble() < 0.7f)
                {
                    matrix[row, col] = mainType;
                }
                else
                {
                    matrix[row, col] = GetRandomTypeFromList(boneAnalysis.BoneTypes);
                }
            }
        }
    }
    
    // 中等难度：按比例生成
    private void GenerateForMediumDifficulty(int[,] matrix, BoneAnalysis boneAnalysis, float difficulty)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        int totalCells = rows * cols;
        
        // 根据生命值比例计算每种类型需要的炮台数量
        var turretCountsByType = new Dictionary<int, int>();
        int remainingCells = totalCells;
        
        foreach (var kv in boneAnalysis.TypeHealthSums)
        {
            int type = kv.Key;
            float healthRatio = (float)kv.Value / boneAnalysis.TotalHealth;
            int count = Math.Max(1, (int)(totalCells * healthRatio * (1.2f - difficulty)));
            turretCountsByType[type] = count;
            remainingCells -= count;
        }
        
        // 分配剩余的格子
        if (remainingCells > 0)
        {
            var types = boneAnalysis.BoneTypes.ToList();
            while (remainingCells > 0)
            {
                int type = types[random.Next(types.Count)];
                turretCountsByType[type]++;
                remainingCells--;
            }
        }
        
        // 填充矩阵
        var positions = GetAllPositions(rows, cols);
        ShuffleList(positions);
        
        foreach (var kv in turretCountsByType)
        {
            int type = kv.Key;
            int count = kv.Value;
            
            for (int i = 0; i < count && positions.Count > 0; i++)
            {
                var pos = positions[0];
                positions.RemoveAt(0);
                matrix[pos.Item1, pos.Item2] = type;
            }
        }
        
        // 填充剩余位置
        foreach (var pos in positions)
        {
            matrix[pos.Item1, pos.Item2] = GetRandomTypeFromList(boneAnalysis.BoneTypes);
        }
    }
    
    // 困难难度：偏向稀有类型
    private void GenerateForHardDifficulty(int[,] matrix, BoneAnalysis boneAnalysis, float difficulty)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                // 60%概率生成稀有类型，40%随机
                if (random.NextDouble() < 0.6f)
                {
                    matrix[r, c] = boneAnalysis.LeastNeededType;
                }
                else
                {
                    matrix[r, c] = GetRandomTypeFromList(boneAnalysis.BoneTypes);
                }
            }
        }
    }
    
    // 极难难度：对抗性生成
    private void GenerateForExtremeDifficulty(int[,] matrix, BoneAnalysis boneAnalysis, float difficulty)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                // 80%概率不给最需要的类型
                if (random.NextDouble() < 0.8f)
                {
                    matrix[r, c] = GetRandomTypeExcept(boneAnalysis.BoneTypes, boneAnalysis.MostNeededType);
                }
                else
                {
                    matrix[r, c] = boneAnalysis.MostNeededType;
                }
            }
        }
    }
    
    // 确保类型覆盖
    private void EnsureTypeCoverage(int[,] matrix, BoneAnalysis boneAnalysis)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        
        // 统计现有类型
        var existingTypes = new HashSet<int>();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                existingTypes.Add(matrix[r, c]);
            }
        }
        
        // 检查是否缺少某些类型
        foreach (int type in boneAnalysis.BoneTypes)
        {
            if (!existingTypes.Contains(type))
            {
                // 在随机位置添加缺少的类型
                int attempts = 0;
                while (attempts < 10)
                {
                    int r = random.Next(rows);
                    int c = random.Next(cols);
                    
                    // 替换一个非关键位置
                    if (matrix[r, c] != boneAnalysis.MostNeededType)
                    {
                        matrix[r, c] = type;
                        break;
                    }
                    attempts++;
                }
            }
        }
    }
    
    // 为炮台分配攻击次数（核心算法）
    private TurretInfo[,] AssignAttackCounts(int[,] typeMatrix, BoneAnalysis boneAnalysis, float difficulty)
    {
        int rows = typeMatrix.GetLength(0);
        int cols = typeMatrix.GetLength(1);
        TurretInfo[,] result = new TurretInfo[rows, cols];
        
        // 统计每种类型的炮台数量
        var turretCountsByType = new Dictionary<int, int>();
        var turretPositionsByType = new Dictionary<int, List<(int, int)>>();
        
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int type = typeMatrix[r, c];
                
                if (!turretCountsByType.ContainsKey(type))
                {
                    turretCountsByType[type] = 0;
                    turretPositionsByType[type] = new List<(int, int)>();
                }
                
                turretCountsByType[type]++;
                turretPositionsByType[type].Add((r, c));
            }
        }
        
        // 为每种类型分配攻击次数
        var attackCountsByType = new Dictionary<int, List<int>>();
        
        foreach (int type in boneAnalysis.BoneTypes)
        {
            if (turretCountsByType.ContainsKey(type))
            {
                int turretCount = turretCountsByType[type];
                int totalHealth = boneAnalysis.TypeHealthSums[type];
                
                // 获取该类型炮台的配置
                var config = GetConfTurret(type);
                
                // 计算该类型需要的总攻击次数
                // 总攻击次数 = 总生命值 / 每次攻击伤害
                int totalAttacksNeeded = (int)Math.Ceiling((float)totalHealth / config.AttackValue);
                
                // 分配攻击次数给每个炮台
                var attackCounts = DistributeAttacksToTurrets(
                    totalAttacksNeeded, 
                    turretCount, 
                    config.MaxHitNum, 
                    difficulty
                );
                
                attackCountsByType[type] = attackCounts;
            }
        }
        
        // 填充结果矩阵
        foreach (var kv in turretPositionsByType)
        {
            int type = kv.Key;
            var positions = kv.Value;
            
            if (attackCountsByType.ContainsKey(type))
            {
                var attackCounts = attackCountsByType[type];
                
                for (int i = 0; i < positions.Count; i++)
                {
                    int r = positions[i].Item1;
                    int c = positions[i].Item2;
                    
                    var config = GetConfTurret(type);
                    int attackCount = i < attackCounts.Count ? attackCounts[i] : 1;
                    
                    result[r, c] = new TurretInfo
                    {
                        Id = config.Id,
                        CurrentHitNum = attackCount
                    };
                }
            }
            else
            {
                // 没有该类型的龙骨，分配默认攻击次数
                foreach (var pos in positions)
                {
                    var config = GetConfTurret(type);
                    result[pos.Item1, pos.Item2] = new TurretInfo
                    {
                        Id = config.Id,
                        CurrentHitNum = random.Next(1, config.MaxHitNum + 1)
                    };
                }
            }
        }
        
        return result;
    }
    
    // 分配攻击次数给多个炮台（考虑难度）
    private List<int> DistributeAttacksToTurrets(int totalAttacksNeeded, int turretCount, int maxHitNum, float difficulty)
    {
        List<int> attackCounts = new List<int>();
        
        if (turretCount <= 0) return attackCounts;
        
        // 简单难度：均匀分配，保证足够
        if (difficulty < 0.3f)
        {
            int baseCount = Mathf.Max(1, totalAttacksNeeded / turretCount);
            int remainder = totalAttacksNeeded % turretCount;
            
            for (int i = 0; i < turretCount; i++)
            {
                int count = baseCount + (i < remainder ? 1 : 0);
                attackCounts.Add(Mathf.Clamp(count, 1, maxHitNum));
            }
            
            // 检查总和是否足够，如果不够，增加某些炮台的攻击次数
            int currentSum = attackCounts.Sum();
            if (currentSum < totalAttacksNeeded)
            {
                int needed = totalAttacksNeeded - currentSum;
                for (int i = 0; i < needed; i++)
                {
                    int index = i % turretCount;
                    if (attackCounts[index] < maxHitNum)
                    {
                        attackCounts[index]++;
                    }
                }
            }
        }
        // 中等难度：基本均匀，略有波动
        else if (difficulty < 0.6f)
        {
            int baseCount = Mathf.Max(1, totalAttacksNeeded / turretCount);
            
            for (int i = 0; i < turretCount; i++)
            {
                int variation = random.Next(-1, 2);
                int count = Mathf.Clamp(baseCount + variation, 1, maxHitNum);
                attackCounts.Add(count);
            }
            
            // 调整到目标总和
            AdjustToTargetSum(attackCounts, totalAttacksNeeded, maxHitNum);
        }
        // 高难度：不均匀分配，制造挑战
        else
        {
            int remaining = totalAttacksNeeded;
            
            for (int i = 0; i < turretCount - 1; i++)
            {
                // 计算当前炮台的最大可能攻击次数
                int maxForThis = Mathf.Min(remaining - (turretCount - i - 1), maxHitNum);
                int minForThis = Mathf.Max(1, remaining - maxHitNum * (turretCount - i - 1));
                
                int count = random.Next(minForThis, maxForThis + 1);
                attackCounts.Add(count);
                remaining -= count;
            }
            
            // 最后一个炮台获得剩余攻击次数
            attackCounts.Add(Mathf.Clamp(remaining, 1, maxHitNum));
        }
        
        return attackCounts;
    }
    
    // 调整攻击次数列表到目标总和
    private void AdjustToTargetSum(List<int> attackCounts, int targetSum, int maxHitNum)
    {
        int currentSum = attackCounts.Sum();
        int diff = targetSum - currentSum;
        
        while (diff != 0)
        {
            int index = random.Next(attackCounts.Count);
            int change = Math.Sign(diff);
            
            int newValue = attackCounts[index] + change;
            if (newValue >= 1 && newValue <= maxHitNum)
            {
                attackCounts[index] = newValue;
                diff -= change;
            }
        }
    }
    
    // 工具方法：获取所有位置
    private List<(int, int)> GetAllPositions(int rows, int cols)
    {
        var positions = new List<(int, int)>();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                positions.Add((r, c));
            }
        }
        return positions;
    }
    
    // 工具方法：从列表中随机选择类型
    private int GetRandomTypeFromList(HashSet<int> types)
    {
        if (types.Count == 0) return 0;
        
        int index = random.Next(types.Count);
        return types.ElementAt(index);
    }
    
    // 工具方法：获取除了指定类型外的随机类型
    private int GetRandomTypeExcept(HashSet<int> types, int excludeType)
    {
        var availableTypes = new List<int>();
        foreach (int type in types)
        {
            if (type != excludeType)
                availableTypes.Add(type);
        }
        
        if (availableTypes.Count == 0)
            return random.Next(4); // 0-3随机
        
        return availableTypes[random.Next(availableTypes.Count)];
    }
    
    // 工具方法：打乱列表
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
    
    // 获取炮台配置
    private ConfTurret GetConfTurret(int type)
    {
        // 调用你的配置系统
        int configId = type + 1; // 假设配置ID = 类型 + 1
        return ConfTurret.GetConf<ConfTurret>(configId);
    }
}

// 龙骨分析结果
public class BoneAnalysis
{
    public Dictionary<int, int> TypeHealthSums = new Dictionary<int, int>(); // 每种类型的总生命值
    public Dictionary<int, int> TypeCounts = new Dictionary<int, int>();     // 每种类型的数量
    public HashSet<int> BoneTypes = new HashSet<int>();                     // 所有存在的类型
    public int TotalHealth = 0;                                             // 所有龙骨总生命值
    public int MostNeededType = 0;                                          // 生命值最多的类型
    public int LeastNeededType = 0;                                         // 生命值最少的类型
}

// 炮台信息结构
public struct TurretInfo
{
    public int Id;           // 炮台ID
    public int CurrentHitNum;// 当前剩余攻击次数
}

// 龙骨信息结构
public struct DragonBoneInfo
{
    public int Type;
    public int Health;
    
    public DragonBoneInfo(int type, int health)
    {
        Type = type;
        Health = health;
    }
}

// 炮台配置
// public class ConfTurret
// {
//     public int Id;
//     public int AttackValue;
//     public int MaxHitNum;
//     
//     public static ConfTurret GetConf<T>(int id)
//     {
//         // 这里应该调用你的配置系统
//         return new ConfTurret
//         {
//             Id = id,
//             AttackValue = 1,
//             MaxHitNum = 3
//         };
//     }
// }