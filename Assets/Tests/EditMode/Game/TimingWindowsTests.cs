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
            Assert.AreEqual(0.025f, TimingWindows.Perfect);
            Assert.AreEqual(0.050f, TimingWindows.Great);
            Assert.AreEqual(0.100f, TimingWindows.Good);
        }

        [Test]
        public void GetJudgment_Zero_ReturnsPerfect()
        {
            Assert.AreEqual(Judgment.Perfect, TimingWindows.GetJudgment(0f));
        }

        [Test]
        public void GetJudgment_WithinPerfect_ReturnsPerfect()
        {
            Assert.AreEqual(Judgment.Perfect, TimingWindows.GetJudgment(0.020f));
        }

        [Test]
        public void GetJudgment_ExactlyPerfect_ReturnsPerfect()
        {
            Assert.AreEqual(Judgment.Perfect, TimingWindows.GetJudgment(0.025f));
        }

        [Test]
        public void GetJudgment_BetweenPerfectAndGreat_ReturnsGreat()
        {
            Assert.AreEqual(Judgment.Great, TimingWindows.GetJudgment(0.035f));
        }

        [Test]
        public void GetJudgment_ExactlyGreat_ReturnsGreat()
        {
            Assert.AreEqual(Judgment.Great, TimingWindows.GetJudgment(0.050f));
        }

        [Test]
        public void GetJudgment_BetweenGreatAndGood_ReturnsGood()
        {
            Assert.AreEqual(Judgment.Good, TimingWindows.GetJudgment(0.075f));
        }

        [Test]
        public void GetJudgment_ExactlyGood_ReturnsGood()
        {
            Assert.AreEqual(Judgment.Good, TimingWindows.GetJudgment(0.100f));
        }

        [Test]
        public void GetJudgment_BeyondGood_ReturnsMiss()
        {
            Assert.AreEqual(Judgment.Miss, TimingWindows.GetJudgment(0.150f));
            Assert.AreEqual(Judgment.Miss, TimingWindows.GetJudgment(1.0f));
        }
    }
}
