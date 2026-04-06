using UnityEngine;

namespace PulseHighway.Audio
{
    public class SynthwaveGenerator : GenreGenerator
    {
        // Filter states
        private float bassFilterState;
        private float leadFilterState;
        private float padFilterState;

        // Musical data
        private readonly int[] bassNotes = { 36, 38, 40, 41, 43, 45 }; // C2-A2
        private readonly int[][] chordProgressions = {
            new[] { 48, 52, 55 }, // C major
            new[] { 53, 57, 60 }, // F major
            new[] { 55, 59, 62 }, // G major
            new[] { 45, 48, 52 }, // A minor
        };
        private readonly int[] arpNotes = { 60, 64, 67, 72, 67, 64 }; // C major arp up-down

        public SynthwaveGenerator(int sampleRate, int bpm, float duration, int seed)
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

                // Kick drum - 4 on the floor
                sample += GenerateKick(time, intensity) * 0.45f;

                // Snare on 2 and 4
                if (section >= 1f)
                    sample += GenerateSnare(time, intensity) * 0.3f;

                // Hi-hat pattern
                if (section >= 1f)
                    sample += GenerateHiHat(time, intensity) * 0.15f;

                // Bass line
                sample += GenerateBass(time, intensity) * 0.35f;

                // Arp/Lead during build and drop
                if (section >= 1f)
                    sample += GenerateArp(time, intensity) * 0.25f;

                // Pad
                sample += GeneratePad(time, intensity) * 0.15f;

                // Master compression and limiting
                sample *= 0.7f;
                sample = SynthVoice.SoftClip(sample);

                // Stereo width
                float stereoMod = Mathf.Sin(time * 0.3f) * 0.15f;
                float left = sample * (1f + stereoMod);
                float right = sample * (1f - stereoMod);

                AddStereoSample(buffer, i, left, right, channels);
            }
        }

        private float GenerateKick(float time, float intensity)
        {
            float beatPos = (time % beatDuration) / beatDuration;
            if (beatPos > 0.3f) return 0f;

            float kickTime = beatPos * beatDuration;
            float pitchSweep = 150f * Mathf.Exp(-kickTime * 30f) + 45f;
            float phase = SynthVoice.GetPhase(time, pitchSweep);
            float env = SynthVoice.ADSR(kickTime, 0.15f, 0.005f, 0.1f, 0.2f, 0.05f);

            return SynthVoice.Oscillator(phase, WaveformType.Sine) * env * intensity;
        }

        private float GenerateSnare(float time, float intensity)
        {
            float halfBarPos = time % (beatDuration * 2f);
            float snareTime = halfBarPos - beatDuration;
            if (snareTime < 0 || snareTime > 0.2f) return 0f;

            float env = SynthVoice.ADSR(snareTime, 0.2f, 0.003f, 0.08f, 0.1f, 0.1f);
            float noise = SynthVoice.Oscillator(0, WaveformType.Noise);
            float tone = SynthVoice.Oscillator(SynthVoice.GetPhase(time, 200f), WaveformType.Sine);

            return (noise * 0.7f + tone * 0.3f) * env * intensity;
        }

        private float GenerateHiHat(float time, float intensity)
        {
            float eighthBeat = beatDuration * 0.5f;
            float pos = time % eighthBeat;
            if (pos > 0.05f) return 0f;

            float env = Mathf.Exp(-pos * 100f);
            float noise = SynthVoice.Oscillator(0, WaveformType.Noise);
            float hpIn = noise;
            float hpPrevIn = 0f, hpPrevOut = 0f;
            float filtered = SynthVoice.HighPassFilter(hpIn, ref hpPrevIn, ref hpPrevOut, 0.1f);

            return filtered * env * intensity * 0.5f;
        }

        private float GenerateBass(float time, float intensity)
        {
            int barIndex = Mathf.FloorToInt(time / barDuration);
            int noteIndex = barIndex % bassNotes.Length;
            float freq = MidiToFreq(bassNotes[noteIndex]);

            float beatInBar = (time % barDuration) / beatDuration;
            int eighthNote = Mathf.FloorToInt(beatInBar * 2f);
            float eighthTime = (beatInBar * 2f - eighthNote) * (beatDuration * 0.5f);

            // Eighth note pattern with rests
            bool play = eighthNote % 3 != 2; // play 2, rest 1 pattern
            if (!play) return 0f;

            float env = SynthVoice.ADSR(eighthTime, beatDuration * 0.4f, 0.01f, 0.05f, 0.7f, 0.05f);
            float phase = SynthVoice.GetPhase(time, freq);
            float saw = SynthVoice.Oscillator(phase, WaveformType.Sawtooth);

            // Low pass filter
            float cutoff = 0.1f + intensity * 0.15f;
            float filtered = SynthVoice.LowPassFilter(saw, ref bassFilterState, cutoff);

            return filtered * env * intensity;
        }

        private float GenerateArp(float time, float intensity)
        {
            float sixteenthDur = beatDuration * 0.25f;
            int sixteenthIndex = Mathf.FloorToInt(time / sixteenthDur);
            int noteIndex = sixteenthIndex % arpNotes.Length;
            float noteTime = time % sixteenthDur;

            float freq = MidiToFreq(arpNotes[noteIndex]);
            float env = SynthVoice.ADSR(noteTime, sixteenthDur * 0.8f, 0.005f, 0.03f, 0.5f, 0.02f);

            float phase = SynthVoice.GetPhase(time, freq);
            float osc = SynthVoice.Oscillator(phase, WaveformType.Square) * 0.5f +
                        SynthVoice.Oscillator(SynthVoice.GetPhase(time, freq * 1.005f), WaveformType.Sawtooth) * 0.5f;

            float cutoff = 0.15f + intensity * 0.25f;
            float filtered = SynthVoice.LowPassFilter(osc, ref leadFilterState, cutoff);

            return filtered * env * (intensity - 0.3f);
        }

        private float GeneratePad(float time, float intensity)
        {
            int barIndex = Mathf.FloorToInt(time / (barDuration * 2f));
            int chordIdx = barIndex % chordProgressions.Length;
            var chord = chordProgressions[chordIdx];

            float padSample = 0f;
            foreach (int midi in chord)
            {
                float freq = MidiToFreq(midi);
                float phase = SynthVoice.GetPhase(time, freq);
                padSample += SynthVoice.Oscillator(phase, WaveformType.Sawtooth) * 0.2f;

                // Detuned layer
                float phase2 = SynthVoice.GetPhase(time, freq * 1.003f);
                padSample += SynthVoice.Oscillator(phase2, WaveformType.Sawtooth) * 0.15f;
            }

            float cutoff = 0.05f + intensity * 0.08f;
            float filtered = SynthVoice.LowPassFilter(padSample, ref padFilterState, cutoff);

            return filtered * intensity * 0.5f;
        }

        private float MidiToFreq(int midi)
        {
            return 440f * Mathf.Pow(2f, (midi - 69) / 12f);
        }
    }
}
