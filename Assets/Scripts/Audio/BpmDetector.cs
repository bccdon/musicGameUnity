using UnityEngine;
using System.Collections.Generic;

namespace PulseHighway.Audio
{
    public static class BpmDetector
    {
        private const int HOP_SIZE = 512;
        private const int RMS_WINDOW = 1024;
        private const int MIN_BPM = 60;
        private const int MAX_BPM = 200;

        /// <summary>
        /// Detect BPM from mono audio samples using onset interval analysis.
        /// </summary>
        public static int DetectBpm(float[] monoSamples, int sampleRate)
        {
            if (monoSamples == null || monoSamples.Length < sampleRate)
                return 120; // Default fallback

            // Step 1: Calculate energy per frame
            int numFrames = monoSamples.Length / HOP_SIZE;
            float[] energies = new float[numFrames];

            for (int f = 0; f < numFrames; f++)
            {
                int start = f * HOP_SIZE;
                float sum = 0f;
                int count = 0;
                for (int j = start; j < start + RMS_WINDOW && j < monoSamples.Length; j++)
                {
                    sum += monoSamples[j] * monoSamples[j];
                    count++;
                }
                energies[f] = count > 0 ? Mathf.Sqrt(sum / count) : 0f;
            }

            // Step 2: Detect onsets (energy peaks)
            var onsetFrames = new List<int>();
            int windowSize = 6;

            for (int f = windowSize; f < numFrames - 1; f++)
            {
                float localAvg = 0f;
                for (int j = f - windowSize; j < f; j++)
                    localAvg += energies[j];
                localAvg /= windowSize;

                float threshold = localAvg * 1.4f;
                if (energies[f] > threshold && energies[f] > energies[f - 1] && energies[f] >= energies[f + 1])
                {
                    onsetFrames.Add(f);
                }
            }

            if (onsetFrames.Count < 4)
                return 120; // Not enough onsets

            // Step 3: Calculate intervals between onsets (in seconds)
            var intervals = new List<float>();
            for (int i = 1; i < onsetFrames.Count; i++)
            {
                float intervalSec = (onsetFrames[i] - onsetFrames[i - 1]) * HOP_SIZE / (float)sampleRate;
                if (intervalSec > 0.2f && intervalSec < 2f) // Between 30 and 300 BPM range
                    intervals.Add(intervalSec);
            }

            if (intervals.Count < 3)
                return 120;

            // Step 4: Build BPM histogram
            int[] histogram = new int[MAX_BPM - MIN_BPM + 1];

            foreach (float interval in intervals)
            {
                float bpm = 60f / interval;

                // Also check half and double time
                float[] candidates = { bpm, bpm * 2f, bpm / 2f };

                foreach (float candidate in candidates)
                {
                    int rounded = Mathf.RoundToInt(candidate);
                    if (rounded >= MIN_BPM && rounded <= MAX_BPM)
                    {
                        int idx = rounded - MIN_BPM;
                        // Weight nearby BPMs too (±2 tolerance)
                        for (int d = -2; d <= 2; d++)
                        {
                            int ni = idx + d;
                            if (ni >= 0 && ni < histogram.Length)
                                histogram[ni]++;
                        }
                    }
                }
            }

            // Step 5: Find peak
            int bestIdx = 0;
            int bestCount = 0;
            for (int i = 0; i < histogram.Length; i++)
            {
                if (histogram[i] > bestCount)
                {
                    bestCount = histogram[i];
                    bestIdx = i;
                }
            }

            int detectedBpm = bestIdx + MIN_BPM;

            // Sanity: prefer common tempos (bias toward 120 if unclear)
            if (bestCount < 5)
                return 120;

            return detectedBpm;
        }
    }
}
