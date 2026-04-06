using NUnit.Framework;
using PulseHighway.Core;
using PulseHighway.Game;

namespace PulseHighway.Tests.EditMode.Game
{
    [TestFixture]
    public class TimingWindowsTests
    {
        [Test]
        public void Constants_AreCorrect()
        {
            Assert.AreEqual(0.045f, TimingWindows.Perfect);
            Assert.AreEqual(0.090f, TimingWindows.Great);
            Assert.AreEqual(0.150f, TimingWindows.Good);
        }

        [Test]
        public void GetJudgment_Zero_ReturnsPerfect()
        {
            Assert.AreEqual(Judgment.Perfect, TimingWindows.GetJudgment(0f));
        }

        [Test]
        public void GetJudgment_WithinPerfect_ReturnsPerfect()
        {
            Assert.AreEqual(Judgment.Perfect, TimingWindows.GetJudgment(0.030f));
        }

        [Test]
        public void GetJudgment_ExactlyPerfect_ReturnsPerfect()
        {
            Assert.AreEqual(Judgment.Perfect, TimingWindows.GetJudgment(0.045f));
        }

        [Test]
        public void GetJudgment_BetweenPerfectAndGreat_ReturnsGreat()
        {
            Assert.AreEqual(Judgment.Great, TimingWindows.GetJudgment(0.060f));
        }

        [Test]
        public void GetJudgment_ExactlyGreat_ReturnsGreat()
        {
            Assert.AreEqual(Judgment.Great, TimingWindows.GetJudgment(0.090f));
        }

        [Test]
        public void GetJudgment_BetweenGreatAndGood_ReturnsGood()
        {
            Assert.AreEqual(Judgment.Good, TimingWindows.GetJudgment(0.120f));
        }

        [Test]
        public void GetJudgment_ExactlyGood_ReturnsGood()
        {
            Assert.AreEqual(Judgment.Good, TimingWindows.GetJudgment(0.150f));
        }

        [Test]
        public void GetJudgment_BeyondGood_ReturnsMiss()
        {
            Assert.AreEqual(Judgment.Miss, TimingWindows.GetJudgment(0.200f));
            Assert.AreEqual(Judgment.Miss, TimingWindows.GetJudgment(1.0f));
        }
    }
}
