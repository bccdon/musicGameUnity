using UnityEngine;

namespace PulseHighway.Audio
{
    public class DnBGenerator : GenreGenerator
    {
        private float bassFilterState;
        private float reesePrevIn, reesePrevOut;

        private readonly int[] bassNotes = { 33, 35, 36, 38, 40 }; // A1-E2
        private readonly int[] leadNotes = { 60, 63, 65, 67, 70, 72 }; // C4 minor pentatonic

        public DnBGenerator(int sampleRate, int bpm, float duration, int seed)
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

                // Breakbeat kick
                sample += GenerateKick(time, intensity) * 0.5f;

                // Snare
                if (section >= 0.5f)
                    sample += GenerateSnare(time, intensity) * 0.35f;

                // Hi-hat
                sample += GenerateHiHat(time, intensity) * 0.15f;

                // Reese bass
                if (section >= 1f)
                    sample += GenerateReeseBass(time, intensity) * 0.35f;

                // Stab/lead during drop
                if (section >= 2f)
                    sample += GenerateStab(time, intensity) * 0.2f;

                // Sub bass
                sample += GenerateSubBass(time, intensity) * 0.3f;

                sample *= 0.65f;
                sample = SynthVoice.SoftClip(sample);

                float stereoMod = Mathf.Sin(time * 0.5f) * 0.1f;
                AddStereoSample(buffer, i, sample * (1f + stereoMod), sample * (1f - stereoMod), channels);
            }
        }

        private float GenerateKick(float time, float intensity)
        {
            // DnB breakbeat pattern: kick on 1 and 3.5 (of a 2-beat pattern)
            float twoBeats = beatDuration * 2f;
            float pos = time % twoBeats;

            float kickTime = -1f;
            if (pos < 0.15f) kickTime = pos;
            else if (pos > beatDuration * 1.5f && pos < beatDuration * 1.5f + 0.15f)
                kickTime = pos - beatDuration * 1.5f;

            if (kickTime < 0) return 0f;

            float pitchSweep = 180f * Mathf.Exp(-kickTime * 35f) + 50f;
            float phase = SynthVoice.GetPhase(time, pitchSweep);
            float env = SynthVoice.ADSR(kickTime, 0.12f, 0.003f, 0.06f, 0.3f, 0.05f);

            return SynthVoice.Oscillator(phase, WaveformType.Sine) * env * intensity;
        }

        private float GenerateSnare(float time, float intensity)
        {
            float twoBeats = beatDuration * 2f;
            float pos = time % twoBeats;
            float snareTime = pos - beatDuration;
            if (snareTime < 0 || snareTime > 0.25f) return 0f;

            float env = SynthVoice.ADSR(snareTime, 0.25f, 0.002f, 0.06f, 0.15f, 0.12f);
            float noise = SynthVoice.Oscillator(0, WaveformType.Noise);
            float tone = SynthVoice.Oscillator(SynthVoice.GetPhase(time, 220f), WaveformType.Sine);

            return (noise * 0.75f + tone * 0.25f) * env * intensity;
        }

        private float GenerateHiHat(float time, float intensity)
        {
            float sixteenthDur = beatDuration * 0.25f;
            float pos = time % sixteenthDur;
            if (pos > 0.04f) return 0f;

            bool accent = (Mathf.FloorToInt(time / sixteenthDur) % 2) == 0;
            float vol = accent ? 1f : 0.5f;
            float env = Mathf.Exp(-pos * 120f);
            float noise = SynthVoice.Oscillator(0, WaveformType.Noise);

            return noise * env * intensity * vol * 0.6f;
        }

        private float GenerateReeseBass(float time, float intensity)
        {
            int barIndex = Mathf.FloorToInt(time / barDuration);
            int noteIdx = barIndex % bassNotes.Length;
            float freq = 440f * Mathf.Pow(2f, (bassNotes[noteIdx] - 69) / 12f);

            float noteInBar = time % barDuration;
            float env = SynthVoice.ADSR(noteInBar, barDuration * 0.9f, 0.05f, 0.1f, 0.8f, 0.1f);

            // Detuned sawtooth pair = Reese bass
            float phase1 = SynthVoice.GetPhase(time, freq);
            float phase2 = SynthVoice.GetPhase(time, freq * 1.008f);
            float osc = SynthVoice.Oscillator(phase1, WaveformType.Sawtooth) * 0.5f +
                        SynthVoice.Oscillator(phase2, WaveformType.Sawtooth) * 0.5f;

            // Modulating filter
            float cutoffMod = 0.08f + 0.1f * Mathf.Sin(time * 2f) * intensity;
            float filtered = SynthVoice.LowPassFilter(osc, ref bassFilterState, cutoffMod);

            return filtered * env * intensity;
        }

        private float GenerateStab(float time, float intensity)
        {
            float eighthDur = beatDuration * 0.5f;
            int eighthIdx = Mathf.FloorToInt(time / eighthDur);
            float noteTime = time % eighthDur;

            // Only play on certain eighths
            if (eighthIdx % 4 == 0 || eighthIdx % 4 == 3)
            {
                int noteIdx = (eighthIdx / 4) % leadNotes.Length;
                float freq = 440f * Mathf.Pow(2f, (leadNotes[noteIdx] - 69) / 12f);

                float env = SynthVoice.ADSR(noteTime, eighthDur * 0.6f, 0.005f, 0.03f, 0.4f, 0.03f);
                float phase = SynthVoice.GetPhase(time, freq);
                float osc = SynthVoice.Oscillator(phase, WaveformType.Square);

                return osc * env * (intensity - 0.3f);
            }
            return 0f;
        }

        private float GenerateSubBass(float time, float intensity)
        {
            int barIndex = Mathf.FloorToInt(time / barDuration);
            int noteIdx = barIndex % bassNotes.Length;
            float freq = 440f * Mathf.Pow(2f, (bassNotes[noteIdx] - 69) / 12f) * 0.5f; // Octave down

            float phase = SynthVoice.GetPhase(time, freq);
            float env = SynthVoice.ADSR(time % barDuration, barDuration * 0.95f, 0.02f, 0.1f, 0.9f, 0.05f);

            return SynthVoice.Oscillator(phase, WaveformType.Sine) * env * intensity * 0.6f;
        }
    }
}
