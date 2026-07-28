using System;
using UnityEngine;
using TapMinies.Core;

namespace TapMinies.Gameplay
{
    public class StageManager : MonoBehaviour
    {
        [SerializeField] private EnemyController enemy;

        [Header("Scaling")]
        [SerializeField] private int baseEnemyHealth = 50;
        [SerializeField] private float healthGrowthPerStage = 1.15f;
        [SerializeField] private long baseGoldReward = 10;
        [SerializeField] private float goldGrowthPerStage = 1.12f;

        [Header("Appearance")]
        [Tooltip("Cycled by stage tier so progression is visible; index = (stage-1)/stagesPerTier.")]
        [SerializeField] private Sprite[] enemyTierSprites;
        [SerializeField] private Sprite bossSprite;
        [SerializeField] private int stagesPerTier = 5;
        [SerializeField] private UnityEngine.UI.Image enemyImage;

        [Header("Boss")]
        [SerializeField] private int bossStageInterval = 5;
        [SerializeField] private float bossHealthMultiplier = 5f;
        [SerializeField] private float bossGoldMultiplier = 3f;
        [SerializeField] private float bossTimeLimit = 20f;

        public int CurrentStage { get; private set; } = 1;
        public int HighestStageCleared { get; private set; }
        public bool IsBossStage => CurrentStage % bossStageInterval == 0;
        public float BossTimeLimit => bossTimeLimit;

        public event Action<float> OnBossTimerChanged;

        private float remainingBossTime;
        private bool timerActive;

        void OnEnable()
        {
            enemy.OnDeath += HandleEnemyDeath;
        }

        void OnDisable()
        {
            enemy.OnDeath -= HandleEnemyDeath;
        }

        public void Initialize(int startingStage, int startingHighestCleared)
        {
            CurrentStage = Mathf.Max(1, startingStage);
            HighestStageCleared = startingHighestCleared;
            SpawnEnemyForCurrentStage();
        }

        void Update()
        {
            if (!timerActive) return;

            remainingBossTime -= Time.deltaTime;
            OnBossTimerChanged?.Invoke(Mathf.Max(0f, remainingBossTime));

            if (remainingBossTime <= 0f)
            {
                timerActive = false;
                HandleBossTimeout();
            }
        }

        void HandleEnemyDeath(EnemyController deadEnemy)
        {
            timerActive = false;

            GameManager.Instance.Currency.AddGold(deadEnemy.GoldReward);
            GameEvents.RaiseEnemyDied((int)deadEnemy.GoldReward);

            HighestStageCleared = CurrentStage;
            CurrentStage++;
            SpawnEnemyForCurrentStage();
        }

        void HandleBossTimeout()
        {
            CurrentStage = Mathf.Max(1, HighestStageCleared);
            SpawnEnemyForCurrentStage();
        }

        void SpawnEnemyForCurrentStage()
        {
            int health = Mathf.RoundToInt(baseEnemyHealth * Mathf.Pow(healthGrowthPerStage, CurrentStage - 1));
            long gold = (long)(baseGoldReward * Mathf.Pow(goldGrowthPerStage, CurrentStage - 1));

            if (IsBossStage)
            {
                health = Mathf.RoundToInt(health * bossHealthMultiplier);
                gold = (long)(gold * bossGoldMultiplier);
                remainingBossTime = bossTimeLimit;
                timerActive = true;
                OnBossTimerChanged?.Invoke(remainingBossTime);
            }

            ApplyEnemyAppearance();
            enemy.Initialize(health, gold);
            GameEvents.RaiseStageChanged(CurrentStage);
        }

        void ApplyEnemyAppearance()
        {
            if (enemyImage == null) return;

            if (IsBossStage && bossSprite != null)
            {
                enemyImage.sprite = bossSprite;
                enemyImage.rectTransform.sizeDelta = new Vector2(480, 480);
                return;
            }

            if (enemyTierSprites != null && enemyTierSprites.Length > 0)
            {
                int tier = Mathf.Clamp((CurrentStage - 1) / Mathf.Max(1, stagesPerTier), 0, enemyTierSprites.Length - 1);
                enemyImage.sprite = enemyTierSprites[tier];
            }
            enemyImage.rectTransform.sizeDelta = new Vector2(384, 384);
        }
    }
}
