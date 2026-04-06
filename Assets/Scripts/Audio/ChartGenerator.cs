using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PulseHighway.Game;

namespace PulseHighway.Audio
{
    public class ChartGenerator
    {
        // Difficulty settings matching original
        private static readonly Dictionary<string, DifficultySettings> DIFFICULTY_SETTINGS =
            new Dictionary<string, DifficultySettings>
        {
            { "easy", new DifficultySettings(0.25f, true, 1, 0.5f, 50, 150) },
            { "medium", new DifficultySettings(0.5f, true, 2, 1.0f, 150, 300) },
            { "hard", new DifficultySettings(0.75f, false, 4, 2.0f, 300, 500) },
            { "expert", new DifficultySettings(0.95f, false, 0, 3.0f, 500, 10000) }
        };

        private const float MIN_NOTE_SPACING = 0.2f;

        public ChartData Generate(AnalysisResult analysis, LevelConfig config)
        {
            var settings = DIFFICULTY_SETTINGS.ContainsKey(config.difficulty)
                ? DIFFICULTY_SETTINGS[config.difficulty]
                : DIFFICULTY_SETTINGS["medium"];

            // Filter onsets by energy percentile
            var filteredOnsets = FilterOnsets(analysis.onsets, settings.onsetPercentile);

            // Generate notes from onsets
            var notes = new List<NoteData>();
            float lastNoteTimePerLane = -1f;
            float[] laneLastTime = new float[5];
            for (int i = 0; i < 5; i++) laneLastTime[i] = -1f;

            foreach (var onset in filteredOnsets)
            {
                if (onset.time < 2f || onset.time > config.duration - 2f) continue;

                // Determine lane from frequency data
                int lane = GetLaneForTime(onset.time, analysis.frequencyData);

                // Enforce minimum spacing
                if (onset.time - laneLastTime[lane] < MIN_NOTE_SPACING) continue;

                // Quantize if needed
                float noteTime = onset.time;
                if (settings.quantizeToGrid)
                {
                    noteTime = QuantizeToGrid(noteTime, config.bpm, settings.subdivision);
                }

                // Check for hold notes
                NoteType type = NoteType.Tap;
                float duration = 0f;
                if (config.holdProbability > 0 && Random.value < config.holdProbability)
                {
                    type = NoteType.Hold;
                    float beatDur = 60f / config.bpm;
                    duration = beatDur * (0.5f + Random.value * 1.5f); // 0.5 to 2 beats
                }

                notes.Add(new NoteData(noteTime, lane, type, duration));
                laneLastTime[lane] = noteTime + (type == NoteType.Hold ? duration : 0f);
            }

            // Apply density scaling
            int maxNotes = Mathf.RoundToInt(settings.maxNoteCount * config.noteDensity);
            maxNotes = Mathf.Max(settings.minNoteCount, maxNotes);

            if (notes.Count > maxNotes)
            {
                // Keep evenly distributed subset
                var sorted = notes.OrderBy(n => n.time).ToList();
                float step = (float)sorted.Count / maxNotes;
                var trimmed = new List<NoteData>();
                for (float i = 0; i < sorted.Count && trimmed.Count < maxNotes; i += step)
                {
                    trimmed.Add(sorted[Mathf.FloorToInt(i)]);
                }
                notes = trimmed;
            }

            // Ensure minimum note count with beat-grid fill
            if (notes.Count < settings.minNoteCount)
            {
                FillWithBeatGrid(notes, config, settings);
            }

            // Sort by time
            notes.Sort((a, b) => a.time.CompareTo(b.time));

            var metadata = new ChartMetadata
            {
                songTitle = config.songTitle,
                artist = "Pulse Highway AI",
                duration = config.duration,
                bpm = config.bpm,
                noteCount = notes.Count,
                avgNotesPerSecond = notes.Count / config.duration
            };

            return new ChartData(notes.ToArray(), metadata);
        }

        private List<Onset> FilterOnsets(List<Onset> onsets, float percentile)
        {
            if (onsets.Count == 0) return new List<Onset>();

            var sorted = onsets.OrderBy(o => o.energy).ToList();
            float threshold = sorted[Mathf.FloorToInt(sorted.Count * (1f - percentile))].energy;

            return onsets.Where(o => o.energy >= threshold).ToList();
        }

        private int GetLaneForTime(float time, List<FrequencyFrame> freqData)
        {
            if (freqData.Count == 0) return Random.Range(0, 5);

            // Find closest frequency frame
            int bestIdx = 0;
            float bestDist = float.MaxValue;
            for (int i = 0; i < freqData.Count; i++)
            {
                float dist = Mathf.Abs(freqData[i].time - time);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestIdx = i;
                }
            }

            return freqData[bestIdx].DominantLane();
        }

        private float QuantizeToGrid(float time, int bpm, int subdivision)
        {
            float beatDur = 60f / bpm;
            float gridSize = subdivision > 0 ? beatDur / subdivision : beatDur;

            return Mathf.Round(time / gridSize) * gridSize;
        }

        private void FillWithBeatGrid(List<NoteData> notes, LevelConfig config, DifficultySettings settings)
        {
            float beatDur = 60f / config.bpm;
            float gridSize = settings.subdivision > 0 ? beatDur / settings.subdivision : beatDur;

            HashSet<float> existingTimes = new HashSet<float>();
            foreach (var n in notes) existingTimes.Add(Mathf.Round(n.time * 100f) / 100f);

            float time = 2f;
            while (time < config.duration - 2f && notes.Count < settings.minNoteCount)
            {
                float roundedTime = Mathf.Round(time * 100f) / 100f;
                if (!existingTimes.Contains(roundedTime))
                {
                    int lane = Random.Range(0, 5);
                    NoteType type = NoteType.Tap;
                    float dur = 0f;

                    if (config.holdProbability > 0 && Random.value < config.holdProbability * 0.5f)
                    {
                        type = NoteType.Hold;
                        dur = beatDur * (0.5f + Random.value);
                    }

                    notes.Add(new NoteData(time, lane, type, dur));
                    existingTimes.Add(roundedTime);
                }
                time += gridSize;
            }
        }
    }

    public struct DifficultySettings
    {
        public float onsetPercentile;
        public bool quantizeToGrid;
        public int subdivision;
        public float targetNotesPerSecond;
        public int minNoteCount;
        public int maxNoteCount;

        public DifficultySettings(float onsetPercentile, bool quantizeToGrid, int subdivision,
            float targetNotesPerSecond, int minNoteCount, int maxNoteCount)
        {
            this.onsetPercentile = onsetPercentile;
            this.quantizeToGrid = quantizeToGrid;
            this.subdivision = subdivision;
            this.targetNotesPerSecond = targetNotesPerSecond;
            this.minNoteCount = minNoteCount;
            this.maxNoteCount = maxNoteCount;
        }
    }
}
