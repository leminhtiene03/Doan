using System.Collections.Generic;
using Game.Core;
using Game.Data;
using Game.Enemies;
using Game.Progression;
using Game.Spawning;
using Game.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.DebugTools
{
    using Game.Player;

    public class DebugTools : MonoBehaviour
    {
        private Player player;
        [SerializeField] private PlayerProgression progression;
        [SerializeField] private SpawnManager spawnManager;
        [SerializeField] private EnemyData[] spawnableEnemies;
        [SerializeField] private float debugXPAmount = 10f;
        [SerializeField] private int debugGoldAmount = 50;
        [SerializeField] private float statsRefreshInterval = 0.5f;
        [SerializeField] private Rect panelRect = new(10f, 10f, 260f, 800f);

        private string[] spawnButtonLabels;
        private string spawnCountText = "100";
        private string jumpMinuteText = "5";
        private string statsLabel = string.Empty;
        private bool isVisible = true;
        private int framesSinceRefresh;
        private float timeSinceRefresh;

        private void Awake()
        {
            player = GameManager.Instance.Player;
            if (!Debug.isDebugBuild)
            {
                Destroy(this);
                return;
            }

            spawnButtonLabels = new string[spawnableEnemies.Length];
            for (int i = 0; i < spawnableEnemies.Length; i++)
                spawnButtonLabels[i] = "Spawn " + spawnableEnemies[i].DisplayName;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
                isVisible = !isVisible;

            framesSinceRefresh++;
            timeSinceRefresh += Time.unscaledDeltaTime;
            if (timeSinceRefresh >= statsRefreshInterval)
                RefreshStats();
        }

        private void RefreshStats()
        {
            int fps = Mathf.RoundToInt(framesSinceRefresh / timeSinceRefresh);
            framesSinceRefresh = 0;
            timeSinceRefresh = 0f;
            GameManager game = GameManager.Instance;
            int runSeconds = Mathf.FloorToInt(game.RunTime);
            statsLabel = $"FPS: {fps} ({1000f / Mathf.Max(fps, 1):0.0} ms)\n"
                + $"State: {game.State}  TimeScale: {Time.timeScale}\n"
                + $"Time: {runSeconds / 60:00}:{runSeconds % 60:00}  Difficulty: {game.Difficulty.DisplayName}\n"
                + $"Wave: {spawnManager.CurrentWaveIndex + 1}  Max alive: {spawnManager.CurrentMaxAlive}"
                + (spawnManager.IsPaused ? "  (paused)\n" : "\n")
                + $"Pending level ups: {progression.PendingLevelUps}  Chests: {progression.PendingChests}\n"
                + $"Enemies: {EnemyManager.Instance.AliveCount} alive / {EnemyManager.Instance.Active.Count} active\n"
                + $"HP: {player.CurrentHP:0}/{player.Stats.MaxHP:0}\n"
                + $"Level: {progression.Level}  XP: {progression.CurrentXP:0}/{progression.RequiredXP:0}\n"
                + $"Gold: {progression.Gold}";
        }

        private void OnGUI()
        {
            if (!isVisible)
                return;

            GUILayout.BeginArea(panelRect, GUI.skin.box);
            GUILayout.Label(statsLabel);
            DrawPlayerControls();
            DrawTimeControls();
            DrawSpawnControls();
            DrawGameOver();
            GUILayout.Label("F1: toggle panel");
            GUILayout.EndArea();
        }

        private void DrawPlayerControls()
        {
            player.IsInvincible = GUILayout.Toggle(player.IsInvincible, "Invincible");
            if (GUILayout.Button("Full heal"))
                player.RestoreFullHealth();
            if (GUILayout.Button("+XP"))
                progression.AddXP(debugXPAmount);
            if (GUILayout.Button("+Gold"))
                progression.AddGold(debugGoldAmount);
            if (GUILayout.Button("Max weapons"))
                MaxAllWeapons();
            if (GUILayout.Button("Give evolution items"))
                GiveEvolutionItems();
            if (GUILayout.Button("Open chest"))
                progression.AddChest();
        }

        private void MaxAllWeapons()
        {
            IReadOnlyList<Weapon> weapons = player.Inventory.Weapons;
            for (int i = 0; i < weapons.Count; i++)
            {
                while (!weapons[i].IsMaxLevel)
                    weapons[i].LevelUp();
            }
        }

        private void GiveEvolutionItems()
        {
            PlayerInventory inventory = player.Inventory;
            for (int i = 0; i < inventory.Weapons.Count; i++)
            {
                PassiveItemData item = inventory.Weapons[i].Data.EvolutionItem;
                if (item != null && !inventory.HasPassive(item))
                    inventory.AddPassive(item);
            }
        }

        private void DrawTimeControls()
        {
            spawnManager.enabled = GUILayout.Toggle(spawnManager.enabled, "Auto spawn");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+1 min"))
                GameManager.Instance.SkipTime(60f);
            if (GUILayout.Button("+5 min"))
                GameManager.Instance.SkipTime(300f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Minute");
            jumpMinuteText = GUILayout.TextField(jumpMinuteText);
            if (GUILayout.Button("Go") && float.TryParse(jumpMinuteText, out float minute))
                GameManager.Instance.SetRunTime(minute * 60f);
            GUILayout.EndHorizontal();
        }

        private void DrawSpawnControls()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Count");
            spawnCountText = GUILayout.TextField(spawnCountText);
            GUILayout.EndHorizontal();

            for (int i = 0; i < spawnableEnemies.Length; i++)
            {
                if (GUILayout.Button(spawnButtonLabels[i]))
                    SpawnEnemies(spawnableEnemies[i]);
            }

            if (GUILayout.Button("Clear all enemies"))
                EnemyManager.Instance.DespawnAll();
        }

        private void DrawGameOver()
        {
            if (GameManager.Instance.State != GameState.GameOver)
                return;

            GUILayout.Label("GAME OVER");
            if (GUILayout.Button("Restart"))
                GameManager.Instance.RestartRun();
        }

        private void SpawnEnemies(EnemyData data)
        {
            if (!int.TryParse(spawnCountText, out int count))
                return;

            EnemyManager manager = EnemyManager.Instance;
            for (int i = 0; i < count; i++)
                manager.Spawn(data, manager.GetRandomOffscreenPosition());
        }
    }
}
