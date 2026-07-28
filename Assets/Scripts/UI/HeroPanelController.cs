using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TapMinies.Core;
using TapMinies.Gameplay;

namespace TapMinies.UI
{
    public class HeroPanelController : MonoBehaviour
    {
        [SerializeField] private HeroManager heroManager;
        [SerializeField] private RectTransform rowContainer;
        [Header("9-sliced skin for runtime-built rows")]
        [SerializeField] private Sprite rowSprite;
        [SerializeField] private Sprite buttonSprite;

        private readonly List<HeroRowView> rows = new List<HeroRowView>();

        void Awake()
        {
            BuildRows();
        }

        void OnEnable()
        {
            GameEvents.OnGoldChanged += HandleGoldChanged;
            GameEvents.OnStageChanged += HandleStageChanged;
            heroManager.OnHeroesChanged += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            GameEvents.OnGoldChanged -= HandleGoldChanged;
            GameEvents.OnStageChanged -= HandleStageChanged;
            heroManager.OnHeroesChanged -= Refresh;
        }

        void HandleGoldChanged(long _) => Refresh();
        void HandleStageChanged(int _) => Refresh();

        void BuildRows()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            for (int i = 0; i < heroManager.HeroCount; i++)
            {
                int index = i;

                var rowGo = new GameObject($"HeroRow_{i}", typeof(RectTransform));
                rowGo.transform.SetParent(rowContainer, false);
                var layoutElement = rowGo.AddComponent<LayoutElement>();
                layoutElement.preferredHeight = 160;
                layoutElement.minHeight = 160;
                var bgImg = rowGo.AddComponent<Image>();
                bgImg.color = new Color(1f, 1f, 1f, 0.08f);
                if (rowSprite != null)
                {
                    bgImg.sprite = rowSprite;
                    bgImg.type = Image.Type.Sliced;
                }

                var portraitGo = new GameObject("Portrait", typeof(RectTransform));
                portraitGo.transform.SetParent(rowGo.transform, false);
                var portraitRt = portraitGo.GetComponent<RectTransform>();
                portraitRt.anchorMin = new Vector2(0f, 0.5f);
                portraitRt.anchorMax = new Vector2(0f, 0.5f);
                portraitRt.anchoredPosition = new Vector2(90, 0);
                portraitRt.sizeDelta = new Vector2(120, 120);
                var portraitImg = portraitGo.AddComponent<Image>();
                portraitImg.sprite = heroManager.GetHeroData(index).portrait;
                portraitImg.preserveAspect = true;
                portraitImg.raycastTarget = false;

                var infoGo = new GameObject("Info", typeof(RectTransform));
                infoGo.transform.SetParent(rowGo.transform, false);
                var infoRt = infoGo.GetComponent<RectTransform>();
                infoRt.anchorMin = new Vector2(0f, 0f);
                infoRt.anchorMax = new Vector2(1f, 1f);
                infoRt.offsetMin = new Vector2(180, 10);
                infoRt.offsetMax = new Vector2(-260, -10);
                var infoText = infoGo.AddComponent<Text>();
                infoText.font = font;
                infoText.fontSize = 30;
                infoText.alignment = TextAnchor.MiddleLeft;
                infoText.color = Color.white;

                var btnGo = new GameObject("ActionButton", typeof(RectTransform));
                btnGo.transform.SetParent(rowGo.transform, false);
                var btnRt = btnGo.GetComponent<RectTransform>();
                btnRt.anchorMin = new Vector2(1f, 0.5f);
                btnRt.anchorMax = new Vector2(1f, 0.5f);
                btnRt.anchoredPosition = new Vector2(-130, 0);
                btnRt.sizeDelta = new Vector2(220, 110);
                var btnImg = btnGo.AddComponent<Image>();
                if (buttonSprite != null)
                {
                    btnImg.sprite = buttonSprite;
                    btnImg.type = Image.Type.Sliced;
                }
                var button = btnGo.AddComponent<Button>();

                var btnTextGo = new GameObject("Label", typeof(RectTransform));
                btnTextGo.transform.SetParent(btnGo.transform, false);
                var btnTextRt = btnTextGo.GetComponent<RectTransform>();
                btnTextRt.anchorMin = Vector2.zero;
                btnTextRt.anchorMax = Vector2.one;
                btnTextRt.offsetMin = Vector2.zero;
                btnTextRt.offsetMax = Vector2.zero;
                var actionText = btnTextGo.AddComponent<Text>();
                actionText.font = font;
                actionText.fontSize = 24;
                actionText.alignment = TextAnchor.MiddleCenter;
                actionText.color = Color.white;

                button.onClick.AddListener(() => heroManager.TryUpgradeHero(index));

                rows.Add(new HeroRowView
                {
                    InfoText = infoText,
                    ActionText = actionText,
                    ActionImage = btnImg,
                    PortraitImage = portraitImg,
                    Button = button
                });
            }
        }

        void Refresh()
        {
            for (int i = 0; i < rows.Count; i++)
            {
                var data = heroManager.GetHeroData(i);
                int level = heroManager.GetHeroLevel(i);
                bool unlocked = heroManager.IsUnlocked(i);
                var row = rows[i];

                if (!unlocked)
                {
                    row.InfoText.text = $"{data.heroName}\nLocked - reach Stage {data.unlockStage}";
                    row.ActionText.text = "Locked";
                    row.Button.interactable = false;
                    row.ActionImage.color = new Color(0.3f, 0.3f, 0.3f);
                    // Silhouette the portrait so the player can see what they're working toward
                    row.PortraitImage.color = new Color(0.12f, 0.12f, 0.14f, 0.85f);
                    continue;
                }

                row.PortraitImage.color = Color.white;

                long damage = data.GetDamageAtLevel(level);
                long cost = heroManager.GetUpgradeCost(i);
                bool canAfford = GameManager.Instance.Currency.Gold >= cost;

                row.InfoText.text = level > 0
                    ? $"{data.heroName}  Lv.{level}\nDMG {NumberFormat.Short(damage)}/s"
                    : $"{data.heroName}\nNot hired";
                row.ActionText.text = level > 0
                    ? $"Upgrade\n{NumberFormat.Short(cost)}g"
                    : $"Hire\n{NumberFormat.Short(cost)}g";
                row.Button.interactable = canAfford;
                row.ActionImage.color = canAfford ? new Color(0.2f, 0.6f, 0.3f) : new Color(0.4f, 0.4f, 0.4f);
            }
        }

        private class HeroRowView
        {
            public Text InfoText;
            public Text ActionText;
            public Image ActionImage;
            public Image PortraitImage;
            public Button Button;
        }
    }
}
