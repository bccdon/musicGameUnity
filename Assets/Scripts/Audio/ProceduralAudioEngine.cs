using UnityEngine;
using PulseHighway.Game;

namespace PulseHighway.Audio
{
    public class ProceduralAudioEngine : MonoBehaviour
    {
        public static ProceduralAudioEngine Instance { get; private set; }

        private const int SAMPLE_RATE = 44100;
        private const int CHANNELS = 2;

        private AudioClip cachedClip;
        private int cachedLevelId = -1;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public AudioClip GenerateTrack(LevelConfig config)
        {
            if (cachedLevelId == config.id && cachedClip != null)
                return cachedClip;

            int totalSamples = Mathf.CeilToInt(config.duration * SAMPLE_RATE);
            float[] buffer = new float[totalSamples * CHANNELS];

            // Create genre-specific generator
            GenreGenerator generator = config.genre switch
            {
                "synthwave" => new SynthwaveGenerator(SAMPLE_RATE, config.bpm, config.duration, config.id),
                "dnb" => new DnBGenerator(SAMPLE_RATE, config.bpm, config.duration, config.id),
                "techno" => new TechnoGenerator(SAMPLE_RATE, config.bpm, config.duration, config.id),
                _ => new SynthwaveGenerator(SAMPLE_RATE, config.bpm, config.duration, config.id)
            };

            // Generate
            generator.FillBuffer(buffer, CHANNELS);

            // Apply master effects
            ApplyFadeIn(buffer, CHANNELS, SAMPLE_RATE, 2f);
            ApplyFadeOut(buffer, CHANNELS, SAMPLE_RATE, 3f);
            NormalizeBuffer(buffer);

            // Create AudioClip
            var clip = AudioClip.Create(
                $"Level{config.id}_{config.songTitle}",
                totalSamples,
                CHANNELS,
                SAMPLE_RATE,
                false
            );
            clip.SetData(buffer, 0);

            cachedClip = clip;
            cachedLevelId = config.id;

            return clip;
        }

        public float[] GetSamples(AudioClip clip)
        {
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);
            return samples;
        }

        private void ApplyFadeIn(float[] buffer, int channels, int sampleRate, float fadeTime)
        {
            int fadeSamples = Mathf.CeilToInt(fadeTime * sampleRate);
            for (int i = 0; i < fadeSamples && i * channels < buffer.Length; i++)
            {
                float gain = (float)i / fadeSamples;
                gain = gain * gain; // Exponential curve
                for (int ch = 0; ch < channels; ch++)
                {
                    int idx = i * channels + ch;
                    if (idx < buffer.Length)
                        buffer[idx] *= gain;
                }
            }
        }

        private void ApplyFadeOut(float[] buffer, int channels, int sampleRate, float fadeTime)
        {
            int fadeSamples = Mathf.CeilToInt(fadeTime * sampleRate);
            int totalSamples = buffer.Length / channels;
            int fadeStart = totalSamples - fadeSamples;

            for (int i = fadeStart; i < totalSamples; i++)
            {
                float gain = (float)(totalSamples - i) / fadeSamples;
                gain = gain * gain;
                for (int ch = 0; ch < channels; ch++)
                {
                    int idx = i * channels + ch;
                    if (idx < buffer.Length)
                        buffer[idx] *= gain;
                }
            }
        }

        private void NormalizeBuffer(float[] buffer)
        {
            float maxAmp = 0f;
            for (int i = 0; i < buffer.Length; i++)
            {
                float abs = Mathf.Abs(buffer[i]);
                if (abs > maxAmp) maxAmp = abs;
            }

            if (maxAmp > 0.01f)
            {
                float gain = 0.85f / maxAmp;
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] *= gain;
                }
            }
        }
    }
}
