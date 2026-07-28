using UnityEngine;
using UnityEngine.UI;
using TapMinies.Core;
using TapMinies.UI;

namespace TapMinies.Gameplay
{
    /// <summary>
    /// All the non-functional feedback for the enemy: squash on hit, colour flash,
    /// damage numbers, death burst, and a shake on boss kills.
    /// Kept separate from EnemyController so game logic stays testable without visuals.
    /// </summary>
    [RequireComponent(typeof(EnemyController))]
    public class EnemyJuice : MonoBehaviour
    {
        [SerializeField] private EffectsLayer effects;
        [SerializeField] private StageManager stageManager;
        [SerializeField] private Image enemyImage;

        [Header("Feel")]
        [SerializeField] private float punchScale = 0.12f;
        [SerializeField] private float punchRecovery = 9f;
        [SerializeField] private float flashRecovery = 7f;

        private EnemyController enemy;
        private RectTransform rect;
        private Vector2 restPosition;
        private float punch;
        private float flash;
        private float shake;

        void Awake()
        {
            enemy = GetComponent<EnemyController>();
            rect = GetComponent<RectTransform>();
            restPosition = rect.anchoredPosition;
        }

        void OnEnable()
        {
            enemy.OnDamaged += HandleDamaged;
            enemy.OnDeath += HandleDeath;
        }

        void OnDisable()
        {
            enemy.OnDamaged -= HandleDamaged;
            enemy.OnDeath -= HandleDeath;
        }

        void HandleDamaged(int amount)
        {
            punch = 1f;
            flash = 1f;
            if (effects != null)
                effects.SpawnDamageNumber(restPosition, amount, false);
        }

        void HandleDeath(EnemyController _)
        {
            bool wasBoss = stageManager != null && stageManager.IsBossStage;

            if (effects != null)
            {
                effects.SpawnBurst(restPosition,
                    wasBoss ? new Color(1f, 0.75f, 0.3f) : new Color(1f, 1f, 1f, 0.95f),
                    wasBoss ? 26 : 14);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayKill();
                AudioManager.Instance.PlayCoin();
                if (wasBoss) AudioManager.Instance.PlayBoss();
            }

            shake = wasBoss ? 1f : 0.35f;
            punch = 1f;
        }

        void Update()
        {
            float dt = Time.deltaTime;

            // Squash-and-stretch: scale up on hit, spring back.
            if (punch > 0f)
            {
                punch = Mathf.Max(0f, punch - dt * punchRecovery);
                float s = 1f + punchScale * punch;
                transform.localScale = new Vector3(s, 1f + punchScale * punch * 0.6f, 1f);
            }
            else if (transform.localScale != Vector3.one)
            {
                transform.localScale = Vector3.one;
            }

            // White flash on impact.
            if (enemyImage != null && flash > 0f)
            {
                flash = Mathf.Max(0f, flash - dt * flashRecovery);
                enemyImage.color = Color.Lerp(Color.white, new Color(1.6f, 1.6f, 1.6f), flash);
            }

            // Positional shake, strongest on boss death.
            if (shake > 0f)
            {
                shake = Mathf.Max(0f, shake - dt * 3.2f);
                float mag = 26f * shake * shake;
                rect.anchoredPosition = restPosition + new Vector2(
                    Random.Range(-mag, mag), Random.Range(-mag, mag));
            }
            else if (rect.anchoredPosition != restPosition)
            {
                rect.anchoredPosition = restPosition;
            }
        }
    }
}
