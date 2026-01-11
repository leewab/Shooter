using System;
using System.Collections.Generic;
using System.Linq;
using Framework.UIFramework;
using Gameplay;
using UnityEngine;

/// <summary>
/// 炮台实体类（存储核心状态和信息）
/// </summary>
public class TurretData
{
    // 唯一标识ID
    public int Index { get; set; }
    // 配置ID
    public int Id { get; set; }
    // 是否存活（未被消除）
    public bool IsAlive { get; set; } 
    // 所在列（0/1/2，固定3列）
    public int Column { get; set; } 
    // 所在列内的位置索引（前置→后置：0→n-1）
    public int PositionIndex { get; set; } 

    public TurretData(int index, int id, int column, int positionIndex)
    {
        Index = index;
        Id = id;
        Column = column;
        PositionIndex = positionIndex;
        IsAlive = true; // 初始化默认存活
    }
}

/// <summary>
/// 3×n炮台网格管理器（承载核心算法）
/// </summary>
public class TurretHandler : SingletonMono<TurretHandler>
{
    [SerializeField] private TurretSeat[] _turretSeatList;
    [SerializeField] private TurretsGrid _turretsGrid;
    
    // 0解锁 1锁死
    private int[] _turretSeatLock = new[] { 0, 0, 0, 0, 0 };
    
    // 固定横向3列
    private int _columnCount = 3; 
    // 竖向行数n（每列最大炮台数量）
    private int _rowCount = 10; 
    // 核心网格数据：[列索引][列内炮台列表]，保证每列独立管理、补位
    private List<List<TurretData>> _turretDataList;

    // 炮台移除
    public event Action<int> OnRefreshTurret;
 
    private void Update()
    {
        OnRaycastClick();
    }
    
    private void InitTurretSeat()
    {
        if (_turretSeatList == null) return;
        for (int i = 0; i < _turretSeatLock.Length; i++)
        {
            _turretSeatList[i].SetActive(_turretSeatLock[i] == 0);
        }
    }

    /// <summary>
    /// 初始化生成3×n炮台网格（核心初始化算法）
    /// </summary>
    /// <param name="rowCount">竖向行数n（每列炮台最大数量，必须>0）</param>
    private void InitTurretGrid()
    {
        // 1. 参数校验
        if (_rowCount <= 0)
        {
            throw new ArgumentException("竖向行数n必须大于0", nameof(_rowCount));
        }

        int index = 0;
        _turretDataList = new List<List<TurretData>>(_columnCount);
        for (int column = 0; column < _columnCount; column++)
        {
            List<TurretData> columnTurrets = new List<TurretData>(_rowCount);
            for (int positionIndex = 0; positionIndex < _rowCount; positionIndex++)
            {
                int randomTurretId = TurretManager.Instance.GetRandomTurretId();
                TurretData newTurretData = new TurretData(
                    index: index++,
                    id:randomTurretId,
                    column: column,
                    positionIndex: positionIndex
                );
                columnTurrets.Add(newTurretData);
            }

            _turretDataList.Add(columnTurrets);
        }

        _turretsGrid.InitializeTurrets(_turretDataList);
    }

    /// <summary>
    /// 点击消除指定炮台（核心消除算法）
    /// </summary>
    /// <param name="targetTurretData">待消除的目标炮台</param>
    /// <returns>是否消除成功</returns>
    public bool EliminateTurret(TurretData targetTurretData)
    {
        // 1. 基础参数校验
        if (targetTurretData == null || !targetTurretData.IsAlive)
        {
            return false;
        }

        int column = targetTurretData.Column;
        int targetPos = targetTurretData.PositionIndex;

        // 2. 校验列的合法性（0~2）
        if (column < 0 || column >= _columnCount)
        {
            return false;
        }

        List<TurretData> columnTurrets = _turretDataList[column];

        // 3. 关键校验：目标炮台是否为「当前列有效前置」（无存活炮台在其更前方）
        bool isFrontValid = true;
        for (int pos = 0; pos < targetPos; pos++)
        {
            if (columnTurrets[pos].IsAlive)
            {
                // 目标炮台前方有存活炮台，不属于可消除的「前置炮台」
                isFrontValid = false;
                break;
            }
        }

        if (!isFrontValid)
        {
            return false;
        }

        // 4. 标记炮台为已消除（非销毁数据，为补位保留结构）
        targetTurretData.IsAlive = false;

        // 5. 触发当前列的自动补位逻辑
        AutoFillColumn(column);

        return true;
    }

