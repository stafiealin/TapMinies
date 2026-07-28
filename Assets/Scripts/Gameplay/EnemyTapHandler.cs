using UnityEngine;
using UnityEngine.EventSystems;
using TapMinies.Core;

namespace TapMinies.Gameplay
{
    [RequireComponent(typeof(EnemyController))]
    public class EnemyTapHandler : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private int tapDamage = 10;

        private EnemyController enemy;

        void Awake()
        {
            enemy = GetComponent<EnemyController>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Sound fires before the damage call so it still plays on the killing blow,
            // where TakeDamage early-returns once health is already zero.
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayTap();

            enemy.TakeDamage(tapDamage);
        }
    }
}
