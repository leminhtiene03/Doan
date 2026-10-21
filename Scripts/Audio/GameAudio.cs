using System.Collections.Generic;
using Game.Core;
using Game.Data;
using Game.Enemies;
using Game.Progression;
using UnityEngine;

namespace Game.Audio
{
    using Game.Player;

    public class GameAudio : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private PickupManager pickupManager;
        [SerializeField] private LevelUpManager levelUpManager;
        [SerializeField] private SoundData enemyHit;
        [SerializeField] private SoundData enemyDeath;
        [SerializeField] private SoundData playerHurt;
        [SerializeField] private SoundData gemPickup;
        [SerializeField] private SoundData coinPickup;
        [SerializeField] private SoundData chestPickup;
        [SerializeField] private SoundData levelUp;
        [SerializeField] private SoundData chestOpen;
        [SerializeField] private SoundData gameOver;

        private Player player;

        private void Awake() => player = GameManager.Instance.Player;

        private void OnEnable()
        {
            gameManager.OnStateChanged += HandleStateChanged;
            pickupManager.OnCollected += HandlePickupCollected;
            levelUpManager.OnOptionsReady += HandleOptionsReady;
            player.OnHurt += HandlePlayerHurt;
        }

        private void OnDisable()
        {
            gameManager.OnStateChanged -= HandleStateChanged;
            pickupManager.OnCollected -= HandlePickupCollected;
            levelUpManager.OnOptionsReady -= HandleOptionsReady;
            if (player != null)
                player.OnHurt -= HandlePlayerHurt;
        }

        private void Start()
        {
            EnemyManager.Instance.OnEnemyDamaged += HandleEnemyDamaged;
            EnemyManager.Instance.OnEnemyDied += HandleEnemyDied;
        }

        private void OnDestroy()
        {
            if (EnemyManager.Instance == null)
                return;
            EnemyManager.Instance.OnEnemyDamaged -= HandleEnemyDamaged;
            EnemyManager.Instance.OnEnemyDied -= HandleEnemyDied;
        }

        private void HandleEnemyDamaged(Enemy enemy) => AudioManager.Play(enemyHit);

        private void HandleEnemyDied(Enemy enemy) => AudioManager.Play(enemyDeath);

        private void HandlePlayerHurt() => AudioManager.Play(playerHurt);

        private void HandlePickupCollected(PickupKind kind)
        {
            switch (kind)
            {
                case PickupKind.Gem:
                    AudioManager.Play(gemPickup);
                    break;
                case PickupKind.Coin:
                    AudioManager.Play(coinPickup);
                    break;
                case PickupKind.Chest:
                    AudioManager.Play(chestPickup);
                    break;
            }
        }

        private void HandleOptionsReady(IReadOnlyList<UpgradeOption> options) =>
            AudioManager.Play(levelUpManager.IsChestReward ? chestOpen : levelUp);

        private void HandleStateChanged(GameState state)
        {
            if (state != GameState.GameOver)
                return;
            AudioManager.StopMusic();
            AudioManager.Play(gameOver);
        }
    }
}
