using UnityEngine;
using System.Collections.Generic;

namespace PulseHighway.Audio
{
    public class AnalysisResult
    {
        public int bpm;
        public float confidence;
        public List<Onset> onsets;
        public List<FrequencyFrame> frequencyData;
        public float duration;
    }

    public struct Onset
    {
        public float time;
        public float energy;

        public Onset(float time, float energy)
        {
            this.time = time;
            this.energy = energy;
        }
    }

    public struct FrequencyFrame
    {
        public float time;
        public float subBass;   // 20-60 Hz
        public float bass;      // 60-250 Hz
        public float lowMid;    // 250-500 Hz
        public float mid;       // 500-2000 Hz
        public float high;      // 2000+ Hz

        public int DominantLane()
        {
            float max = subBass;
            int lane = 0;

            if (bass > max) { max = bass; lane = 1; }
            if (lowMid > max) { max = lowMid; lane = 2; }
            if (mid > max) { max = mid; lane = 3; }
            if (high > max) { lane = 4; }

            return lane;
        }
    }

    public class AudioAnalyzer
    {
        private const int HOP_SIZE = 512;
        private const int RMS_WINDOW = 2048;

        public AnalysisResult Analyze(float[] samples, int sampleRate, int expectedBpm)
        {
            // Convert to mono if stereo
            float[] mono = ConvertToMono(samples, 2);
            float duration = (float)mono.Length / sampleRate;

            // Detect onsets
            var onsets = DetectOnsets(mono, sampleRate);

            // Analyze frequencies
            var freqData = AnalyzeFrequencies(mono, sampleRate);

            return new AnalysisResult
            {
                bpm = expectedBpm,
                confidence = 85f,
                onsets = onsets,
                frequencyData = freqData,
                duration = duration
            };
        }

        private float[] ConvertToMono(float[] stereo, int channels)
        {
            if (channels == 1) return stereo;

            int monoLength = stereo.Length / channels;
            float[] mono = new float[monoLength];
            for (int i = 0; i < monoLength; i++)
            {
                float sum = 0f;
                for (int ch = 0; ch < channels; ch++)
                    sum += stereo[i * channels + ch];
                mono[i] = sum / channels;
            }
            return mono;
        }

        private List<Onset> DetectOnsets(float[] mono, int sampleRate)
        {
            var onsets = new List<Onset>();
            int numFrames = mono.Length / HOP_SIZE;
            float[] energies = new float[numFrames];

            // Calculate RMS energy per frame
            for (int frame = 0; frame < numFrames; frame++)
            {
                int start = frame * HOP_SIZE;
                float sum = 0f;
                int count = 0;
                for (int j = start; j < start + RMS_WINDOW && j < mono.Length; j++)
                {
                    sum += mono[j] * mono[j];
                    count++;
                }
                energies[frame] = count > 0 ? Mathf.Sqrt(sum / count) : 0f;
            }

            // Detect peaks with dynamic threshold
            int windowSize = 8;
            float sensitivity = 0.5f;

            for (int frame = windowSize; frame < numFrames - 1; frame++)
            {
                // Local average
                float localAvg = 0f;
                for (int j = frame - windowSize; j < frame; j++)
                    localAvg += energies[j];
                localAvg /= windowSize;

                // Peak detection
                float threshold = localAvg * (1f + sensitivity);
                if (energies[frame] > threshold && energies[frame] > energies[frame - 1] && energies[frame] >= energies[frame + 1])
                {
                    float time = (float)(frame * HOP_SIZE) / sampleRate;
                    onsets.Add(new Onset(time, energies[frame]));
                }
            }

            // Enforce minimum spacing
            var filtered = new List<Onset>();
            float lastOnsetTime = -1f;
            foreach (var onset in onsets)
            {
                if (onset.time - lastOnsetTime >= 0.05f)
                {
                    filtered.Add(onset);
                    lastOnsetTime = onset.time;
                }
            }

            return filtered;
        }

        private List<FrequencyFrame> AnalyzeFrequencies(float[] mono, int sampleRate)
        {
            var frames = new List<FrequencyFrame>();
            int frameSize = 4096;
            int hopSize = 2048;

            for (int start = 0; start + frameSize < mono.Length; start += hopSize)
            {
                float time = (float)start / sampleRate;

                // Simple band energy estimation using Goertzel-like approach
                float subBass = GetBandEnergy(mono, start, frameSize, sampleRate, 20, 60);
                float bass = GetBandEnergy(mono, start, frameSize, sampleRate, 60, 250);
                float lowMid = GetBandEnergy(mono, start, frameSize, sampleRate, 250, 500);
                float mid = GetBandEnergy(mono, start, frameSize, sampleRate, 500, 2000);
                float high = GetBandEnergy(mono, start, frameSize, sampleRate, 2000, 8000);

                frames.Add(new FrequencyFrame
                {
                    time = time,
                    subBass = subBass,
                    bass = bass,
                    lowMid = lowMid,
                    mid = mid,
                    high = high
                });
            }

            return frames;
        }

        private float GetBandEnergy(float[] samples, int start, int windowSize, int sampleRate,
            float freqLow, float freqHigh)
        {
            // Simplified DFT for specific frequency bands
            float energy = 0f;
            int numBins = 8; // Check several frequencies within the band
            float freqStep = (freqHigh - freqLow) / numBins;

            for (int bin = 0; bin < numBins; bin++)
            {
                float freq = freqLow + bin * freqStep;
                float omega = 2f * Mathf.PI * freq / sampleRate;

                float realSum = 0f, imagSum = 0f;
                int count = Mathf.Min(windowSize, samples.Length - start);

                for (int n = 0; n < count; n++)
                {
                    float sample = samples[start + n];
                    realSum += sample * Mathf.Cos(omega * n);
                    imagSum += sample * Mathf.Sin(omega * n);
                }

                energy += (realSum * realSum + imagSum * imagSum) / (count * count);
            }

            return Mathf.Sqrt(energy / numBins);
        }
    }
}
