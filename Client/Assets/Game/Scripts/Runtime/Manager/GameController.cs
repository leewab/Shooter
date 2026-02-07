using System;
using Framework.UIFramework;
using GameUI;
using UnityEngine;

namespace Gameplay
{
    public enum GameState
    {
        None,
        Preparing,
        Playing,
        Paused,
        GameOver
    }

    public class GameController : SingletonMono<GameController>
    {
        [Header("Game Settings")]
        [SerializeField] private DragonController dragonController;
        [SerializeField] private TurretHandler turretsHandler;

        public GameState CurrentState { get; private set; }
        public float GameTime { get; private set; }
        public int Score { get; private set; }

        public event Action<bool> OnGameEnd;
        public event Action<int> OnScoreChanged;
        public event Action<float> OnGameTimeChanged;

        private void Start()
        {
            InitializeGame();
            RestartGame();
        }

        private void Update()
        {
            if (CurrentState == GameState.Playing)
            {
                GameTime += Time.deltaTime;
                OnGameTimeChanged?.Invoke(GameTime);
            }
        }

        private void OnDestroy()
        {
            UnregisterEvents();
        }

        private void InitializeGame()
        {
            if (dragonController == null) dragonController = FindObjectOfType<DragonController>();
            if (turretsHandler == null) turretsHandler = FindObjectOfType<TurretHandler>();
            UIDefine.Init();
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            if (DragonController.Instance != null)
            {
                DragonController.Instance.OnSuccessEvent += HandleDragonResult;
            }
        }

        private void UnregisterEvents()
        {
            if (DragonController.Instance != null)
            {
                DragonController.Instance.OnSuccessEvent -= HandleDragonResult;
            }
        }

        public void PrepareGame()
        {
            CurrentState = GameState.Preparing;
            Score = 0;
            GameTime = 0f;
            
            if (dragonController != null)
            {
                dragonController.ResetDragon();
                dragonController.StartMoving();
            }

            if (turretsHandler != null)
            {
                turretsHandler.ClearTurret();
                turretsHandler.InitTurret();
            }
            
            Debug.Log("[GameController] Game Prepared");
        }

        public void StartGame()
        {
            CurrentState = GameState.Playing;

            if (dragonController != null)
            {
                dragonController.InitDragon();
                dragonController.StartMoving();
            }

            Debug.Log("[GameController] Game Started");
        }

        public void PauseGame()
        {
            if (CurrentState != GameState.Playing) return;

            CurrentState = GameState.Paused;

            if (dragonController != null)
            {
                dragonController.StopMoving();
            }

            Debug.Log("[GameController] Game Paused");
        }

        public void ResumeGame()
        {
            if (CurrentState != GameState.Paused) return;

            CurrentState = GameState.Playing;

            if (dragonController != null)
            {
                dragonController.StartMoving();
            }

            Debug.Log("[GameController] Game Resumed");
        }

        public void EndGame(bool win)
        {
            CurrentState = GameState.GameOver;
            if (dragonController != null)
            {
                dragonController.StopMoving();
            }
            
            OnGameEnd?.Invoke(win);
            Debug.Log($"[GameController] Game End - Win: {win}");
        }

        public void RestartGame()
        {
            PrepareGame();
            StartGame();

            Debug.Log("[GameController] Game Restarted");
        }

        public void QuitGame()
        {
            CurrentState = GameState.None;

            if (dragonController != null)
            {
                dragonController.StopMoving();
            }

            UIManager.CloseCurrent();
            OnGameEnd?.Invoke(false);

            Debug.Log("[GameController] Game Quit");
        }

        public void AddScore(int points)
        {
            Score += points;
            OnScoreChanged?.Invoke(Score);
        }

        private void HandleDragonResult(bool success)
        {
            EndGame(success);
        }
    }
}
