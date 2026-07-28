using UnityEngine;

namespace TapMinies.UI
{
    public class UIPanelToggle : MonoBehaviour
    {
        [SerializeField] private GameObject target;

        public void Toggle()
        {
            target.SetActive(!target.activeSelf);
        }

        public void Close()
        {
            target.SetActive(false);
        }
    }
}
