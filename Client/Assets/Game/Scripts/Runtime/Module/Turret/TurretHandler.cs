using System;
using System.Collections.Generic;
using Gameplay;
using UnityEngine;

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
    // Turret索引
    private int _Index = 0;
    private LayerMask targetLayer;
    // 核心网格数据：[列索引][列内炮台列表]，保证每列独立管理、补位
    private TurretInfo[,] _turretInfoMatrix;

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

        _turretInfoMatrix = TurretMatrixManager.Instance.GenerateTurretMatrix();
        _turretsGrid.InitializeTurrets(_turretInfoMatrix);
    }

    /// <summary>
    /// 点击消除指定炮台（核心消除算法）
    /// </summary>
    /// <param name="targetTurretData">待消除的目标炮台</param>
    /// <returns>是否消除成功</returns>
    public void EliminateTurret(int removeRow, int removeCol)
    {
        int rowLength = _turretInfoMatrix.GetLength(0);
        int colLength = _turretInfoMatrix.GetLength(1);

        if (removeCol < 0 || removeCol >= colLength)
        {
            Debug.LogError("消除列不合法！ removeCol:" + removeCol);
            return;
        }

        for (int i = 0; i < rowLength; i++)
        {
            if (i < rowLength - 1)
            {
                // 将下一行的Turret移动到当前行
                _turretInfoMatrix[i, removeCol] = _turretInfoMatrix[i + 1, removeCol];
            }
            else
            {
                // 生成新的Turret
                var turretInfo = TurretMatrixManager.Instance.GetTurretInfo();
                _turretInfoMatrix[i, removeCol] = turretInfo;
            }
        }

        _turretsGrid.RefreshTurrets(removeRow, removeCol, _turretInfoMatrix);
    }
    
    private void OnRaycastClick()
    {
        if (GameController.Instance.CurrentState != GameState.Playing) return;
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

    public void ClearTurret()
    {
        _turretInfoMatrix = null;

        if (_turretsGrid != null)
        {
            _turretsGrid.ClearTurrets();
        }

        if (_turretSeatList != null)
        {
            foreach (var turretSeat in _turretSeatList)
            {
                turretSeat.ResetSeat();
            }
        }
    }
    
    public TurretSeat GetTurretSeat()
    {
        for (int i = 0; i < _turretSeatList.Length; i++)
        {
            if (!_turretSeatList[i].IsOccupy && _turretSeatList[i].IsActive) return _turretSeatList[i];
        }
        
        // Debug.LogError("炮台已经满了");
        return null;
    }

    public bool HasValidTurretSeat(List<DragonJoint> dragonBones)
    {
        // var turretSeat = GetTurretSeat();
        // if (turretSeat == null)
        // {
        //     foreach (var dragonJoint in dragonBones)
        //     {
        //         if (dragonJoint.IsBody())
        //         {
        //             var colorType = dragonJoint.GetColorType();
        //             foreach (var seat in _turretSeatList)
        //             {
        //                 if (seat.GetSeatColorType() == colorType) return true;
        //             }
        //         }
        //     }
        //     
        //     return false;
        // }

        return true;
    }
    
}