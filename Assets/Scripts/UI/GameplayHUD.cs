using UnityEngine;
using UnityEngine.UI;
using TapMinies.Core;
using TapMinies.Gameplay;

namespace TapMinies.UI
{
    public class GameplayHUD : MonoBehaviour
    {
        [SerializeField] private Text goldText;
        [SerializeField] private RectTransform enemyHealthFill;
        [SerializeField] private EnemyController enemy;

        [SerializeField] private Text stageText;
        [SerializeField] private GameObject bossTimerContainer;
        [SerializeField] private RectTransform bossTimerFill;
        [SerializeField] private StageManager stageManager;

        void OnEnable()
        {
            GameEvents.OnGoldChanged += UpdateGold;
            GameEvents.OnStageChanged += UpdateStage;
            enemy.OnHealthChanged += UpdateEnemyHealth;
            stageManager.OnBossTimerChanged += UpdateBossTimer;
        }

        void OnDisable()
        {
            GameEvents.OnGoldChanged -= UpdateGold;
            GameEvents.OnStageChanged -= UpdateStage;
            enemy.OnHealthChanged -= UpdateEnemyHealth;
            stageManager.OnBossTimerChanged -= UpdateBossTimer;
        }

        void Start()
        {
            UpdateGold(GameManager.Instance.Currency.Gold);
            UpdateStage(stageManager.CurrentStage);
        }

        void UpdateGold(long amount)
        {
            goldText.text = $"Gold: {amount}";
        }

        void UpdateEnemyHealth(EnemyController e)
        {
            float fraction = e.MaxHealth > 0 ? (float)e.CurrentHealth / e.MaxHealth : 0f;
            var anchorMax = enemyHealthFill.anchorMax;
            anchorMax.x = fraction;
            enemyHealthFill.anchorMax = anchorMax;
        }

        void UpdateStage(int stage)
        {
            stageText.text = stageManager.IsBossStage ? $"Stage {stage} - BOSS!" : $"Stage {stage}";
            bossTimerContainer.SetActive(stageManager.IsBossStage);
            if (stageManager.IsBossStage)
            {
                UpdateBossTimer(stageManager.BossTimeLimit);
            }
        }

        void UpdateBossTimer(float remaining)
        {
            float fraction = stageManager.BossTimeLimit > 0 ? remaining / stageManager.BossTimeLimit : 0f;
            var anchorMax = bossTimerFill.anchorMax;
            anchorMax.x = Mathf.Clamp01(fraction);
            bossTimerFill.anchorMax = anchorMax;
        }
    }
}
