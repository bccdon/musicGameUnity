using NUnit.Framework;
using UnityEngine;
using PulseHighway.Audio;

namespace PulseHighway.Tests.EditMode.Audio
{
    [TestFixture]
    public class AudioAnalyzerTests
    {
        private AudioAnalyzer analyzer;

        [SetUp]
        public void SetUp()
        {
            analyzer = new AudioAnalyzer();
        }

        [Test]
        public void Analyze_SilentAudio_ReturnsResult()
        {
            // Stereo silence, 1 second at 44100Hz
            float[] silence = new float[44100 * 2];
            var result = analyzer.Analyze(silence, 44100, 120);

            Assert.IsNotNull(result);
            Assert.AreEqual(120, result.bpm);
            Assert.IsNotNull(result.onsets);
            Assert.IsNotNull(result.frequencyData);
        }

        [Test]
        public void Analyze_ReturnsDuration()
        {
            float[] samples = new float[44100 * 2]; // 1 second stereo
            var result = analyzer.Analyze(samples, 44100, 120);

            Assert.AreEqual(1f, result.duration, 0.01f);
        }

        [Test]
        public void Analyze_PassesThroughBpm()
        {
            float[] samples = new float[44100 * 2];
            var result = analyzer.Analyze(samples, 44100, 140);
            Assert.AreEqual(140, result.bpm);
        }

        [Test]
        public void Analyze_LoudImpulse_DetectsOnset()
        {
            // 2 seconds stereo, silence then a loud impulse at 0.5s
            int sampleRate = 44100;
            float[] samples = new float[sampleRate * 2 * 2];

            // Create impulse at 0.5s
            int impulseStart = (int)(0.5f * sampleRate);
            for (int i = impulseStart; i < impulseStart + 2048 && i * 2 + 1 < samples.Length; i++)
            {
                samples[i * 2] = 0.9f;     // left
                samples[i * 2 + 1] = 0.9f; // right
            }

            var result = analyzer.Analyze(samples, sampleRate, 120);
            Assert.Greater(result.onsets.Count, 0, "Should detect at least one onset from impulse");
        }

        [Test]
        public void FrequencyFrame_DominantLane_SubBassHighest_Returns0()
        {
            var frame = new FrequencyFrame
            {
                subBass = 1f, bass = 0.1f, lowMid = 0.1f, mid = 0.1f, high = 0.1f
            };
            Assert.AreEqual(0, frame.DominantLane());
        }

        [Test]
        public void FrequencyFrame_DominantLane_HighHighest_Returns4()
        {
            var frame = new FrequencyFrame
            {
                subBass = 0.1f, bass = 0.1f, lowMid = 0.1f, mid = 0.1f, high = 1f
            };
            Assert.AreEqual(4, frame.DominantLane());
        }
    }
}
