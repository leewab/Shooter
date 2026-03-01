using System;

namespace Gameplay
{
    public class LevelManager : Singleton<LevelManager>
    {
        private int _LevelID = 1;

        public Action<int> OnLevelChange;

        public int GetCurrentLevel()
        {
            return _LevelID;
        }

        public float GetCurrentDifficulty()
        {
            return _LevelID / (float)10;
        }

        public void StartNextLevel()
        {
            _LevelID++;
            OnLevelChange?.Invoke(_LevelID);
            GameController.Instance.PrepareGame();
        }

        public void RestartLevel()
        {
            GameController.Instance.PrepareGame();
        }
        
        public void StopGame()
        {
            GameController.Instance.QuitGame();
        }
        
    }
}