    /// <summary>
    /// 指定列的炮台自动向前补位（核心补位算法）
    /// </summary>
    /// <param name="column">待补位的列索引（0~2）</param>
    private void AutoFillColumn(int column)
    {
        // 1. 校验列的合法性
        if (column < 0 || column >= _columnCount)
        {
            return;
        }

        List<TurretData> columnTurrets = _turretDataList[column];

        // 步骤1：筛选当前列所有存活的炮台（后置炮台集合）
        List<TurretData> aliveTurrets = columnTurrets.Where(t => t.IsAlive).ToList();

        // 步骤2：重置当前列所有炮台状态（先标记为全部未存活、清空位置）
        foreach (TurretData turret in columnTurrets)
        {
            turret.IsAlive = false;
        }

        // 步骤3：后置存活炮台向前补位，填充到列的最前方（从positionIndex=0开始）
        for (int newPos = 0; newPos < aliveTurrets.Count; newPos++)
        {
            TurretData fillTurretData = aliveTurrets[newPos];
            // 更新炮台的实际位置索引（向前补位）
            fillTurretData.PositionIndex = newPos;
            // 标记为存活，完成补位
            columnTurrets[newPos] = fillTurretData;
            columnTurrets[newPos].IsAlive = true;
        }

        // 扩展：同步更新视觉视图（如移动炮台的坐标位置，对应新的positionIndex）
        // SyncColumnViewToGrid(column);
        OnRefreshTurret?.Invoke(column);
    }
    
    private void OnRaycastClick()
    {
        if (Input.GetMouseButtonDown(0)) // 鼠标左键点击
        {
            // 将屏幕点击位置转换为世界坐标
            Vector3 clickPosition = UIRoot.UICamera.ScreenToWorldPoint(Input.mousePosition);
            
            // 创建2D射线
            RaycastHit2D hit = Physics2D.Raycast(clickPosition, Vector2.zero);
            
            if (hit.collider != null && hit.collider.gameObject.CompareTag("Turret"))
            {
                Debug.Log($"点击到: {hit.collider.gameObject.name}");
                OnClickTurret(hit.collider.gameObject);
            }
        }
    }

    private void OnClickTurret(GameObject turretObj)
    {
        TurretEntity turret = turretObj.GetComponent<TurretEntity>();
        if (turret != null)
        {
            if (!turret.IsFirst)
            {
                Debug.LogError("当前不是第一排炮塔！");
                return;
            }

            if (turret.IsActive)
            {
                Debug.LogError("当前炮塔已被激活！");
                return;
            }
            
            var seat = GetTurretSeat();
            if (seat != null)
            {
                seat.SetupTurret(turret);
            }
        }
    }

    
    /// <summary>
    /// 清空指定列的所有炮台（重置状态）
    /// </summary>
    private void ClearColumn(int column)
    {
        if (_turretDataList == null) return;
        if (column < 0 || column >= _columnCount) return;
        foreach (TurretData turret in _turretDataList[column])
        {
            turret.IsAlive = false;
        }
    }
    

    public void InitTurret()
    {
        InitTurretGrid();
        InitTurretSeat();
    }
    
    /// <summary>
    /// 清空整个3×n网格的炮台（重置状态）
    /// </summary>
    public void ClearAllGrid()
    {
        for (int column = 0; column < _columnCount; column++)
        {
            ClearColumn(column);
        }
    }

    /// <summary>
    /// 查询指定列的当前最前置存活炮台
    /// </summary>
    public TurretData GetFrontTurret(int column)
    {
        if (column < 0 || column >= _columnCount) return null;
        foreach (TurretData turret in _turretDataList[column])
        {
            if (turret.IsAlive)
            {
                // 第一个存活的即为最前置
                return turret; 
            }
        }

        // 该列无存活炮台
        return null; 
    }

    public void ClearTurret()
    {
        if (_turretDataList != null)
        {
            foreach (var turretList in _turretDataList)
            {
                turretList.Clear();
            }
            _turretDataList.Clear();
            _turretDataList = null;
        }

        if (_turretsGrid != null)
        {
            _turretsGrid.ClearTurrets();
        }

        // _turretSeatList = null;
        // _turretsGrid = null;
    }
    
    public TurretSeat GetTurretSeat()
    {
        for (int i = 0; i < _turretSeatList.Length; i++)
        {
            if (!_turretSeatList[i].IsOccupy && _turretSeatList[i].IsActive) return _turretSeatList[i];
        }
        
        Debug.LogError("炮台已经满了");
        return null;
    }
    
}