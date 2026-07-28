using System;
using UnityEngine;

namespace TapMinies.Gameplay
{
    public class EnemyController : MonoBehaviour
    {
        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public long GoldReward { get; private set; }

        public event Action<EnemyController> OnHealthChanged;
        public event Action<EnemyController> OnDeath;

        public void Initialize(int maxHealth, long goldReward)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            GoldReward = goldReward;
            OnHealthChanged?.Invoke(this);
        }

        public void TakeDamage(int amount)
        {
            if (CurrentHealth <= 0) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            OnHealthChanged?.Invoke(this);

            if (CurrentHealth <= 0)
            {
                OnDeath?.Invoke(this);
            }
        }
    }
}
