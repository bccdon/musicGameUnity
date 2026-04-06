using UnityEngine;

namespace PulseHighway.Audio
{
    public enum WaveformType
    {
        Sine,
        Sawtooth,
        Square,
        Triangle,
        Noise
    }

    public static class SynthVoice
    {
        private static System.Random noiseRng = new System.Random(42);

        public static float Oscillator(float phase, WaveformType type)
        {
            // phase is 0..1 within one cycle
            float p = phase % 1f;
            if (p < 0) p += 1f;

            return type switch
            {
                WaveformType.Sine => Mathf.Sin(p * Mathf.PI * 2f),
                WaveformType.Sawtooth => 2f * p - 1f,
                WaveformType.Square => p < 0.5f ? 1f : -1f,
                WaveformType.Triangle => 4f * Mathf.Abs(p - 0.5f) - 1f,
                WaveformType.Noise => (float)(noiseRng.NextDouble() * 2.0 - 1.0),
                _ => 0f
            };
        }

        public static float GetPhase(float time, float frequency)
        {
            return (time * frequency) % 1f;
        }

        public static float ADSR(float noteTime, float noteLength, float attack, float decay,
            float sustainLevel, float release)
        {
            if (noteTime < 0) return 0f;

            if (noteTime < attack)
                return noteTime / attack;

            if (noteTime < attack + decay)
            {
                float decayProgress = (noteTime - attack) / decay;
                return 1f - (1f - sustainLevel) * decayProgress;
            }

            if (noteTime < noteLength)
                return sustainLevel;

            float releaseTime = noteTime - noteLength;
            if (releaseTime < release)
                return sustainLevel * (1f - releaseTime / release);

            return 0f;
        }

        public static float LowPassFilter(float input, ref float prevOutput, float cutoffNormalized)
        {
            float alpha = Mathf.Clamp01(cutoffNormalized);
            float output = prevOutput + alpha * (input - prevOutput);
            prevOutput = output;
            return output;
        }

        public static float HighPassFilter(float input, ref float prevInput, ref float prevOutput, float cutoffNormalized)
        {
            float alpha = Mathf.Clamp01(1f - cutoffNormalized);
            float output = alpha * (prevOutput + input - prevInput);
            prevInput = input;
            prevOutput = output;
            return output;
        }

        public static float Distortion(float input, float drive)
        {
            return Mathf.Atan(input * drive) / Mathf.Atan(drive);
        }

        public static float SoftClip(float input)
        {
            if (input > 1f) return 1f;
            if (input < -1f) return -1f;
            return 1.5f * input - 0.5f * input * input * input;
        }
    }
}
