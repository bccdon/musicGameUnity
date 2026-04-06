using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PulseHighway.Game;

namespace PulseHighway.Audio
{
    public class ChartGenerator
    {
        private const float MIN_NOTE_SPACING = 0.15f;

        private static readonly Dictionary<string, DifficultySettings> SETTINGS =
            new Dictionary<string, DifficultySettings>
        {
            { "easy",   new DifficultySettings(0.3f,  true,  1, 0.5f, 40, 120) },
            { "medium", new DifficultySettings(0.55f, true,  2, 1.0f, 100, 250) },
            { "hard",   new DifficultySettings(0.75f, false, 4, 2.0f, 200, 450) },
            { "expert", new DifficultySettings(0.90f, false, 0, 3.0f, 350, 8000) }
        };

        private System.Random rng;

        public ChartData Generate(AnalysisResult analysis, LevelConfig config)
        {
            rng = new System.Random(config.id + config.bpm); // Deterministic per level

            var settings = SETTINGS.ContainsKey(config.difficulty)
                ? SETTINGS[config.difficulty] : SETTINGS["medium"];

            // Step 1: Weight onsets by energy (not hard cutoff)
            var weightedOnsets = WeightOnsets(analysis.onsets);

            // Step 2: Select notes using energy threshold
            var notes = new List<NoteData>();
            float[] laneLastTime = new float[5];
            for (int i = 0; i < 5; i++) laneLastTime[i] = -1f;
            int lastLane = 2; // Start center

            foreach (var onset in weightedOnsets)
            {
                if (onset.time < 1.5f || onset.time > config.duration - 1.5f) continue;

                // Skip weak onsets based on difficulty (keep more for harder)
                if (onset.energy < GetEnergyThreshold(weightedOnsets, settings.onsetPercentile))
                    continue;

                // Assign lane with smoothing (avoid big jumps)
                int lane = GetSmoothedLane(onset.time, analysis.frequencyData, lastLane);

                if (onset.time - laneLastTime[lane] < MIN_NOTE_SPACING) continue;

                // Gentle quantization: snap only if within 30ms of grid
                float noteTime = onset.time;
                if (settings.quantizeToGrid)
                    noteTime = GentleQuantize(noteTime, config.bpm, settings.subdivision, 0.03f);

                // Hold notes
                NoteType type = NoteType.Tap;
                float duration = 0f;
                if (config.holdProbability > 0 && (float)rng.NextDouble() < config.holdProbability)
                {
                    type = NoteType.Hold;
                    float beatDur = 60f / config.bpm;
                    duration = beatDur * (0.5f + (float)rng.NextDouble() * 1.5f);
                }

                notes.Add(new NoteData(noteTime, lane, type, duration));
                laneLastTime[lane] = noteTime + (type == NoteType.Hold ? duration : 0f);
                lastLane = lane;
            }

            // Step 3: Thin or fill to target density
            int maxNotes = Mathf.RoundToInt(settings.maxNoteCount * config.noteDensity);
            maxNotes = Mathf.Max(settings.minNoteCount, maxNotes);

            if (notes.Count > maxNotes)
            {
                // Thin by removing every Nth note (preserves rhythm feel)
                notes = ThinNotes(notes, maxNotes);
            }

            if (notes.Count < settings.minNoteCount)
            {
                FillWithBeatGrid(notes, config, settings);
            }

            notes.Sort((a, b) => a.time.CompareTo(b.time));

            var metadata = new ChartMetadata
            {
                songTitle = config.songTitle,
                artist = "Pulse Highway AI",
                duration = config.duration,
                bpm = config.bpm,
                noteCount = notes.Count,
                avgNotesPerSecond = notes.Count / Mathf.Max(config.duration, 1f)
            };

            return new ChartData(notes.ToArray(), metadata);
        }

        private List<Onset> WeightOnsets(List<Onset> onsets)
        {
            if (onsets.Count == 0) return onsets;

            // Normalize energy to 0-1 range
            float maxEnergy = onsets.Max(o => o.energy);
            if (maxEnergy < 0.001f) return onsets;

            return onsets.Select(o => new Onset(o.time, o.energy / maxEnergy)).ToList();
        }

