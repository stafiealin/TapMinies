namespace TapMinies.Core
{
    public class CurrencyService
    {
        public long Gold { get; private set; }

        public void SetGold(long amount)
        {
            Gold = amount;
            GameEvents.RaiseGoldChanged(Gold);
        }

        public void AddGold(long amount)
        {
            Gold += amount;
            GameEvents.RaiseGoldChanged(Gold);
        }

        public bool TrySpendGold(long amount)
        {
            if (amount > Gold) return false;
            Gold -= amount;
            GameEvents.RaiseGoldChanged(Gold);
            return true;
        }
    }
}
