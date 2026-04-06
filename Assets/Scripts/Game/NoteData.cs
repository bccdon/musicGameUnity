namespace PulseHighway.Game
{
    public enum NoteType
    {
        Tap,
        Hold
    }

    [System.Serializable]
    public struct NoteData
    {
        public float time;     // seconds from song start
        public int lane;       // 0-4
        public NoteType type;
        public float duration; // for hold notes, in seconds

        public NoteData(float time, int lane, NoteType type = NoteType.Tap, float duration = 0f)
        {
            this.time = time;
            this.lane = lane;
            this.type = type;
            this.duration = duration;
        }
    }

    [System.Serializable]
    public class ChartData
    {
        public NoteData[] notes;
        public ChartMetadata metadata;

        public ChartData(NoteData[] notes, ChartMetadata metadata)
        {
            this.notes = notes;
            this.metadata = metadata;
        }
    }

    [System.Serializable]
    public class ChartMetadata
    {
        public string songTitle;
        public string artist;
        public float duration;
        public int bpm;
        public int noteCount;
        public float avgNotesPerSecond;
    }
}
