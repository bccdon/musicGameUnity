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
        public Onset(float time, float energy) { this.time = time; this.energy = energy; }
    }

    public struct FrequencyFrame
    {
        public float time;
        public float subBass, bass, lowMid, mid, high;

        public int DominantLane()
        {
            float max = subBass; int lane = 0;
            if (bass > max) { max = bass; lane = 1; }
            if (lowMid > max) { max = lowMid; lane = 2; }
            if (mid > max) { max = mid; lane = 3; }
            if (high > max) { lane = 4; }
            return lane;
        }
    }

    public class AudioAnalyzer
    {
        private const int FFT_SIZE = 1024;
        private const int HOP_SIZE = 512;

        public AnalysisResult Analyze(float[] samples, int sampleRate, int expectedBpm)
        {
            float[] mono = ConvertToMono(samples, 2);
            float duration = (float)mono.Length / sampleRate;
            var onsets = DetectOnsetsSpectralFlux(mono, sampleRate);
            var freqData = AnalyzeFrequencies(mono, sampleRate);

            return new AnalysisResult
            {
                bpm = expectedBpm > 0 ? expectedBpm : 120,
                confidence = onsets.Count > 10 ? 90f : 60f,
                onsets = onsets,
                frequencyData = freqData,
                duration = duration
            };
        }

        private float[] ConvertToMono(float[] stereo, int channels)
        {
            if (channels == 1) return stereo;
            int len = stereo.Length / channels;
            float[] mono = new float[len];
            for (int i = 0; i < len; i++)
            {
                float sum = 0f;
                for (int ch = 0; ch < channels; ch++) sum += stereo[i * channels + ch];
                mono[i] = sum / channels;
            }
            return mono;
        }

        /// <summary>
        /// Spectral flux onset detection with Hann-windowed FFT and adaptive thresholding.
        /// </summary>
        private List<Onset> DetectOnsetsSpectralFlux(float[] mono, int sampleRate)
        {
            int numFrames = (mono.Length - FFT_SIZE) / HOP_SIZE;
            if (numFrames < 2) return new List<Onset>();

            int halfFFT = FFT_SIZE / 2;
            float[] prevMag = new float[halfFFT];
            float[] flux = new float[numFrames];
            float[] window = new float[FFT_SIZE];
            for (int i = 0; i < FFT_SIZE; i++)
                window[i] = 0.5f * (1f - Mathf.Cos(2f * Mathf.PI * i / (FFT_SIZE - 1)));

            // Step 1: Compute spectral flux per frame
            for (int frame = 0; frame < numFrames; frame++)
            {
                int offset = frame * HOP_SIZE;
                float[] mag = new float[halfFFT];

                // Simplified DFT on decimated bins (every 2nd for speed)
                for (int k = 0; k < halfFFT; k += 2)
                {
                    float omega = 2f * Mathf.PI * k / FFT_SIZE;
                    float re = 0f, im = 0f;
                    for (int n = 0; n < FFT_SIZE && offset + n < mono.Length; n++)
                    {
                        float w = mono[offset + n] * window[n];
                        re += w * Mathf.Cos(omega * n);
                        im += w * Mathf.Sin(omega * n);
                    }
                    float m = Mathf.Sqrt(re * re + im * im);
                    mag[k] = m;
                    if (k + 1 < halfFFT) mag[k + 1] = m; // Interpolate
                }

                // Spectral flux: sum of positive differences only
                float f = 0f;
                for (int b = 0; b < halfFFT; b++)
                {
                    float diff = mag[b] - prevMag[b];
                    if (diff > 0) f += diff;
                }
                flux[frame] = f;
                System.Array.Copy(mag, prevMag, halfFFT);
            }

            // Step 2: Adaptive threshold peak-picking
            var onsets = new List<Onset>();
            int tw = Mathf.Max(8, sampleRate / HOP_SIZE / 5); // ~200ms window

            for (int frame = tw; frame < numFrames - 1; frame++)
            {
                float avg = 0f;
                for (int j = frame - tw; j < frame; j++) avg += flux[j];
                avg /= tw;

                float threshold = avg * 1.4f + 0.005f;

                if (flux[frame] > threshold &&
                    flux[frame] > flux[frame - 1] &&
                    flux[frame] >= flux[frame + 1])
                {
                    float time = (float)(frame * HOP_SIZE) / sampleRate;
                    onsets.Add(new Onset(time, flux[frame]));
                }
            }

            // Step 3: Enforce 60ms minimum spacing
            var filtered = new List<Onset>();
            float last = -1f;
            foreach (var o in onsets)
            {
                if (o.time - last >= 0.06f) { filtered.Add(o); last = o.time; }
            }
            return filtered;
        }

        private List<FrequencyFrame> AnalyzeFrequencies(float[] mono, int sampleRate)
        {
            var frames = new List<FrequencyFrame>();
            int frameSize = 2048;
            int hop = 1024;

            for (int start = 0; start + frameSize < mono.Length; start += hop)
            {
                float time = (float)start / sampleRate;
                frames.Add(new FrequencyFrame
                {
                    time = time,
                    subBass = BandRMS(mono, start, frameSize, sampleRate, 20, 60),
                    bass = BandRMS(mono, start, frameSize, sampleRate, 60, 250),
                    lowMid = BandRMS(mono, start, frameSize, sampleRate, 250, 500),
                    mid = BandRMS(mono, start, frameSize, sampleRate, 500, 2000),
                    high = BandRMS(mono, start, frameSize, sampleRate, 2000, 8000)
                });
            }
            return frames;
        }

        private float BandRMS(float[] s, int start, int win, int sr, float fLo, float fHi)
        {
            float energy = 0f;
            int bins = 4;
            float step = (fHi - fLo) / bins;
            int count = Mathf.Min(win, s.Length - start);

            for (int b = 0; b < bins; b++)
            {
                float freq = fLo + b * step;
                float omega = 2f * Mathf.PI * freq / sr;
                float re = 0f, im = 0f;
                for (int n = 0; n < count; n++)
                {
                    re += s[start + n] * Mathf.Cos(omega * n);
                    im += s[start + n] * Mathf.Sin(omega * n);
                }
                energy += (re * re + im * im) / (count * count);
            }
            return Mathf.Sqrt(energy / bins);
        }
    }
}
