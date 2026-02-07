using System;
using System.Collections.Generic;
using System.Linq;
using GameConfig;
using UnityEngine;
using Random = System.Random;

public class TurretMatrixGenerator
{
    private Random random;
    private const int MinAttackCount = 1; // 最小攻击次数，可配置
    
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
    
    /// <summary>
    /// 动态补充炮台
    /// </summary>
    /// <param name="turretInfos">当前炮阵信息</param>
    /// <param name="dragonBones">剩余龙骨信息</param>
    /// <param name="difficulty">难度系数</param>
    /// <param name="totalRows">总行数</param>
    /// <returns>生成的炮台信息</returns>
    public TurretInfo GenerateTurretInfo(TurretInfo[,] turretInfos, DragonBoneInfo[] dragonBones, float difficulty, int totalRows = 6)
    {
        int rows = turretInfos.GetLength(0);
        int cols = turretInfos.GetLength(1);
        
        // 1. 分析当前炮阵前三行的攻击覆盖情况
        var existingTurretAnalysis = AnalyzeExistingTurrets(turretInfos, rows, cols);
        
        // 2. 分析剩余龙骨的威胁程度（即使龙骨信息为空也能处理）
        var boneAnalysis = AnalyzeBones(dragonBones ?? new DragonBoneInfo[0]);
        
        // 3. 计算综合优先级
        var priorityScores = CalculatePriorityScores(existingTurretAnalysis, boneAnalysis, difficulty);
        
        // 4. 根据优先级和难度生成类型
        int turretType = SelectTurretType(priorityScores, boneAnalysis, difficulty);
        
        // 5. 获取该类型的配置
        var confTurret = GetConfTurret(turretType);

        var confButtle = GetConfBullet(confTurret.BulletId);
        
        // 6. 计算攻击次数
        int attackCount = CalculateAttackCount(turretType, existingTurretAnalysis, boneAnalysis, difficulty, confTurret.MaxHitNum, confButtle.Damage);
        
        // 7. 返回炮台信息
        return new TurretInfo
        {
            Id = confTurret.Id,
            Type = turretType,
            AttackNum = attackCount
        };
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
        int num = rows * cols;
        var turretCountsByType = new Dictionary<int, int>(num);
        var turretPositionsByType = new Dictionary<int, List<(int, int)>>(num);
        
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int type = typeMatrix[r, c];
                
                if (!turretCountsByType.ContainsKey(type))
                {
                    turretCountsByType[type] = 0;
                    turretPositionsByType[type] = new List<(int, int)>(cols);
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
                var confTurret = GetConfTurret(type);

                var confBullet = GetConfBullet(confTurret.BulletId);
                
                // 计算该类型需要的总攻击次数
                // 总攻击次数 = 总生命值 / 每次攻击伤害
                int totalAttacksNeeded = (int)Math.Ceiling((float)totalHealth / confBullet.Damage);
                
                // 分配攻击次数给每个炮台
                var attackCounts = DistributeAttacksToTurrets(
                    totalAttacksNeeded, 
                    turretCount, 
                    confTurret.MaxHitNum, 
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
                        Type = type,
                        AttackNum = attackCount,
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
                        Type = type,
                        AttackNum = random.Next(MinAttackCount, config.MaxHitNum + 1)
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
            int baseCount = Mathf.Max(MinAttackCount, totalAttacksNeeded / turretCount);
            int remainder = totalAttacksNeeded % turretCount;
            
            for (int i = 0; i < turretCount; i++)
            {
                int count = baseCount + (i < remainder ? 1 : 0);
                attackCounts.Add(Mathf.Clamp(count, MinAttackCount, maxHitNum));
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
            int baseCount = Mathf.Max(MinAttackCount, totalAttacksNeeded / turretCount);
            
            for (int i = 0; i < turretCount; i++)
            {
                int variation = random.Next(-1, 2);
                int count = Mathf.Clamp(baseCount + variation, MinAttackCount, maxHitNum);
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
                int minForThis = Mathf.Max(MinAttackCount, remaining - maxHitNum * (turretCount - i - 1));
                
                int count = random.Next(minForThis, maxForThis + 1);
                attackCounts.Add(count);
                remaining -= count;
            }
            
            // 最后一个炮台获得剩余攻击次数
            attackCounts.Add(Mathf.Clamp(remaining, MinAttackCount, maxHitNum));
        }
        
        return attackCounts;
    }
    
    // 调整攻击次数列表到目标总和
    private void AdjustToTargetSum(List<int> attackCounts, int targetSum, int maxHitNum)
    {
        int currentSum = attackCounts.Sum();
        int diff = targetSum - currentSum;
        
        while (diff > 0)
        {
            int index = random.Next(attackCounts.Count);
            int change = Math.Sign(diff);
            
            int newValue = attackCounts[index] + change;
            if (newValue >= MinAttackCount && newValue <= maxHitNum)
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

    private ConfBullet GetConfBullet(int bulletId)
    {
        return ConfBullet.GetConf<ConfBullet>(bulletId);
    }
    
    // 分析当前炮阵前三行的攻击覆盖情况
    private ExistingTurretAnalysis AnalyzeExistingTurrets(TurretInfo[,] turretInfos, int rows, int cols)
    {
        var analysis = new ExistingTurretAnalysis();
        int visibleRows = Math.Min(3, rows);
        
        for (int r = 0; r < visibleRows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var turret = turretInfos[r, c];
                int type = turret.Id - 1; // 假设Id = 类型 + 1
                
                if (!analysis.TypeCounts.ContainsKey(type))
                {
                    analysis.TypeCounts[type] = 0;
                    analysis.TotalAttacksByType[type] = 0;
                }
                
                analysis.TypeCounts[type]++;
                analysis.TotalAttacksByType[type] += turret.AttackNum;
                analysis.TotalTurrets++;
            }
        }
        
        return analysis;
    }
    
    // 计算综合优先级分数
    private Dictionary<int, float> CalculatePriorityScores(ExistingTurretAnalysis turretAnalysis, BoneAnalysis boneAnalysis, float difficulty)
    {
        var priorityScores = new Dictionary<int, float>();
        
        foreach (int type in boneAnalysis.BoneTypes)
        {
            float score = 0;
            
            // 1. 龙骨威胁程度（生命值占比）
            float threatScore = boneAnalysis.TypeHealthSums.ContainsKey(type) 
                ? (float)boneAnalysis.TypeHealthSums[type] / boneAnalysis.TotalHealth 
                : 0;
            
            // 2. 炮台覆盖程度（现有炮台数量的反比）
            float coverageScore = 0;
            if (turretAnalysis.TypeCounts.ContainsKey(type) && turretAnalysis.TypeCounts[type] > 0)
            {
                coverageScore = 1.0f / (turretAnalysis.TypeCounts[type] + 1);
            }
            else
            {
                coverageScore = 1.0f; // 没有该类型炮台，优先级最高
            }
            
            // 3. 攻击能力匹配度（现有攻击能力与龙骨生命值的匹配程度）
            float attackMatchScore = 0;
            if (turretAnalysis.TotalAttacksByType.ContainsKey(type) && boneAnalysis.TypeHealthSums.ContainsKey(type))
            {
                int existingAttacks = turretAnalysis.TotalAttacksByType[type];
                int boneHealth = boneAnalysis.TypeHealthSums[type];
                attackMatchScore = 1.0f - Math.Min(1.0f, (float)existingAttacks / boneHealth);
            }
            else
            {
                attackMatchScore = 1.0f; // 没有攻击能力，优先级最高
            }
            
            // 根据难度调整权重
            float threatWeight, coverageWeight, attackWeight;
            
            if (difficulty < 0.3f)
            {
                // 简单难度：优先保证类型覆盖
                threatWeight = 0.3f;
                coverageWeight = 0.5f;
                attackWeight = 0.2f;
            }
            else if (difficulty < 0.6f)
            {
                // 中等难度：平衡考虑
                threatWeight = 0.4f;
                coverageWeight = 0.3f;
                attackWeight = 0.3f;
            }
            else if (difficulty < 0.8f)
            {
                // 困难难度：优先应对威胁
                threatWeight = 0.6f;
                coverageWeight = 0.2f;
                attackWeight = 0.2f;
            }
            else
            {
                // 极难难度：高度对抗性
                threatWeight = 0.7f;
                coverageWeight = 0.15f;
                attackWeight = 0.15f;
            }
            
            // 计算综合分数
            score = threatScore * threatWeight + coverageScore * coverageWeight + attackMatchScore * attackWeight;
            priorityScores[type] = score;
        }
        
        return priorityScores;
    }
    
    // 根据优先级和难度选择炮台类型
    private int SelectTurretType(Dictionary<int, float> priorityScores, BoneAnalysis boneAnalysis, float difficulty)
    {
        if (priorityScores.Count == 0)
        {
            return 0;
        }
        
        // 按优先级排序
        var sortedTypes = priorityScores.OrderByDescending(kv => kv.Value).ToList();
        
        if (difficulty < 0.3f)
        {
            // 简单难度：70%概率选择最高优先级，30%随机
            if (random.NextDouble() < 0.7f)
            {
                return sortedTypes[0].Key;
            }
            else
            {
                return GetRandomTypeFromList(boneAnalysis.BoneTypes);
            }
        }
        else if (difficulty < 0.6f)
        {
            // 中等难度：根据优先级概率选择
            float totalScore = sortedTypes.Sum(kv => kv.Value);
            float randomValue = (float)random.NextDouble() * totalScore;
            float cumulativeScore = 0;
            
            foreach (var kv in sortedTypes)
            {
                cumulativeScore += kv.Value;
                if (randomValue <= cumulativeScore)
                {
                    return kv.Key;
                }
            }
            return sortedTypes[0].Key;
        }
        else if (difficulty < 0.8f)
        {
            // 困难难度：60%概率选择高威胁类型，40%按优先级
            if (random.NextDouble() < 0.6f)
            {
                return boneAnalysis.MostNeededType;
            }
            else
            {
                return sortedTypes[0].Key;
            }
        }
        else
        {
            // 极难难度：80%概率选择非最高优先级，20%选择最高优先级
            if (random.NextDouble() < 0.8f && sortedTypes.Count > 1)
            {
                int index = random.Next(1, sortedTypes.Count);
                return sortedTypes[index].Key;
            }
            else
            {
                return sortedTypes[0].Key;
            }
        }
    }
    
    // 计算攻击次数
    private int CalculateAttackCount(int turretType, ExistingTurretAnalysis turretAnalysis, BoneAnalysis boneAnalysis, float difficulty, int maxHitNum, int attackValue)
    {
        int boneHealth = boneAnalysis.TypeHealthSums.ContainsKey(turretType) 
            ? boneAnalysis.TypeHealthSums[turretType] 
            : 0;
        
        int existingAttacks = turretAnalysis.TotalAttacksByType.ContainsKey(turretType)
            ? turretAnalysis.TotalAttacksByType[turretType]
            : 0;
        
        // 计算还需要多少攻击次数
        int remainingAttacksNeeded = Math.Max(MinAttackCount, boneHealth - existingAttacks);
        int attacksNeeded = (int)Math.Ceiling((float)remainingAttacksNeeded / attackValue);
        
        // 根据难度调整攻击次数
        if (difficulty < 0.3f)
        {
            // 简单难度：保证足够，但不超过最大值
            return Math.Min(attacksNeeded, maxHitNum);
        }
        else if (difficulty < 0.6f)
        {
            // 中等难度：基本保证，略有波动
            int variation = random.Next(-1, 2);
            int count = attacksNeeded + variation;
            return Math.Clamp(count, MinAttackCount, maxHitNum);
        }
        else
        {
            // 高难度：不均匀分配，增加挑战
            if (random.NextDouble() < 0.5f)
            {
                // 50%概率给较少的攻击次数
                int minAttacks = Math.Max(MinAttackCount, attacksNeeded / 2);
                return random.Next(minAttacks, Math.Min(attacksNeeded, maxHitNum) + 1);
            }
            else
            {
                // 50%概率给较多的攻击次数
                int maxAttacks = Math.Min(attacksNeeded * 2, maxHitNum);
                return random.Next(Math.Min(attacksNeeded, maxHitNum), maxAttacks + 1);
            }
        }
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

// 现有炮台分析结果
public class ExistingTurretAnalysis
{
    public Dictionary<int, int> TypeCounts = new Dictionary<int, int>();           // 每种类型的炮台数量
    public Dictionary<int, int> TotalAttacksByType = new Dictionary<int, int>();   // 每种类型的总攻击次数
    public int TotalTurrets = 0;                                                   // 总炮台数量
}

// 炮台信息结构
public struct TurretInfo
{
    public int Id;              // 炮台Id
    public int Type;            // 炮台类型
    public int AttackNum;       // 攻击次数
    // public int Col;
    // public int Row;
}

public struct TurretPos
{
    public int RowIndex;
    public int ColIndex;

    public TurretPos(int rowIndex, int colIndex)
    {
        RowIndex = rowIndex;
        ColIndex = colIndex;
    }
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
