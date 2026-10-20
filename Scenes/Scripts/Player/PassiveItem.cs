using Game.Data;

namespace Game.Player
{
    public class PassiveItem
    {
        public PassiveItem(PassiveItemData data)
        {
            Data = data;
            Level = 1;
        }

        public PassiveItemData Data { get; }
        public int Level { get; private set; }
        public bool IsMaxLevel => Level >= Data.MaxLevel;

        public void LevelUp() => Level++;
    }
}
