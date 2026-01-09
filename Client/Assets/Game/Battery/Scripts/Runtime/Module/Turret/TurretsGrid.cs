using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class TurretsGrid : MonoBehaviour
    {
        private Vector2 _StartPostion;
        private Vector2 _Space = new Vector2(15, 15);
        private IReadOnlyList<IReadOnlyList<TurretData>> _TurretGrid;

        private void Start()
        {
            TurretHandler.Instance.InitTurretGrid(10);
            _StartPostion = transform.position;
            _TurretGrid = TurretHandler.Instance.TurretGrid;
            Invoke(nameof(InitTurretGrid), 1);
        }

        private void OnEnable()
        {
            TurretHandler.Instance.OnRefreshTurret += OnRefreshTurret;
        }

        private void OnDisable()
        {
            
        }

        private void Update()
        {
            
        }

        private void InitTurretGrid()
        {
            // 行
            for (int i = 0; i < _TurretGrid.Count; i++)
            {
                UpdateTurretGrid(i);
            }
        }

        private void UpdateTurretGrid(int column)
        {
            if (_TurretGrid == null || column >= _TurretGrid.Count) return;
            var turrets = _TurretGrid[column];
            // 列
            for (int i = 0; i < turrets.Count; i++)
            {
                var turret = GenerateTurret(turrets[i]);
                turret.SetTurret(new TurretData(Random.Range(0, 5), column, i));
            }
        }

        private BaseTurret GenerateTurret(TurretData turretDataData)
        {
            Vector2 pos = _StartPostion + new Vector2(turretDataData.Column * _Space.x, - turretDataData.PositionIndex * _Space.y);
            return TurretManager.Instance.InstantiateTurret(transform, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
        }

        private void OnRemoveTurret(TurretData turretData)
        {
            TurretHandler.Instance.EliminateTurret(turretData);
        }

        private void OnRefreshTurret(int column)
        {
            UpdateTurretGrid(column);
        }
        
    }
}