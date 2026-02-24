namespace Gameplay
{
    public class LevelManager : Singleton<LevelManager>
    {
        private int _LevelID = 1;

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
            GameController.Instance.RestartGame();
        }

        public void RestartLevel()
        {
            GameController.Instance.RestartGame();
        }
        
        public void StopGame()
        {
            GameController.Instance.QuitGame();
        }
        
    }
}