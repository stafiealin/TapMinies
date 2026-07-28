using UnityEngine;
using TapMinies.Gameplay;

namespace TapMinies.Core
{
    /// <summary>
    /// Wipes all persistent progress. Destructive and irreversible, so the UI
    /// gates it behind an explicit confirm step rather than a single tap.
    /// </summary>
    public class ProgressResetController : MonoBehaviour
    {
        [SerializeField] private StageManager stageManager;
        [SerializeField] private HeroManager heroManager;
        [SerializeField] private GameObject confirmPanel;

        public void RequestReset()
        {
            if (confirmPanel != null) confirmPanel.SetActive(true);
        }

        public void CancelReset()
        {
            if (confirmPanel != null) confirmPanel.SetActive(false);
        }

        public void ConfirmReset()
        {
            // Order matters: clear hero levels before re-initialising the stage,
            // because Initialize raises OnStageChanged which triggers an autosave.
            GameManager.Instance.Currency.SetGold(0);
            heroManager.LoadLevels(new int[heroManager.HeroCount]);
            stageManager.Initialize(1, 0);

            GameManager.Instance.Save.Delete();

            if (confirmPanel != null) confirmPanel.SetActive(false);
            Debug.Log("[TapMinies] Progress reset to a fresh save.");
        }
    }
}
