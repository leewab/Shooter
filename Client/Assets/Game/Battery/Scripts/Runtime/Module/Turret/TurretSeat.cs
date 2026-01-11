using UnityEngine;

namespace Gameplay
{
    public class TurretSeat : MonoBehaviour
    {
        [SerializeField] private GameObject _turretLock;

        private bool _isActive = false;
        public bool IsActive => _isActive;
        
        private bool _isOccupy = false;
        public bool IsOccupy => _isOccupy;

        public void SetActive(bool isActive)
        {
            this._isActive = isActive;
            this.gameObject.SetActive(true);
            this._turretLock?.SetActive(!isActive);
        }

        public void SetOccupy(bool isOccupy)
        {
            this._isOccupy = isOccupy;
        }

        public bool SetupTurret(TurretEntity turret)
        {
            if (!this._isActive)
            {
                Debug.LogWarning("TurretSeat::SetTurret: Turret is not active.");
                return false;
            }
            
            turret.SetupTurret(this.transform);
            turret.OnDeadEvent?.RemoveListener(OnTurretDeadEvent);
            turret.OnDeadEvent?.AddListener(OnTurretDeadEvent);
            SetOccupy(true);
            return true;
        }

        private void OnTurretDeadEvent(int index)
        {
            SetOccupy(false);
        }
        
    }
}