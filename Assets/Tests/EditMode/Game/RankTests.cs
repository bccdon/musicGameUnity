using NUnit.Framework;
using PulseHighway.Game;

namespace PulseHighway.Tests.EditMode.Game
{
    [TestFixture]
    public class RankTests
    {
        [Test]
        public void EnumOrder_IsCorrect()
        {
            // IsHigherThan relies on enum integer ordering
            Assert.Less((int)Rank.None, (int)Rank.F);
            Assert.Less((int)Rank.F, (int)Rank.C);
            Assert.Less((int)Rank.C, (int)Rank.B);
            Assert.Less((int)Rank.B, (int)Rank.A);
            Assert.Less((int)Rank.A, (int)Rank.S);
        }

        [Test]
        public void IsHigherThan_SHigherThanA()
        {
            Assert.IsTrue(Rank.S.IsHigherThan(Rank.A));
        }

        [Test]
        public void IsHigherThan_FNotHigherThanC()
        {
            Assert.IsFalse(Rank.F.IsHigherThan(Rank.C));
        }

        [Test]
        public void IsHigherThan_SameRank_ReturnsFalse()
        {
            Assert.IsFalse(Rank.A.IsHigherThan(Rank.A));
        }

        [Test]
        public void ToLetter_AllRanks()
        {
            Assert.AreEqual("S", Rank.S.ToLetter());
            Assert.AreEqual("A", Rank.A.ToLetter());
            Assert.AreEqual("B", Rank.B.ToLetter());
            Assert.AreEqual("C", Rank.C.ToLetter());
            Assert.AreEqual("F", Rank.F.ToLetter());
            Assert.AreEqual("-", Rank.None.ToLetter());
        }
    }
}
