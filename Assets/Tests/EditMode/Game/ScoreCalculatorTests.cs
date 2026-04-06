using NUnit.Framework;
using PulseHighway.Core;
using PulseHighway.Game;

namespace PulseHighway.Tests.EditMode.Game
{
    [TestFixture]
    public class ScoreCalculatorTests
    {
        [Test]
        public void CalculateHitScore_Perfect_NoCombo_Returns100()
        {
            Assert.AreEqual(100, ScoreCalculator.CalculateHitScore(Judgment.Perfect, 0));
        }

        [Test]
        public void CalculateHitScore_Perfect_Combo10_Returns200()
        {
            // 100 * (1 + 10 * 0.1) = 100 * 2 = 200
            Assert.AreEqual(200, ScoreCalculator.CalculateHitScore(Judgment.Perfect, 10));
        }

        [Test]
        public void CalculateHitScore_Perfect_Combo5_Returns150()
        {
            // 100 * (1 + 5 * 0.1) = 100 * 1.5 = 150
            Assert.AreEqual(150, ScoreCalculator.CalculateHitScore(Judgment.Perfect, 5));
        }

        [Test]
        public void CalculateHitScore_Great_NoCombo_Returns75()
        {
            Assert.AreEqual(75, ScoreCalculator.CalculateHitScore(Judgment.Great, 0));
        }

        [Test]
        public void CalculateHitScore_Great_Combo10_Returns150()
        {
            // 75 * (1 + 10 * 0.1) = 75 * 2 = 150
            Assert.AreEqual(150, ScoreCalculator.CalculateHitScore(Judgment.Great, 10));
        }

        [Test]
        public void CalculateHitScore_Good_NoCombo_Returns50()
        {
            Assert.AreEqual(50, ScoreCalculator.CalculateHitScore(Judgment.Good, 0));
        }

        [Test]
        public void CalculateHitScore_Miss_ReturnsZero()
        {
            Assert.AreEqual(0, ScoreCalculator.CalculateHitScore(Judgment.Miss, 0));
            Assert.AreEqual(0, ScoreCalculator.CalculateHitScore(Judgment.Miss, 100));
        }

        [Test]
        public void CalculateHoldTick_NoCombo_Returns20()
        {
            Assert.AreEqual(20, ScoreCalculator.CalculateHoldTick(0));
        }

        [Test]
        public void CalculateHoldTick_Combo20_Returns40()
        {
            // 20 * (1 + 20 * 0.05) = 20 * 2 = 40
            Assert.AreEqual(40, ScoreCalculator.CalculateHoldTick(20));
        }

        [Test]
        public void CalculateRank_95Percent_ReturnsS()
        {
            Assert.AreEqual(Rank.S, ScoreCalculator.CalculateRank(0.95f));
            Assert.AreEqual(Rank.S, ScoreCalculator.CalculateRank(1.0f));
        }

        [Test]
        public void CalculateRank_90Percent_ReturnsA()
        {
            Assert.AreEqual(Rank.A, ScoreCalculator.CalculateRank(0.90f));
            Assert.AreEqual(Rank.A, ScoreCalculator.CalculateRank(0.94f));
        }

        [Test]
        public void CalculateRank_80Percent_ReturnsB()
        {
            Assert.AreEqual(Rank.B, ScoreCalculator.CalculateRank(0.80f));
            Assert.AreEqual(Rank.B, ScoreCalculator.CalculateRank(0.89f));
        }

        [Test]
        public void CalculateRank_70Percent_ReturnsC()
        {
            Assert.AreEqual(Rank.C, ScoreCalculator.CalculateRank(0.70f));
            Assert.AreEqual(Rank.C, ScoreCalculator.CalculateRank(0.79f));
        }

        [Test]
        public void CalculateRank_Below70_ReturnsF()
        {
            Assert.AreEqual(Rank.F, ScoreCalculator.CalculateRank(0.69f));
            Assert.AreEqual(Rank.F, ScoreCalculator.CalculateRank(0.0f));
        }
    }
}
