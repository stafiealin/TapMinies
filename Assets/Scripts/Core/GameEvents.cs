using System;

namespace TapMinies.Core
{
    public static class GameEvents
    {
        public static event Action<long> OnGoldChanged;
        public static event Action<int> OnEnemyDied;
        public static event Action<int> OnStageChanged;

        public static void RaiseGoldChanged(long newGold) => OnGoldChanged?.Invoke(newGold);
        public static void RaiseEnemyDied(int goldAwarded) => OnEnemyDied?.Invoke(goldAwarded);
        public static void RaiseStageChanged(int newStage) => OnStageChanged?.Invoke(newStage);
    }
}
