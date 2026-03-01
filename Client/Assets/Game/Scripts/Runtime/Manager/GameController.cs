using System;
using Framework.UIFramework;
using GameUI;
using UnityEngine;
using UnityEngine.Serialization;

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
        public event Action<int> OnGameCountdown;
        
        private int _GameCountdown = 5;
        private float _timer = 0f;

        private void Start()
        {
            InitializeGame();
            PrepareGame();
        }

        private void Update()
        {
            if (CurrentState == GameState.Preparing)
            {
                // 倒计时逻辑
                _timer += Time.deltaTime;
                if (_timer >= 1f)
                {
                    _timer -= 1f;
                    _GameCountdown--;
                    OnGameCountdown?.Invoke(_GameCountdown);
                    
                    if (_GameCountdown <= 0)
                    {
                        StartGame();
                    }
                }
            }
            else if (CurrentState == GameState.Playing)
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
            _GameCountdown = 5; // 重置倒计时
            _timer = 0f;        // 重置计时器

            if (dragonController != null)
            {
                dragonController.InitDragon();
                dragonController.StartMoving();
            }
            if (turretsHandler != null)
            {
                turretsHandler.ClearTurret();
                turretsHandler.InitTurret();
            }

            // 游戏主界面
            UIManager.Open<UIGameMainPanel>();
            OnGameCountdown?.Invoke(_GameCountdown); // 刷新初始倒计时UI
            Debug.Log("游戏状态：" + CurrentState);
        }

        public void StartGame()
        {
            CurrentState = GameState.Playing;
            Debug.Log("游戏状态：" + CurrentState);
        }

        public void PauseGame()
        {
            if (CurrentState != GameState.Playing) return;

            CurrentState = GameState.Paused;

            if (dragonController != null)
            {
                dragonController.StopMoving();
            }
            
            Debug.Log("游戏状态：" + CurrentState);
        }

        public void ResumeGame()
        {
            if (CurrentState != GameState.Paused) return;

            CurrentState = GameState.Playing;

            if (dragonController != null)
            {
                dragonController.StartMoving();
            }

            Debug.Log("游戏状态：" + CurrentState);
        }

        public void EndGame(bool win)
        {
            CurrentState = GameState.GameOver;
            if (dragonController != null)
            {
                dragonController.StopMoving();
            }
            
            OnGameEnd?.Invoke(win);
            Debug.Log("游戏状态：" + CurrentState);
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
