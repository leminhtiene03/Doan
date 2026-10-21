using System;
using Game.Data;
using Game.Progression;
using Game.Shop;
using Unity.Cinemachine;
using UnityEngine;

namespace Game.Core
{
    using Game.Player;

    [DefaultExecutionOrder(-100)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private RunSelection runSelection;
        [SerializeField] private CharacterData defaultCharacter;
        [SerializeField] private DifficultyData defaultDifficulty;
        [SerializeField] private ShopConfig permanentShop;
        [SerializeField] private PlayerProgression progression;
        [SerializeField] private CinemachineCamera followCamera;
        [SerializeField] private Vector2 playerSpawnPosition;

        public event Action<GameState> OnStateChanged;
        public event Action<float> OnRunTimeSkipped;

        public GameState State { get; private set; }
        public bool IsPlaying => State == GameState.Playing;
        public float RunTime { get; private set; }
        public DifficultyData Difficulty { get; private set; }
        public Player Player { get; private set; }

        private void Awake()
        {
            Instance = this;
            Difficulty = runSelection != null && runSelection.Difficulty != null
                ? runSelection.Difficulty
                : defaultDifficulty;
            SpawnPlayer();
            ApplyPermanentUpgrades();
            SetState(GameState.Playing);
        }

        private void Update()
        {
            if (IsPlaying)
                RunTime += Time.deltaTime;
        }

        private void OnEnable()
        {
            Player.OnDeath += HandlePlayerDeath;
            progression.OnLevelChanged += Player.OnLevelChanged;
        }

        private void OnDisable()
        {
            if (Player == null)
                return;
            Player.OnDeath -= HandlePlayerDeath;
            progression.OnLevelChanged -= Player.OnLevelChanged;
        }

        public void SkipTime(float seconds) => SetRunTime(RunTime + seconds);

        public void SetRunTime(float runTime)
        {
            RunTime = Mathf.Max(0f, runTime);
            OnRunTimeSkipped?.Invoke(RunTime);
        }

        public void EnterLevelUp() => SetState(GameState.LevelUp);

        public void ResumePlaying() => SetState(GameState.Playing);

        public void TogglePause()
        {
            if (State == GameState.Playing)
                SetState(GameState.Paused);
            else if (State == GameState.Paused)
                SetState(GameState.Playing);
        }

        public bool IsWeaponUnlocked(WeaponData weapon)
            => AccountLedger == null || !AccountLedger.IsWeaponLocked(permanentShop, weapon);

        public void RestartRun() => SceneLoader.ReloadCurrent();

        public void ExitToMainMenu() => SceneLoader.LoadMainMenu();

        private void SpawnPlayer()
        {
            CharacterData character = runSelection != null && runSelection.Character != null
                ? runSelection.Character
                : defaultCharacter;
            Player = Instantiate(character.PlayerPrefab, playerSpawnPosition, Quaternion.identity);
            followCamera.Follow = Player.transform;
        }

        private ShopLedger AccountLedger => permanentShop != null && runSelection != null ? runSelection.Account?.Ledger : null;

        private void ApplyPermanentUpgrades()
        {
            if (AccountLedger == null)
                return;
            ShopItemEffects.ApplyAll(permanentShop, AccountLedger, Player);
            Player.RestoreFullHealth();
        }

        private void HandlePlayerDeath() => SetState(GameState.GameOver);

        private void SetState(GameState nextState)
        {
            State = nextState;
            Time.timeScale = nextState == GameState.Playing ? 1f : 0f;
            OnStateChanged?.Invoke(nextState);
        }
    }
}
