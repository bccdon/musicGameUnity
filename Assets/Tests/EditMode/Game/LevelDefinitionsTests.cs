using NUnit.Framework;
using PulseHighway.Game;

namespace PulseHighway.Tests.EditMode.Game
{
    [TestFixture]
    public class LevelDefinitionsTests
    {
        [Test]
        public void AllLevels_Returns24Levels()
        {
            Assert.AreEqual(24, LevelDefinitions.AllLevels.Length);
        }

        [Test]
        public void AllLevels_IdsAreSequential()
        {
            var levels = LevelDefinitions.AllLevels;
            for (int i = 0; i < levels.Length; i++)
            {
                Assert.AreEqual(i + 1, levels[i].id, $"Level at index {i} has wrong id");
            }
        }

        [Test]
        public void GetLevel_ValidId_ReturnsCorrectLevel()
        {
            var level1 = LevelDefinitions.GetLevel(1);
            Assert.AreEqual("Ignition", level1.name);
            Assert.AreEqual("easy", level1.difficulty);

            var level24 = LevelDefinitions.GetLevel(24);
            Assert.AreEqual("God Mode", level24.name);
            Assert.AreEqual("expert", level24.difficulty);
        }

        [Test]
        public void GetLevel_InvalidId_ReturnsFallback()
        {
            var level0 = LevelDefinitions.GetLevel(0);
            Assert.AreEqual(1, level0.id);

            var level25 = LevelDefinitions.GetLevel(25);
            Assert.AreEqual(1, level25.id);
        }

        [Test]
        public void AllLevels_DifficultyProgression()
        {
            var levels = LevelDefinitions.AllLevels;
            for (int i = 0; i < 6; i++) Assert.AreEqual("easy", levels[i].difficulty, $"Level {i + 1}");
            for (int i = 6; i < 12; i++) Assert.AreEqual("medium", levels[i].difficulty, $"Level {i + 1}");
            for (int i = 12; i < 18; i++) Assert.AreEqual("hard", levels[i].difficulty, $"Level {i + 1}");
            for (int i = 18; i < 24; i++) Assert.AreEqual("expert", levels[i].difficulty, $"Level {i + 1}");
        }

        [Test]
        public void AllLevels_GenresAreValid()
        {
            string[] validGenres = { "synthwave", "dnb", "techno" };
            foreach (var level in LevelDefinitions.AllLevels)
            {
                CollectionAssert.Contains(validGenres, level.genre,
                    $"Level {level.id} has invalid genre: {level.genre}");
            }
        }

        [Test]
        public void AllLevels_HoldProbabilityInRange()
        {
            foreach (var level in LevelDefinitions.AllLevels)
            {
                Assert.GreaterOrEqual(level.holdProbability, 0f, $"Level {level.id}");
                Assert.LessOrEqual(level.holdProbability, 1f, $"Level {level.id}");
            }
        }

        [Test]
        public void AllLevels_UnlockScorePositive()
        {
            foreach (var level in LevelDefinitions.AllLevels)
            {
                Assert.Greater(level.unlockScore, 0, $"Level {level.id}");
            }
        }

        [Test]
        public void AllLevels_DurationPositive()
        {
            foreach (var level in LevelDefinitions.AllLevels)
            {
                Assert.Greater(level.duration, 0f, $"Level {level.id}");
                Assert.Greater(level.bpm, 0, $"Level {level.id}");
            }
        }

        [Test]
        public void AllLevels_HaveNamesAndDescriptions()
        {
            foreach (var level in LevelDefinitions.AllLevels)
            {
                Assert.IsFalse(string.IsNullOrEmpty(level.name), $"Level {level.id} missing name");
                Assert.IsFalse(string.IsNullOrEmpty(level.description), $"Level {level.id} missing description");
                Assert.IsFalse(string.IsNullOrEmpty(level.songTitle), $"Level {level.id} missing songTitle");
            }
        }
    }
}
