using GameConfig;
using UnityEngine;

namespace Gameplay
{
    public class TurretSeat : MonoBehaviour
    {
        [SerializeField] private GameObject _turretLock;
        private TurretEntity _TurretEntity;

        public bool IsActive;
        public bool IsOccupy;
        
        public void SetActive(bool isActive)
        {
            this.IsActive = isActive;
            this.gameObject.SetActive(true);
            this._turretLock?.SetActive(!isActive);
        }

        public void ResetSeat()
        {
            if (_TurretEntity != null)
            {
                _TurretEntity.Recycle();
                _TurretEntity = null;
            }
        }

        public void SetOccupy(bool isOccupy)
        {
            if (!isOccupy)
            {
                // Debug.LogError(this.gameObject.name + "位置被释放！");
                var turret = this.transform.GetComponentsInChildren<TurretEntity>();
                if (turret != null && turret.Length > 0)
                {
                    Debug.LogError("出现严重问题，Seat被释放了，但是还存在TurretEntity!");
                    Application.Pause();
                }
            }
            
            this.IsOccupy = isOccupy;
        }

        public bool SetupTurret(TurretEntity turret)
        {
            if (!this.IsActive)
            {
                Debug.LogWarning("TurretSeat::SetTurret: Turret is not active.");
                return false;
            }

            if (this.IsOccupy)
            {
                Debug.LogWarning("TurretSeat::SetTurret: Turret is Occupy.");
                return false;
            }
            
            SetOccupy(true);

            _TurretEntity = turret;
            _TurretEntity.SetupTurret(this.transform);
            _TurretEntity.OnDeadEvent -= OnTurretDeadEvent;
            _TurretEntity.OnDeadEvent += OnTurretDeadEvent;
            return true;
        }

        private void OnTurretDeadEvent()
        {
            _TurretEntity = null;
            SetOccupy(false);
        }
        
    }
}