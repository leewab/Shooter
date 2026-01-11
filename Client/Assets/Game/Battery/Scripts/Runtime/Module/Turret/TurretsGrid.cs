using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Gameplay
{
    public class TurretsGrid : MonoBehaviour
    {
        private Vector2 _StartPostion;
        private Vector2 _Space = new Vector2(15, 15);
        private IReadOnlyList<IReadOnlyList<TurretData>> _TurretDataList;
        private Dictionary<int, TurretEntity> _TurretEntitiesMap;

        private void OnEnable()
        {
            TurretHandler.Instance.OnRefreshTurret -= OnRefreshTurret;
            TurretHandler.Instance.OnRefreshTurret += OnRefreshTurret;
        }

        public void InitializeTurrets(IReadOnlyList<IReadOnlyList<TurretData>> turretGrid)
        {
            _TurretDataList = turretGrid;
            _StartPostion = transform.position;
            InitTurretGrid();
            // Invoke(nameof(InitTurretGrid), 1);
        }

        public void ClearTurrets()
        {
            if (_TurretEntitiesMap != null)
            {
                foreach (var turretEntity in _TurretEntitiesMap.Values)
                {
                    turretEntity.Recycle();
                }
                
                _TurretEntitiesMap.Clear();
            }

            _TurretDataList = null;
        }

        private void InitTurretGrid()
        {
            Debug.Log("InitTurretGrid   " + _TurretDataList.Count);
            if  (_TurretEntitiesMap != null) _TurretEntitiesMap.Clear();
            for (int i = 0; i < _TurretDataList.Count; i++)
            {
                var turrets = _TurretDataList[i];
                if (turrets == null)
                {
                    Debug.LogError("turrets wei null");
                    continue;
                }
                if (_TurretEntitiesMap == null)
                {
                    int totalNum = _TurretDataList.Count * turrets.Count;
                    _TurretEntitiesMap = new Dictionary<int, TurretEntity>(totalNum);
                }
                for (int j = 0; j < turrets.Count; j++)
                {
                    var turretData = turrets[j];
                    var turret = GenerateTurret(turretData) as TurretEntity;
                    turret.Init(turretData);
                    turret.OnDeadEvent.RemoveListener(OnDeadEvent);
                    turret.OnDeadEvent.AddListener(OnDeadEvent);
                    _TurretEntitiesMap.Add(turretData.Index, turret);
                }
            }
        }

        private BaseTurret GenerateTurret(TurretData turretData)
        {
            Vector2 pos = _StartPostion + new Vector2(turretData.Column * _Space.x, - turretData.PositionIndex * _Space.y);
            return TurretManager.Instance.InstantiateTurret(transform, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
        }

        private void OnRefreshTurret(int column)
        {
            if (_TurretEntitiesMap == null) return;
            if (_TurretDataList == null || column >= _TurretDataList.Count) return;
            var turrets = _TurretDataList[column];
            for (int j = 0; j < turrets.Count; j++)
            {
                var turretData = turrets[j];
                if (_TurretEntitiesMap.TryGetValue(turretData.Index, out TurretEntity turret))
                {
                    turret.Init(turretData);
                    Vector3 targetPos = new Vector3(
                        _StartPostion.x + turretData.Column * _Space.x,
                        _StartPostion.y - turretData.PositionIndex * _Space.y,
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