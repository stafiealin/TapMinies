using System.Linq;
using UnityEngine;
using TapMinies.Gameplay;

namespace TapMinies.Core
{
    public class GameSaveController : MonoBehaviour
    {
        [SerializeField] private StageManager stageManager;
        [SerializeField] private HeroManager heroManager;
        [SerializeField] private float autosaveInterval = 30f;

        private float autosaveTimer;
        private bool initialized;

        void Start()
        {
            var save = GameManager.Instance.Save.Load();

            if (save != null)
            {
                GameManager.Instance.Currency.SetGold(save.gold);
                heroManager.LoadLevels(save.heroLevels?.ToArray());
                stageManager.Initialize(save.currentStage, save.highestStageCleared);
            }
            else
            {
                stageManager.Initialize(1, 0);
            }

            initialized = true;
        }

        void OnEnable()
        {
            GameEvents.OnStageChanged += HandleStageChanged;
            heroManager.OnHeroesChanged += HandleHeroesChanged;
        }

        void OnDisable()
        {
            GameEvents.OnStageChanged -= HandleStageChanged;
            heroManager.OnHeroesChanged -= HandleHeroesChanged;
        }

        void Update()
        {
            if (!initialized) return;

            autosaveTimer += Time.deltaTime;
            if (autosaveTimer >= autosaveInterval)
            {
                autosaveTimer = 0f;
                SaveNow();
            }
        }

        void HandleStageChanged(int _)
        {
            if (!initialized) return;
            SaveNow();
        }

        void HandleHeroesChanged()
        {
            if (!initialized) return;
            SaveNow();
        }

        void SaveNow()
        {
            var data = new SaveData
            {
                gold = GameManager.Instance.Currency.Gold,
                currentStage = stageManager.CurrentStage,
                highestStageCleared = stageManager.HighestStageCleared,
                heroLevels = heroManager.GetAllLevelsSnapshot().ToList()
            };

            GameManager.Instance.Save.Save(data);
            autosaveTimer = 0f;
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (initialized && pauseStatus) SaveNow();
        }

        void OnApplicationQuit()
        {
            if (initialized) SaveNow();
        }
    }
}
