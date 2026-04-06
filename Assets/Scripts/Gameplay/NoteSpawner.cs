using UnityEngine;
using System.Collections.Generic;
using PulseHighway.Core;
using PulseHighway.Game;

namespace PulseHighway.Gameplay
{
    public class NoteSpawner : MonoBehaviour
    {
        private ChartData chart;
        private List<NoteObject> tapPool = new List<NoteObject>();
        private List<HoldNoteObject> holdPool = new List<HoldNoteObject>();
        private Dictionary<int, NoteObject> activeNotes = new Dictionary<int, NoteObject>();

        private const int TAP_POOL_SIZE = 60;
        private const int HOLD_POOL_SIZE = 25;

        private float totalWidth;
        private float startX;

        public void Initialize(ChartData chart)
        {
            this.chart = chart;
            totalWidth = GameManager.LANE_COUNT * GameManager.LANE_WIDTH;
            startX = -totalWidth * 0.5f + GameManager.LANE_WIDTH * 0.5f;

            // Pre-allocate tap note pool
            for (int i = 0; i < TAP_POOL_SIZE; i++)
            {
                var go = new GameObject($"TapNote_{i}");
                go.transform.SetParent(transform);
                var note = go.AddComponent<NoteObject>();
                note.Setup(0); // Will be reconfigured on activation
                tapPool.Add(note);
            }

            // Pre-allocate hold note pool
            for (int i = 0; i < HOLD_POOL_SIZE; i++)
            {
                var go = new GameObject($"HoldNote_{i}");
                go.transform.SetParent(transform);
                var note = go.AddComponent<HoldNoteObject>();
                note.SetupHold(0, 1f); // Will be reconfigured
                holdPool.Add(note);
            }
        }

        public void UpdateNotes(float currentTime)
        {
            if (chart == null) return;

            for (int i = 0; i < chart.notes.Length; i++)
            {
                var noteData = chart.notes[i];
                float timeDiff = noteData.time - currentTime;

                // Within visible range?
                if (timeDiff > 0 && timeDiff <= GameManager.LOOK_AHEAD)
                {
                    if (!activeNotes.ContainsKey(i))
                    {
                        SpawnNote(i, noteData);
                    }

                    // Update position
                    if (activeNotes.TryGetValue(i, out var noteObj))
                    {
                        float z = GameManager.HIT_LINE_Z - timeDiff * GameManager.NOTE_SPEED;
                        float x = startX + noteData.lane * GameManager.LANE_WIDTH;
                        noteObj.transform.position = new Vector3(x, 0.5f, z);
                    }
                }
                // Past the miss window or too far behind
                else if (timeDiff < -0.5f)
                {
                    if (activeNotes.TryGetValue(i, out var noteObj))
                    {
                        noteObj.Deactivate();
                        activeNotes.Remove(i);
                    }
                }
            }
        }

        public void HideNote(int noteIndex)
        {
            if (activeNotes.TryGetValue(noteIndex, out var noteObj))
            {
                noteObj.Deactivate();
                activeNotes.Remove(noteIndex);
            }
        }

        public void StartHold(int noteIndex)
        {
            if (activeNotes.TryGetValue(noteIndex, out var noteObj))
            {
                if (noteObj is HoldNoteObject holdNote)
                {
                    holdNote.StartHolding();
                }
            }
        }

        public void EndHold(int noteIndex)
        {
            if (activeNotes.TryGetValue(noteIndex, out var noteObj))
            {
                if (noteObj is HoldNoteObject holdNote)
                {
                    holdNote.StopHolding();
                }
                noteObj.Deactivate();
                activeNotes.Remove(noteIndex);
            }
        }

        private void SpawnNote(int index, NoteData noteData)
        {
            NoteObject noteObj = null;

            if (noteData.type == NoteType.Hold)
            {
                // Find available hold note from pool
                foreach (var hold in holdPool)
                {
                    if (!hold.IsActive)
                    {
                        noteObj = hold;
                        // Reconfigure for this lane/duration
                        hold.SetupHold(noteData.lane, noteData.duration);
                        break;
                    }
                }
            }
            else
            {
                // Find available tap note from pool
                foreach (var tap in tapPool)
                {
                    if (!tap.IsActive)
                    {
                        noteObj = tap;
                        tap.Setup(noteData.lane);
                        break;
                    }
                }
            }

            if (noteObj != null)
            {
                noteObj.Activate(index);
                activeNotes[index] = noteObj;
            }
        }
    }
}
