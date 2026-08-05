using UnityEngine;
using UnityEngine.UI;
using TapMinies.Data;

namespace TapMinies.UI
{
    /// <summary>
    /// Cycles a hero row's portrait through its idle frames, and plays a one-shot
    /// attack sequence when the hero deals idle damage. Falls back to the static
    /// portrait for heroes that don't have generated frame sets yet.
    /// </summary>
    public class HeroPortraitAnimator : MonoBehaviour
    {
        private Image image;
        private HeroData data;
        private Sprite[] currentFrames;
        private int frameIndex;
        private float frameTimer;
        private bool attacking;

        public void Init(Image target, HeroData heroData)
        {
            image = target;
            data = heroData;
            attacking = false;
            frameIndex = 0;
            frameTimer = 0f;
            currentFrames = HasFrames(data.idleFrames) ? data.idleFrames : null;
            ApplyFrame();
        }

        public void PlayAttack()
        {
            if (!HasFrames(data.attackFrames)) return;

            attacking = true;
            currentFrames = data.attackFrames;
            frameIndex = 0;
            frameTimer = 0f;
            ApplyFrame();
        }

        void Update()
        {
            if (currentFrames == null || data.frameRate <= 0f) return;

            frameTimer += Time.deltaTime;
            float frameDuration = 1f / data.frameRate;
            if (frameTimer < frameDuration) return;
            frameTimer -= frameDuration;

            frameIndex++;
            if (frameIndex >= currentFrames.Length)
            {
                frameIndex = 0;
                if (attacking)
                {
                    attacking = false;
                    currentFrames = HasFrames(data.idleFrames) ? data.idleFrames : null;
                }
            }
            ApplyFrame();
        }

        void ApplyFrame()
        {
            image.sprite = currentFrames != null ? currentFrames[frameIndex] : data.portrait;
        }

        static bool HasFrames(Sprite[] frames) => frames != null && frames.Length > 0;
    }
}
