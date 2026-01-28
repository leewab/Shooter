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
    public int Col { get; set; } 
    // 所在列内的位置索引（前置→后置：0→n-1）
    public int Row { get; set; } 
    public TurretInfo TurretInfo { get; set; }

    public TurretData(int index, int id, int col, int row, TurretInfo turretInfo)
    {
        Index = index;
        Id = id;
        Col = col;
        Row = row;
        IsAlive = true; // 初始化默认存活
        TurretInfo = turretInfo;
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
    private LayerMask targetLayer;
    // 核心网格数据：[列索引][列内炮台列表]，保证每列独立管理、补位
    private TurretData[,] _turretDataList;

    // 炮台移除
    public event Action<int> OnRefreshTurret;

    protected override void Awake()
    {
        base.Awake();
        targetLayer = LayerMask.GetMask("Game");
    }

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
    
    private void InitTurretGrid()
    {
        // 1. 参数校验
        if (_rowCount <= 0)
        {
            throw new ArgumentException("竖向行数n必须大于0", nameof(_rowCount));
        }

        int index = 0;
        var dragonArr = DragonManager.Instance.GetDragonBoneType();
        var difficulty = LevelManager.Instance.GetCurrentDifficulty();
        var turretMatrixGenerator = new TurretMatrixGenerator();
        var turretMatrix = turretMatrixGenerator.GenerateTurretMatrix(dragonArr, difficulty);
        var rowLength = turretMatrix.GetLength(0);
        var colLength = turretMatrix.GetLength(1);
        _turretDataList = new TurretData[rowLength,colLength];
        for (int i = 0; i < rowLength; i++)
        {
            for (int j = 0; j < colLength; j++)
            {
                var turretInfo = turretMatrix[i, j];
                TurretData newTurretData = new TurretData(
                    index: index++,
                    id:turretInfo.Id,
                    col: j,
                    row: i,
                    turretInfo: turretInfo
                );
                _turretDataList[i, j] = newTurretData;
            }
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
        
        int rowLength = _turretDataList.GetLength(0);
        int colLength = _turretDataList.GetLength(1);

        int colIndex = targetTurretData.Col;
        int rowIndex = targetTurretData.Row;

        // 2. 校验列的合法性（0~2）
        if (colIndex < 0 || colIndex >= colLength)
        {
            return false;
        }

        if (rowIndex != 0)
        {
            return false;
        }

        // 4. 标记炮台为已消除（非销毁数据，为补位保留结构）
        targetTurretData.IsAlive = false;

        // 5. 触发当前列的自动补位逻辑
        int index = 0;
        for (int i = 0; i < rowLength; i++)
        {
            var columnTurrets = _turretDataList[i, colIndex];
            if (columnTurrets != null && columnTurrets.IsAlive)
            {
                columnTurrets.Row = index++;
            }
        }
        
        OnRefreshTurret?.Invoke(colIndex);
        return true;
    }
    
    private void OnRaycastClick()
    {
        if (Input.GetMouseButtonDown(0)) // 鼠标左键点击
        {
            // 将屏幕点击位置转换为世界坐标
            if (Camera.main != null)
            {
                // 从相机发射射线到鼠标位置
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer))
                {
                    if (hit.collider.gameObject.CompareTag("Turret"))
                    {
                        OnClickTurret(hit.collider.gameObject);
                    }
                }
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

            SetupTurret(turret);
        }
    }

    private void SetupTurret(TurretEntity turret)
    {
        var seat = GetTurretSeat();
        if (seat != null)
        {
            bool isSuccess = seat.SetupTurret(turret);
            if (!isSuccess) SetupTurret(turret);
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
        if (_turretDataList == null) return;
        var rowLength = _turretDataList.GetLength(0);
        var colLength = _turretDataList.GetLength(1);
        for (int i = 0; i < rowLength; i++)
        {
            for (int j = 0; j < colLength; j++)
            {
                _turretDataList[i, j].IsAlive = false;
            }
        }
    }

    public void ClearTurret()
    {
        if (_turretDataList != null)
        {
            _turretDataList = null;
        }

        if (_turretsGrid != null)
        {
            _turretsGrid.ClearTurrets();
        }

        if (_turretSeatList != null)
        {
            foreach (var turretSeat in _turretSeatList)
            {
                turretSeat.SetOccupy(false);
            }
        }
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