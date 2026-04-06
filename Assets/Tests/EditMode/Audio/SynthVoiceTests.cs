using NUnit.Framework;
using UnityEngine;
using PulseHighway.Audio;

namespace PulseHighway.Tests.EditMode.Audio
{
    [TestFixture]
    public class SynthVoiceTests
    {
        private const float Epsilon = 0.01f;

        // Oscillator tests
        [Test]
        public void Oscillator_Sine_AtZero_ReturnsZero()
        {
            Assert.AreEqual(0f, SynthVoice.Oscillator(0f, WaveformType.Sine), Epsilon);
        }

        [Test]
        public void Oscillator_Sine_AtQuarter_ReturnsOne()
        {
            Assert.AreEqual(1f, SynthVoice.Oscillator(0.25f, WaveformType.Sine), Epsilon);
        }

        [Test]
        public void Oscillator_Sine_AtHalf_ReturnsZero()
        {
            Assert.AreEqual(0f, SynthVoice.Oscillator(0.5f, WaveformType.Sine), Epsilon);
        }

        [Test]
        public void Oscillator_Sawtooth_AtZero_ReturnsNegOne()
        {
            Assert.AreEqual(-1f, SynthVoice.Oscillator(0f, WaveformType.Sawtooth), Epsilon);
        }

        [Test]
        public void Oscillator_Sawtooth_AtHalf_ReturnsZero()
        {
            Assert.AreEqual(0f, SynthVoice.Oscillator(0.5f, WaveformType.Sawtooth), Epsilon);
        }

        [Test]
        public void Oscillator_Square_FirstHalf_ReturnsOne()
        {
            Assert.AreEqual(1f, SynthVoice.Oscillator(0.25f, WaveformType.Square), Epsilon);
        }

        [Test]
        public void Oscillator_Square_SecondHalf_ReturnsNegOne()
        {
            Assert.AreEqual(-1f, SynthVoice.Oscillator(0.75f, WaveformType.Square), Epsilon);
        }

        [Test]
        public void Oscillator_Triangle_AtHalf_ReturnsNegOne()
        {
            // 4 * |0.5 - 0.5| - 1 = -1
            Assert.AreEqual(-1f, SynthVoice.Oscillator(0.5f, WaveformType.Triangle), Epsilon);
        }

        [Test]
        public void Oscillator_Noise_ReturnsInRange()
        {
            for (int i = 0; i < 100; i++)
            {
                float val = SynthVoice.Oscillator(0f, WaveformType.Noise);
                Assert.GreaterOrEqual(val, -1f);
                Assert.LessOrEqual(val, 1f);
            }
        }

        // Phase tests
        [Test]
        public void GetPhase_BasicCalculation()
        {
            // time=1, freq=1 => phase=0 (wraps to 0)
            float phase = SynthVoice.GetPhase(1f, 1f);
            Assert.AreEqual(0f, phase, Epsilon);
        }

        [Test]
        public void GetPhase_HalfCycle()
        {
            float phase = SynthVoice.GetPhase(0.5f, 1f);
            Assert.AreEqual(0.5f, phase, Epsilon);
        }

        // ADSR tests
        [Test]
        public void ADSR_BeforeNote_ReturnsZero()
        {
            Assert.AreEqual(0f, SynthVoice.ADSR(-0.1f, 1f, 0.1f, 0.1f, 0.5f, 0.1f));
        }

        [Test]
        public void ADSR_DuringAttack_Ramps()
        {
            // At halfway through attack (0.05 of 0.1), should be ~0.5
            float val = SynthVoice.ADSR(0.05f, 1f, 0.1f, 0.1f, 0.5f, 0.1f);
            Assert.AreEqual(0.5f, val, Epsilon);
        }

        [Test]
        public void ADSR_EndOfAttack_ReturnsOne()
        {
            float val = SynthVoice.ADSR(0.1f, 1f, 0.1f, 0.1f, 0.5f, 0.1f);
            Assert.AreEqual(1f, val, Epsilon);
        }

        [Test]
        public void ADSR_DuringSustain_HoldsSustainLevel()
        {
            // Past attack (0.1) and decay (0.1), in sustain region
            float val = SynthVoice.ADSR(0.5f, 1f, 0.1f, 0.1f, 0.5f, 0.1f);
            Assert.AreEqual(0.5f, val, Epsilon);
        }

        [Test]
        public void ADSR_AfterRelease_ReturnsZero()
        {
            // Note length 1.0, release 0.1. At time 1.2 (well past release)
            float val = SynthVoice.ADSR(1.2f, 1f, 0.1f, 0.1f, 0.5f, 0.1f);
            Assert.AreEqual(0f, val, Epsilon);
        }

        // Filter tests
        [Test]
        public void LowPassFilter_Smooths()
        {
            float prev = 0f;
            float out1 = SynthVoice.LowPassFilter(1f, ref prev, 0.5f);
            Assert.Greater(out1, 0f);
            Assert.Less(out1, 1f);
        }

        [Test]
        public void SoftClip_ClampsAboveOne()
        {
            Assert.AreEqual(1f, SynthVoice.SoftClip(2f));
        }

        [Test]
        public void SoftClip_ClampsBelowNegOne()
        {
            Assert.AreEqual(-1f, SynthVoice.SoftClip(-2f));
        }

        [Test]
        public void Distortion_IncreasesDrive()
        {
            float low = SynthVoice.Distortion(0.5f, 1f);
            float high = SynthVoice.Distortion(0.5f, 10f);
            // Higher drive should saturate more (closer to 1)
            Assert.Greater(high, low);
        }
    }
}
