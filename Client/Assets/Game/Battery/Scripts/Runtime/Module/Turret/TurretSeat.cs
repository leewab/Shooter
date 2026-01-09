using System;
using UnityEngine;

namespace Gameplay
{
    public class TurretSeat : MonoBehaviour
    {
        private bool _isActive = false;
        private bool _isOccupy = false;
        
        [SerializeField]
        private GameObject _turretLock;

        public void SetActive(bool isActive)
        {
            this._isActive = isActive;
            this._turretLock?.SetActive(!isActive);
        }

        public void SetOccupy(bool isOccupy)
        {
            this._isOccupy = isOccupy;
        }

        public bool SetTurret(TurretEntity turret)
        {
            if (!this._isActive)
            {
                Debug.LogWarning("TurretSeat::SetTurret: Turret is not active.");
                return false;
            }

            turret.transform.SetParent(this.transform);
            turret.transform.localPosition = Vector3.zero;
            turret.SetActive(true);
            turret.RemoveTurret();
            turret.OnDeadEvent.RemoveListener(OnTurretDeadEvent);
            turret.OnDeadEvent.AddListener(OnTurretDeadEvent);
            SetOccupy(true);
            return true;
        }

        public bool IsOccupy()
        {
            return this._isOccupy;
        }

        public bool IsActive()
        {
            return this._isActive;
        }

        private void OnTurretDeadEvent()
        {
            SetOccupy(false);
        }
        
        
    }
}