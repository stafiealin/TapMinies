using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TapMinies.Core;

namespace TapMinies.UI
{
    /// <summary>
    /// Pooled transient UI feedback (floating damage numbers + spark bursts).
    /// Pooled rather than Instantiate/Destroy because a tap game spawns these
    /// many times per second and GC spikes are felt as stutter on mobile.
    /// </summary>
    public class EffectsLayer : MonoBehaviour
    {
        [SerializeField] private Sprite sparkSprite;
        [SerializeField] private int damageTextPool = 16;
        [SerializeField] private int sparkPool = 48;

        private class FloatingText
        {
            public Text Label;
            public RectTransform Rect;
            public float Life;
            public float Duration;
            public Vector2 Origin;
            public Vector2 Drift;
        }

        private class Spark
        {
            public Image Image;
            public RectTransform Rect;
            public float Life;
            public float Duration;
            public Vector2 Origin;
            public Vector2 Velocity;
            public float StartSize;
        }

        private readonly List<FloatingText> texts = new List<FloatingText>();
        private readonly List<Spark> sparks = new List<Spark>();

        void Awake()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            for (int i = 0; i < damageTextPool; i++)
            {
                var go = new GameObject($"DmgText_{i}", typeof(RectTransform));
                go.transform.SetParent(transform, false);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(300, 70);

                var label = go.AddComponent<Text>();
                label.font = font;
                label.fontSize = 46;
                label.fontStyle = FontStyle.Bold;
                label.alignment = TextAnchor.MiddleCenter;
                label.raycastTarget = false;

                var outline = go.AddComponent<Outline>();
                outline.effectColor = new Color(0f, 0f, 0f, 0.75f);
                outline.effectDistance = new Vector2(2.5f, -2.5f);

                go.SetActive(false);
                texts.Add(new FloatingText { Label = label, Rect = rt });
            }

            for (int i = 0; i < sparkPool; i++)
            {
                var go = new GameObject($"Spark_{i}", typeof(RectTransform));
                go.transform.SetParent(transform, false);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(28, 28);

                var img = go.AddComponent<Image>();
                img.sprite = sparkSprite;
                img.raycastTarget = false;

                go.SetActive(false);
                sparks.Add(new Spark { Image = img, Rect = rt });
            }
        }

        public void SpawnDamageNumber(Vector2 anchoredPos, long amount, bool emphasised)
        {
            var slot = texts.Find(t => !t.Label.gameObject.activeSelf);
            if (slot == null) return;   // pool exhausted: drop it, never grow mid-frame

            slot.Label.text = NumberFormat.Short(amount);
            slot.Label.color = emphasised ? new Color(1f, 0.85f, 0.25f) : Color.white;
            slot.Label.fontSize = emphasised ? 60 : 44;

            slot.Origin = anchoredPos + new Vector2(Random.Range(-55f, 55f), Random.Range(-25f, 25f));
            slot.Drift = new Vector2(Random.Range(-35f, 35f), Random.Range(150f, 210f));
            slot.Duration = emphasised ? 0.95f : 0.7f;
            slot.Life = 0f;
            slot.Rect.anchoredPosition = slot.Origin;
            slot.Label.gameObject.SetActive(true);
        }

        public void SpawnBurst(Vector2 anchoredPos, Color tint, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var slot = sparks.Find(s => !s.Image.gameObject.activeSelf);
                if (slot == null) return;

                float angle = Random.Range(0f, Mathf.PI * 2f);
                float speed = Random.Range(260f, 620f);

                slot.Origin = anchoredPos;
                slot.Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
                slot.Duration = Random.Range(0.35f, 0.6f);
                slot.StartSize = Random.Range(18f, 40f);
                slot.Life = 0f;
                slot.Image.color = tint;
                slot.Rect.anchoredPosition = anchoredPos;
                slot.Rect.sizeDelta = new Vector2(slot.StartSize, slot.StartSize);
                slot.Image.gameObject.SetActive(true);
            }
        }

        void Update()
        {
            float dt = Time.deltaTime;

            for (int i = 0; i < texts.Count; i++)
            {
                var t = texts[i];
                if (!t.Label.gameObject.activeSelf) continue;

                t.Life += dt;
                float p = t.Life / t.Duration;
                if (p >= 1f)
                {
                    t.Label.gameObject.SetActive(false);
                    continue;
                }

                // Ease-out rise so the number "pops" then settles.
                float rise = 1f - Mathf.Pow(1f - p, 2f);
                t.Rect.anchoredPosition = t.Origin + t.Drift * rise;

                var c = t.Label.color;
                c.a = p < 0.65f ? 1f : Mathf.InverseLerp(1f, 0.65f, p);
                t.Label.color = c;

                float pop = p < 0.18f ? Mathf.Lerp(0.6f, 1.12f, p / 0.18f) : Mathf.Lerp(1.12f, 1f, (p - 0.18f) / 0.82f);
                t.Rect.localScale = Vector3.one * pop;
            }

            for (int i = 0; i < sparks.Count; i++)
            {
                var s = sparks[i];
                if (!s.Image.gameObject.activeSelf) continue;

                s.Life += dt;
                float p = s.Life / s.Duration;
                if (p >= 1f)
                {
                    s.Image.gameObject.SetActive(false);
                    continue;
                }

                s.Velocity += new Vector2(0f, -1400f * dt);          // gravity
                s.Velocity *= 1f - (2.6f * dt);                       // drag
                s.Rect.anchoredPosition += s.Velocity * dt;

                float size = Mathf.Lerp(s.StartSize, 0f, p * p);
                s.Rect.sizeDelta = new Vector2(size, size);

                var c = s.Image.color;
                c.a = 1f - p;
                s.Image.color = c;
            }
        }
    }
}
