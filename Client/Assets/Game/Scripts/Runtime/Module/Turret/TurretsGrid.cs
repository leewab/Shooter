using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

namespace Gameplay
{
    public class TurretsGrid : MonoBehaviour
    {
        private Vector2 _StartPostion;
        private Vector2 _Space = new Vector2(20, 20);
        private TurretData[,] _TurretDataList;
        private Dictionary<int, TurretEntity> _TurretEntitiesMap;

        private void OnEnable()
        {
            TurretHandler.Instance.OnRefreshTurret -= OnRefreshTurret;
            TurretHandler.Instance.OnRefreshTurret += OnRefreshTurret;
        }

        public void InitializeTurrets(TurretData[,] turretGrid)
        {
            _TurretDataList = turretGrid;
            _StartPostion = transform.position;
            InitTurretGrid();
        }

        public void ClearTurrets()
        {
            if (_TurretEntitiesMap != null)
            {
                var entities = _TurretEntitiesMap.Values.ToArray();
                foreach (var turretEntity in entities)
                {
                    turretEntity?.Recycle();
                }
                
                _TurretEntitiesMap.Clear();
            }

            _TurretDataList = null;
        }

        private void InitTurretGrid()
        {
            if  (_TurretEntitiesMap != null) _TurretEntitiesMap.Clear();
            int rowLength = _TurretDataList.GetLength(0);
            int colLength = _TurretDataList.GetLength(1);
            for (int i = 0; i < rowLength; i++)
            {
                for (int j = 0; j < colLength; j++)
                {
                    var turretData = _TurretDataList[i, j];
                    if (turretData == null)
                    {
                        Debug.LogError("turrets wei null");
                        continue;
                    }
                    if (_TurretEntitiesMap == null)
                    {
                        int totalNum = rowLength * colLength;
                        _TurretEntitiesMap = new Dictionary<int, TurretEntity>(totalNum);
                    }
                    var turret = GenerateTurret(turretData) as TurretEntity;
                    if (turret != null)
                    {
                        turret.Init(turretData);
                        turret.OnDeadEvent -= OnDeadEvent;
                        turret.OnDeadEvent += OnDeadEvent;
                        _TurretEntitiesMap.Add(turretData.Index, turret);
                    }
                }
            }
        }

        private BaseTurret GenerateTurret(TurretData turretData)
        {
            Vector2 pos = _StartPostion + new Vector2(turretData.Col * _Space.x, - turretData.Row * _Space.y);
            return TurretManager.Instance.InstantiateTurret(transform, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
        }

        private void OnRefreshTurret(int column)
        {
            if (_TurretEntitiesMap == null) return;
            if (_TurretDataList == null) return;
            int rowLength = _TurretDataList.GetLength(0);
            int colLength = _TurretDataList.GetLength(1);
            if (column >= colLength) return;
            for (int i = 0; i < rowLength; i++)
            {
                var turretData = _TurretDataList[i, column];
                if (!turretData.IsAlive) continue;
                if (_TurretEntitiesMap.TryGetValue(turretData.Index, out TurretEntity turret))
                {
                    turret.Init(turretData);
                    Vector3 targetPos = new Vector3(
                        _StartPostion.x + turretData.Col * _Space.x,
                        _StartPostion.y - turretData.Row * _Space.y,
                        0
                    );
                    turret.transform.DOKill();
                    turret.transform.DOMove(targetPos, 0.3f).SetEase(Ease.OutBack);
                }
            }
        }

        private void OnDeadEvent(int index)
        {
            if (_TurretEntitiesMap.TryGetValue(index, out TurretEntity turret))
            {
                _TurretEntitiesMap.Remove(index);
            }
        }

    }
}