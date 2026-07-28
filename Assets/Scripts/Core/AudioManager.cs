using UnityEngine;

namespace TapMinies.Core
{
    /// <summary>
    /// Small round-robin voice pool. Every one-shot gets a slight random pitch:
    /// identical repeated samples fatigue the ear fast in a tap game.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioClip tapClip;
        [SerializeField] private AudioClip hitClip;
        [SerializeField] private AudioClip killClip;
        [SerializeField] private AudioClip coinClip;
        [SerializeField] private AudioClip upgradeClip;
        [SerializeField] private AudioClip bossClip;

        [SerializeField, Range(0f, 1f)] private float masterVolume = 0.7f;
        [SerializeField] private int voiceCount = 10;

        private AudioSource[] voices;
        private int nextVoice;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            voices = new AudioSource[voiceCount];
            for (int i = 0; i < voiceCount; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0f;   // 2D
                voices[i] = src;
            }
        }

        void Play(AudioClip clip, float volume, float pitchMin, float pitchMax)
        {
            if (clip == null || voices == null) return;

            var src = voices[nextVoice];
            nextVoice = (nextVoice + 1) % voices.Length;

            src.Stop();
            src.clip = clip;
            src.volume = volume * masterVolume;
            src.pitch = Random.Range(pitchMin, pitchMax);
            src.Play();
        }

        public void PlayTap() => Play(tapClip, 0.45f, 0.94f, 1.10f);
        public void PlayHit() => Play(hitClip, 0.35f, 0.92f, 1.12f);
        public void PlayKill() => Play(killClip, 0.55f, 0.96f, 1.06f);
        public void PlayCoin() => Play(coinClip, 0.40f, 0.97f, 1.05f);
        public void PlayUpgrade() => Play(upgradeClip, 0.60f, 1.00f, 1.00f);
        public void PlayBoss() => Play(bossClip, 0.70f, 1.00f, 1.00f);
    }
}
