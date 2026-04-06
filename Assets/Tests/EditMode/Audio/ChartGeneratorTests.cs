using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using PulseHighway.Audio;
using PulseHighway.Game;

namespace PulseHighway.Tests.EditMode.Audio
{
    [TestFixture]
    public class ChartGeneratorTests
    {
        private ChartGenerator generator;
        private LevelConfig easyConfig;
        private AnalysisResult basicAnalysis;

        [SetUp]
        public void SetUp()
        {
            generator = new ChartGenerator();
            Random.InitState(42); // Deterministic

            easyConfig = new LevelConfig
            {
                id = 1, name = "Test", songTitle = "TestSong",
                difficulty = "easy", genre = "synthwave",
                bpm = 120, duration = 60f, scrollSpeed = 30f,
                noteDensity = 1f, holdProbability = 0f, unlockScore = 1000
            };

            // Create basic analysis with some onsets
            var onsets = new List<Onset>();
            for (float t = 2.5f; t < 58f; t += 0.5f)
            {
                onsets.Add(new Onset(t, 0.5f + Random.value * 0.5f));
            }

            var freqData = new List<FrequencyFrame>();
            for (float t = 0; t < 60f; t += 0.5f)
            {
                freqData.Add(new FrequencyFrame
                {
                    time = t,
                    subBass = Random.value,
                    bass = Random.value,
                    lowMid = Random.value,
                    mid = Random.value,
                    high = Random.value
                });
            }

            basicAnalysis = new AnalysisResult
            {
                bpm = 120,
                confidence = 85f,
                onsets = onsets,
                frequencyData = freqData,
                duration = 60f
            };
        }

        [Test]
        public void Generate_ProducesNonEmptyChart()
        {
            var chart = generator.Generate(basicAnalysis, easyConfig);
            Assert.IsNotNull(chart);
            Assert.Greater(chart.notes.Length, 0);
        }

        [Test]
        public void Generate_NotesWithinDuration()
        {
            var chart = generator.Generate(basicAnalysis, easyConfig);
            foreach (var note in chart.notes)
            {
                Assert.GreaterOrEqual(note.time, 1.5f, "Note should be at least 1.5s into song");
                Assert.LessOrEqual(note.time, easyConfig.duration - 1.5f, "Note should be before last 1.5s");
            }
        }

        [Test]
        public void Generate_NotesInValidLanes()
        {
            var chart = generator.Generate(basicAnalysis, easyConfig);
            foreach (var note in chart.notes)
            {
                Assert.GreaterOrEqual(note.lane, 0);
                Assert.LessOrEqual(note.lane, 4);
            }
        }

        [Test]
        public void Generate_MetadataPopulated()
        {
            var chart = generator.Generate(basicAnalysis, easyConfig);
            Assert.AreEqual("TestSong", chart.metadata.songTitle);
            Assert.AreEqual(120, chart.metadata.bpm);
            Assert.AreEqual(60f, chart.metadata.duration);
            Assert.Greater(chart.metadata.noteCount, 0);
        }

        [Test]
        public void Generate_NoHolds_WhenProbabilityZero()
        {
            easyConfig.holdProbability = 0f;
            Random.InitState(42);
            var chart = generator.Generate(basicAnalysis, easyConfig);
            foreach (var note in chart.notes)
            {
                Assert.AreEqual(NoteType.Tap, note.type, "All notes should be tap when holdProbability=0");
            }
        }

        [Test]
        public void Generate_HoldNotes_HavePositiveDuration()
        {
            easyConfig.holdProbability = 1f;
            easyConfig.difficulty = "medium";
            Random.InitState(42);
            var chart = generator.Generate(basicAnalysis, easyConfig);

            bool hasHold = false;
            foreach (var note in chart.notes)
            {
                if (note.type == NoteType.Hold)
                {
                    Assert.Greater(note.duration, 0f);
                    hasHold = true;
                }
            }
            // With 100% hold probability, should have at least some holds
            Assert.IsTrue(hasHold, "Expected at least one hold note with holdProbability=1");
        }

        [Test]
        public void Generate_NotesSortedByTime()
        {
            var chart = generator.Generate(basicAnalysis, easyConfig);
            for (int i = 1; i < chart.notes.Length; i++)
            {
                Assert.LessOrEqual(chart.notes[i - 1].time, chart.notes[i].time,
                    "Notes should be sorted by time");
            }
        }

        [Test]
        public void Generate_MeetsMinimumNoteCount()
        {
            var chart = generator.Generate(basicAnalysis, easyConfig);
            // Easy difficulty minNoteCount is 40
            Assert.GreaterOrEqual(chart.notes.Length, 40);
        }
    }
}
