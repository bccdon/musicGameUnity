using UnityEngine;

namespace PulseHighway.Audio
{
    public class TechnoGenerator : GenreGenerator
    {
        private float acidFilterState;
        private float padFilterState;

        private readonly int[] acidNotes = { 36, 39, 41, 43, 48, 41, 39, 36 }; // Acid pattern

        public TechnoGenerator(int sampleRate, int bpm, float duration, int seed)
            : base(sampleRate, bpm, duration, seed) { }

        public override void FillBuffer(float[] buffer, int channels)
        {
            int totalSamples = buffer.Length / channels;

            for (int i = 0; i < totalSamples; i++)
            {
                float time = (float)i / sampleRate;
                float intensity = GetSectionIntensity(time);
                float section = GetSongSection(time);

                float sample = 0f;

                // Kick - 4 on the floor with rumble
                sample += GenerateKick(time, intensity) * 0.5f;

                // Clap on 2 and 4
                if (section >= 1f)
                    sample += GenerateClap(time, intensity) * 0.25f;

                // Off-beat hi-hat
                sample += GenerateHiHat(time, intensity) * 0.18f;

                // Ride cymbal during drop
                if (section >= 2f)
                    sample += GenerateRide(time, intensity) * 0.1f;

                // Acid 303 bass line
                if (section >= 1f)
                    sample += GenerateAcidBass(time, intensity) * 0.35f;

                // Atmospheric pad
                sample += GenerateAtmosphere(time, intensity) * 0.12f;

                // Sub bass
                sample += GenerateSubBass(time, intensity) * 0.25f;

                sample *= 0.65f;
                sample = SynthVoice.SoftClip(sample);

                float stereoMod = Mathf.Sin(time * 0.7f) * 0.12f;
                AddStereoSample(buffer, i, sample * (1f + stereoMod), sample * (1f - stereoMod), channels);
            }
        }

        private float GenerateKick(float time, float intensity)
        {
            float pos = time % beatDuration;
            if (pos > 0.35f) return 0f;

            // Rumble kick - longer tail than synthwave
            float pitchSweep = 200f * Mathf.Exp(-pos * 25f) + 40f;
            float phase = SynthVoice.GetPhase(time, pitchSweep);
            float env = SynthVoice.ADSR(pos, 0.3f, 0.003f, 0.15f, 0.15f, 0.1f);

            // Add click transient
            float click = SynthVoice.Oscillator(0, WaveformType.Noise) *
                         Mathf.Exp(-pos * 200f) * 0.3f;

            return (SynthVoice.Oscillator(phase, WaveformType.Sine) + click) * env * intensity;
        }

        private float GenerateClap(float time, float intensity)
        {
            float halfBar = beatDuration * 2f;
            float pos = time % halfBar;
            float clapTime = pos - beatDuration;
            if (clapTime < 0 || clapTime > 0.15f) return 0f;

            // Multiple noise bursts for clap texture
            float env1 = Mathf.Exp(-clapTime * 50f) * 0.5f;
            float env2 = Mathf.Exp(-(clapTime - 0.01f) * 40f) * 0.5f;
            float env3 = Mathf.Exp(-(clapTime - 0.02f) * 35f) * 0.7f;

            float noise = SynthVoice.Oscillator(0, WaveformType.Noise);
            float total = noise * (env1 + (clapTime > 0.01f ? env2 : 0f) + (clapTime > 0.02f ? env3 : 0f));

            return total * intensity;
        }

        private float GenerateHiHat(float time, float intensity)
        {
            // Off-beat hi-hats (between kicks)
            float halfBeat = beatDuration * 0.5f;
            float pos = (time + halfBeat * 0.5f) % halfBeat;
            float hatTime = pos - halfBeat * 0.5f;

            if (hatTime < 0 || hatTime > 0.06f) return 0f;

            float env = Mathf.Exp(-hatTime * 80f);
            float noise = SynthVoice.Oscillator(0, WaveformType.Noise);

            return noise * env * intensity * 0.5f;
        }

        private float GenerateRide(float time, float intensity)
        {
            float sixteenthDur = beatDuration * 0.25f;
            float pos = time % sixteenthDur;
            if (pos > 0.08f) return 0f;

            int idx = Mathf.FloorToInt(time / sixteenthDur);
            if (idx % 3 != 0) return 0f; // Sparse ride pattern

            float env = Mathf.Exp(-pos * 30f);
            float noise = SynthVoice.Oscillator(0, WaveformType.Noise);
            float metallic = SynthVoice.Oscillator(SynthVoice.GetPhase(time, 7500f), WaveformType.Sine);

            return (noise * 0.5f + metallic * 0.5f) * env * (intensity - 0.4f) * 0.3f;
        }

        private float GenerateAcidBass(float time, float intensity)
        {
            float sixteenthDur = beatDuration * 0.25f;
            int noteIdx = Mathf.FloorToInt(time / sixteenthDur) % acidNotes.Length;
            float noteTime = time % sixteenthDur;
            float freq = 440f * Mathf.Pow(2f, (acidNotes[noteIdx] - 69) / 12f);

            float env = SynthVoice.ADSR(noteTime, sixteenthDur * 0.7f, 0.005f, 0.03f, 0.5f, 0.02f);

            float phase = SynthVoice.GetPhase(time, freq);
            float saw = SynthVoice.Oscillator(phase, WaveformType.Sawtooth);

            // Acid filter envelope - cutoff sweeps up then down per note
            float filterEnv = Mathf.Exp(-noteTime * 20f);
            float cutoff = 0.05f + filterEnv * intensity * 0.35f;
            float filtered = SynthVoice.LowPassFilter(saw, ref acidFilterState, cutoff);

            // Distortion for acid character
            float distorted = SynthVoice.Distortion(filtered * 1.5f, 3f);

            return distorted * env * intensity * 0.7f;
        }

        private float GenerateAtmosphere(float time, float intensity)
        {
            // Slow evolving pad
            float freq1 = 440f * Mathf.Pow(2f, (48 - 69) / 12f); // C3
            float freq2 = 440f * Mathf.Pow(2f, (55 - 69) / 12f); // G3

            float lfo = Mathf.Sin(time * 0.15f) * 0.5f + 0.5f;
            float phase1 = SynthVoice.GetPhase(time, freq1);
            float phase2 = SynthVoice.GetPhase(time, freq2 * (1f + lfo * 0.002f));

            float osc = SynthVoice.Oscillator(phase1, WaveformType.Sawtooth) * 0.3f +
                        SynthVoice.Oscillator(phase2, WaveformType.Sawtooth) * 0.3f;

            float cutoff = 0.03f + lfo * 0.04f;
            float filtered = SynthVoice.LowPassFilter(osc, ref padFilterState, cutoff);

            return filtered * intensity;
        }

        private float GenerateSubBass(float time, float intensity)
        {
            int noteIdx = Mathf.FloorToInt(time / barDuration) % acidNotes.Length;
            float freq = 440f * Mathf.Pow(2f, (acidNotes[noteIdx] - 69) / 12f) * 0.5f;

            float phase = SynthVoice.GetPhase(time, freq);
            float env = SynthVoice.ADSR(time % barDuration, barDuration * 0.9f, 0.02f, 0.1f, 0.9f, 0.05f);

            return SynthVoice.Oscillator(phase, WaveformType.Sine) * env * intensity * 0.6f;
        }
    }
}
