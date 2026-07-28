using UnityEngine;

namespace TapMinies.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public CurrencyService Currency { get; private set; }
        public SaveService Save { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Currency = new CurrencyService();
            Save = new SaveService();
        }
    }
}