        private float GetEnergyThreshold(List<Onset> onsets, float percentile)
        {
            if (onsets.Count == 0) return 0f;
            var sorted = onsets.OrderBy(o => o.energy).ToList();
            int idx = Mathf.FloorToInt(sorted.Count * (1f - percentile));
            idx = Mathf.Clamp(idx, 0, sorted.Count - 1);
            return sorted[idx].energy;
        }

        /// <summary>
        /// Assign lane based on frequency content, but smooth transitions to avoid wild jumps.
        /// </summary>
        private int GetSmoothedLane(float time, List<FrequencyFrame> freqData, int lastLane)
        {
            if (freqData.Count == 0) return rng.Next(0, 5);

            // Find closest frequency frame
            int bestIdx = 0;
            float bestDist = float.MaxValue;
            for (int i = 0; i < freqData.Count; i++)
            {
                float dist = Mathf.Abs(freqData[i].time - time);
                if (dist < bestDist) { bestDist = dist; bestIdx = i; }
            }

            int freqLane = freqData[bestIdx].DominantLane();

            // Smooth: don't jump more than 2 lanes at a time
            int diff = freqLane - lastLane;
            if (Mathf.Abs(diff) > 2)
                freqLane = lastLane + Mathf.Clamp(diff, -2, 2);

            return Mathf.Clamp(freqLane, 0, 4);
        }

        /// <summary>
        /// Snap to grid only if within tolerance (preserves organic timing).
        /// </summary>
        private float GentleQuantize(float time, int bpm, int subdivision, float tolerance)
        {
            float beatDur = 60f / bpm;
            float gridSize = subdivision > 0 ? beatDur / subdivision : beatDur;
            float nearest = Mathf.Round(time / gridSize) * gridSize;

            if (Mathf.Abs(time - nearest) <= tolerance)
                return nearest;
            return time; // Keep original if too far from grid
        }

        private List<NoteData> ThinNotes(List<NoteData> notes, int targetCount)
        {
            if (notes.Count <= targetCount) return notes;

            // Keep every Nth note to reach target
            var sorted = notes.OrderBy(n => n.time).ToList();
            float step = (float)sorted.Count / targetCount;
            var result = new List<NoteData>();
            for (float i = 0; i < sorted.Count && result.Count < targetCount; i += step)
                result.Add(sorted[Mathf.FloorToInt(i)]);
            return result;
        }

        private void FillWithBeatGrid(List<NoteData> notes, LevelConfig config, DifficultySettings settings)
        {
            float beatDur = 60f / config.bpm;
            float gridSize = settings.subdivision > 0 ? beatDur / settings.subdivision : beatDur;

            var existingTimes = new HashSet<int>();
            foreach (var n in notes) existingTimes.Add(Mathf.RoundToInt(n.time * 100));

            float time = 2f;
            int lane = 2;
            while (time < config.duration - 2f && notes.Count < settings.minNoteCount)
            {
                int key = Mathf.RoundToInt(time * 100);
                if (!existingTimes.Contains(key))
                {
                    // Smooth lane movement
                    lane = Mathf.Clamp(lane + rng.Next(-1, 2), 0, 4);

                    NoteType type = NoteType.Tap;
                    float dur = 0f;
                    if (config.holdProbability > 0 && (float)rng.NextDouble() < config.holdProbability * 0.5f)
                    {
                        type = NoteType.Hold;
                        dur = beatDur * (0.5f + (float)rng.NextDouble());
                    }

                    notes.Add(new NoteData(time, lane, type, dur));
                    existingTimes.Add(key);
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

        public DifficultySettings(float p, bool q, int s, float t, int min, int max)
        {
            onsetPercentile = p; quantizeToGrid = q; subdivision = s;
            targetNotesPerSecond = t; minNoteCount = min; maxNoteCount = max;
        }
    }
}
