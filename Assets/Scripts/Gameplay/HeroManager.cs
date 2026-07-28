using System;
using UnityEngine;
using TapMinies.Core;
using TapMinies.Data;

namespace TapMinies.Gameplay
{
    public class HeroManager : MonoBehaviour
    {
        [SerializeField] private HeroData[] heroRoster;
        [SerializeField] private EnemyController enemy;
        [SerializeField] private StageManager stageManager;

        private int[] heroLevels;
        private float tickAccumulator;

        public event Action OnHeroesChanged;

        public int HeroCount => heroRoster.Length;

        void Awake()
        {
            heroLevels = new int[heroRoster.Length];
        }

        void Update()
        {
            tickAccumulator += Time.deltaTime;
            if (tickAccumulator >= 1f)
            {
                tickAccumulator -= 1f;
                ApplyIdleDamage();
            }
        }

        void ApplyIdleDamage()
        {
            if (enemy.CurrentHealth <= 0) return;

            long totalDamage = 0;
            for (int i = 0; i < heroRoster.Length; i++)
            {
                if (heroLevels[i] > 0)
                {
                    totalDamage += heroRoster[i].GetDamageAtLevel(heroLevels[i]);
                }
            }

            if (totalDamage > 0)
            {
                int damage = (int)Mathf.Min(totalDamage, int.MaxValue);
                enemy.TakeDamage(damage);
            }
        }

        public HeroData GetHeroData(int index) => heroRoster[index];
        public int GetHeroLevel(int index) => heroLevels[index];

        public int[] GetAllLevelsSnapshot() => (int[])heroLevels.Clone();

        public void LoadLevels(int[] levels)
        {
            for (int i = 0; i < heroLevels.Length; i++)
            {
                heroLevels[i] = (levels != null && i < levels.Length) ? levels[i] : 0;
            }
            OnHeroesChanged?.Invoke();
        }
        public bool IsUnlocked(int index) => stageManager.CurrentStage >= heroRoster[index].unlockStage;
        public long GetUpgradeCost(int index) => heroRoster[index].GetUpgradeCost(heroLevels[index]);

        public bool TryUpgradeHero(int index)
        {
            if (!IsUnlocked(index)) return false;

            long cost = GetUpgradeCost(index);
            if (!GameManager.Instance.Currency.TrySpendGold(cost)) return false;

            heroLevels[index]++;
            OnHeroesChanged?.Invoke();
            return true;
        }
    }
}
