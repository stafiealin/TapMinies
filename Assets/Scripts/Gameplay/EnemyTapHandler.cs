using UnityEngine;
using UnityEngine.EventSystems;

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
            enemy.TakeDamage(tapDamage);
        }
    }
}
