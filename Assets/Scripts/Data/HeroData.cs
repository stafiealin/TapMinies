using UnityEngine;

namespace TapMinies.Data
{
    [CreateAssetMenu(fileName = "HeroData", menuName = "TapMinies/Hero Data")]
    public class HeroData : ScriptableObject
    {
        public string heroName;
        public Sprite portrait;
        public int unlockStage = 1;

        public long baseDamage = 5;
        public float damageGrowthPerLevel = 1.07f;

        public long baseCost = 15;
        public float costGrowthPerLevel = 1.15f;

        public long GetDamageAtLevel(int level)
        {
            if (level <= 0) return 0;
            return (long)(baseDamage * Mathf.Pow(damageGrowthPerLevel, level - 1));
        }

        public long GetUpgradeCost(int currentLevel)
        {
            return (long)(baseCost * Mathf.Pow(costGrowthPerLevel, currentLevel));
        }
    }
}
