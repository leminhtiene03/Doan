using Game.Shop;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Run Selection", fileName = "RunSelection")]
    public class RunSelection : ScriptableObject
    {
        public CharacterData Character { get; private set; }
        public DifficultyData Difficulty { get; private set; }
        public AccountProgress Account { get; private set; }

        public void SelectCharacter(CharacterData character) => Character = character;

        public void SelectDifficulty(DifficultyData difficulty) => Difficulty = difficulty;

        public void SetAccount(AccountProgress account) => Account = account;

        public void Clear()
        {
            Character = null;
            Difficulty = null;
            Account = null;
        }
    }
}
