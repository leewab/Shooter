using System.Text;
using UnityEngine;
using DG.Tweening;
using GameConfig;
using UnityEditor;

namespace Gameplay
{
    public class TurretsGrid : MonoBehaviour
    {
        private Vector2 _StartPostion;
        private Vector2 _Space = new Vector2(20, 20);
        private TurretEntity[,] _TurretEntitiesMap;

        public void InitializeTurrets(TurretInfo[,] turretGrid)
        {
            if (turretGrid == null) return;
            _StartPostion = transform.position;
            int rowLength = turretGrid.GetLength(0);
            int colLength = turretGrid.GetLength(1);
            _TurretEntitiesMap = new TurretEntity[rowLength, colLength];
            for (int i = 0; i < rowLength; i++)
            {
                for (int j = 0; j < colLength; j++)
                {
                    var turretData = turretGrid[i, j];
                    _TurretEntitiesMap[i, j] = GenerateTurretEntity(turretData, new TurretPos(i, j));
                }
            }
        }

        private static int GenerateEntityNum = 0;
        public void RefreshTurrets(int removeRow, int removeCol, TurretInfo[,] turretGrid)
        {
            if (turretGrid == null) return;
            _StartPostion = transform.position;
            if (_TurretEntitiesMap == null) return;

            int rowLength = turretGrid.GetLength(0);
            int colLength = turretGrid.GetLength(1);
            if (removeCol >= colLength) return;

            // 创建临时数组
            var turretEntitiesMap = new TurretEntity[rowLength, colLength];

            // 复制所有元素
            for (int i = 0; i < rowLength; i++)
            {
                for (int j = 0; j < colLength; j++)
                {
                    turretEntitiesMap[i, j] = _TurretEntitiesMap[i, j];
                }
            }
            turretEntitiesMap[removeRow, removeCol] = null;

            // 处理当前列
            var posX = _StartPostion.x + removeCol * _Space.x;
            for (int i = removeRow; i < rowLength; i++)
            {
                var turretData = turretGrid[i, removeCol];
                if (i < rowLength - 1 && turretEntitiesMap[i + 1, removeCol] != null)
                {
                    // 使用同一列下一行的TurretEntity
                    Vector3 targetPos = new Vector3(posX, _StartPostion.y - (i * _Space.y), 0);
                    var turretEntity = turretEntitiesMap[i + 1, removeCol];
                    // Debug.LogError($"当前位置{i},{removeCol},补充name:{turretEntity.gameObject.name}");
                    turretEntity.transform.DOKill();
                    turretEntity.transform.DOMove(targetPos, 0.2f).SetEase(Ease.OutBack);
                    turretEntity.gameObject.name = $"Turret_{i}_{removeCol}|{(ColorType)turretData.Type}";
                    turretEntity.Init(turretData, new TurretPos(i, removeCol), 0.3f);
                    turretEntitiesMap[i, removeCol] = turretEntity;
                    turretEntitiesMap[i + 1, removeCol] = null; // 清空原位置
                }
                else
                {
                    GenerateEntityNum++;
                    // 生成新的TurretEntity
                    var turretEntity = GenerateTurretEntity(turretData, new TurretPos(i, removeCol));
                    if (turretEntity != null)
                    {
                        // Debug.LogError($"当前位置为空{i},{removeCol},补充name:{turretEntity.gameObject.name}");
                        turretEntitiesMap[i, removeCol] = turretEntity;
                    }
                    else
                    {
                        Debug.LogError("未生成炮台");
                    }
                }
            }

            // 复制回原数组
            for (int i = 0; i < rowLength; i++)
            {
                for (int j = 0; j < colLength; j++)
                {
                    _TurretEntitiesMap[i, j] = turretEntitiesMap[i, j];
                }
            }
        }

        public void ClearTurrets()
        {
            if (_TurretEntitiesMap != null)
            {
                int rowLength = _TurretEntitiesMap.GetLength(0);
                int colLength = _TurretEntitiesMap.GetLength(1);
                for (int i = 0; i < rowLength; i++)
                {
                    for (int j = 0; j < colLength; j++)
                    {
                        _TurretEntitiesMap[i, j].Recycle();
                    }
                }
                
                _TurretEntitiesMap = null;
            }
        }

        private TurretEntity GenerateTurretEntity(TurretInfo turretInfo, TurretPos turretPos)
        {
            Vector2 pos = _StartPostion + new Vector2(turretPos.ColIndex * _Space.x, - turretPos.RowIndex * _Space.y);
            Vector3 turretPosition = new Vector3(pos.x, pos.y, 0);
            var turret = GenerateTurret(turretPosition) as TurretEntity;
            if (turret != null)
            {
                turret.Init(turretInfo, turretPos);
            }
            else
            {
                Debug.LogError("Turret 生成为空！");
            }
            
            return turret;
        }

        private BaseTurret GenerateTurret(Vector3 turretPosition)
        {
            return TurretManager.Instance.InstantiateTurret(transform, turretPosition, Quaternion.identity);
        }
    }
}