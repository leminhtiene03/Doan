using Game.Core;
using Game.Weapons;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Weapon Data", fileName = "WeaponData")]
    public class WeaponData : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private Weapon weaponPrefab;
        [SerializeField] private PooledEffect effectPrefab;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private AimMode aimMode;
        [SerializeField] private DamageType damageType;
        [SerializeField] private bool isStartingWeapon;
        [SerializeField] private bool playOwnerAttackAnim;
        [SerializeField] private WeaponLevelStats[] levels = new WeaponLevelStats[5];
        [SerializeField] private WeaponData evolution;
        [SerializeField] private PassiveItemData evolutionItem;
        [SerializeField] private SoundData attackSound;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public Sprite DisplayIcon
        {
            get
            {
                if (icon != null)
                    return icon;
                if (projectilePrefab != null && projectilePrefab.Sprite != null)
                    return projectilePrefab.Sprite;
                if (effectPrefab != null && effectPrefab.TryGetComponent(out SpriteRenderer effectRenderer))
                    return effectRenderer.sprite;
                return weaponPrefab != null ? weaponPrefab.PreviewSprite : null;
            }
        }
        public Weapon WeaponPrefab => weaponPrefab;
        public PooledEffect EffectPrefab => effectPrefab;
        public Projectile ProjectilePrefab => projectilePrefab;
        public AimMode AimMode => aimMode;
        public DamageType DamageType => damageType;
        public bool IsStartingWeapon => isStartingWeapon;
        public bool PlayOwnerAttackAnim => playOwnerAttackAnim;
        public int MaxLevel => levels.Length;
        public WeaponData Evolution => evolution;
        public PassiveItemData EvolutionItem => evolutionItem;
        public SoundData AttackSound => attackSound;

        public WeaponLevelStats GetLevel(int level) => levels[Mathf.Clamp(level - 1, 0, levels.Length - 1)];
    }
}
