using UnityEngine;
using UnityEngine.UI;
using TapMinies.Gameplay;

namespace TapMinies.UI
{
    /// <summary>
    /// Shows hired heroes as animated characters on the battle screen, separate from the
    /// static roster panel. Each hero appears once hired and plays its own idle loop and
    /// attack animation independently as HeroManager reports its damage ticks.
    /// </summary>
    public class HeroStageController : MonoBehaviour
    {
        [SerializeField] private HeroManager heroManager;
        [SerializeField] private RectTransform slotContainer;
        [SerializeField] private Vector2 slotSize = new Vector2(90, 90);
        [SerializeField] private float slotSpacing = 100f;

        private GameObject[] slotObjects;
        private HeroPortraitAnimator[] slotAnimators;

        void Awake()
        {
            BuildSlots();
        }

        void OnEnable()
        {
            heroManager.OnHeroesChanged += RefreshVisibility;
            heroManager.OnHeroAttacked += HandleHeroAttacked;
            RefreshVisibility();
        }

        void OnDisable()
        {
            heroManager.OnHeroesChanged -= RefreshVisibility;
            heroManager.OnHeroAttacked -= HandleHeroAttacked;
        }

        void BuildSlots()
        {
            int count = heroManager.HeroCount;
            slotObjects = new GameObject[count];
            slotAnimators = new HeroPortraitAnimator[count];

            for (int i = 0; i < count; i++)
            {
                var slotGo = new GameObject($"HeroStage_{i}", typeof(RectTransform));
                slotGo.transform.SetParent(slotContainer, false);

                var rt = slotGo.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(0f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = slotSize;
                rt.anchoredPosition = new Vector2(slotSpacing * i, 0f);

                var image = slotGo.AddComponent<Image>();
                image.preserveAspect = true;
                image.raycastTarget = false;

                var animator = slotGo.AddComponent<HeroPortraitAnimator>();
                animator.Init(image, heroManager.GetHeroData(i));

                slotGo.SetActive(false);
                slotObjects[i] = slotGo;
                slotAnimators[i] = animator;
            }
        }

        void RefreshVisibility()
        {
            for (int i = 0; i < slotObjects.Length; i++)
                slotObjects[i].SetActive(heroManager.GetHeroLevel(i) > 0);
        }

        void HandleHeroAttacked(int index)
        {
            if (index >= 0 && index < slotAnimators.Length && slotObjects[index].activeSelf)
                slotAnimators[index].PlayAttack();
        }
    }
}
