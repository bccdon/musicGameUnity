namespace PulseHighway.Game
{
    [System.Serializable]
    public struct LevelConfig
    {
        public int id;
        public string name;
        public string description;
        public string songTitle;
        public string difficulty; // "easy", "medium", "hard", "expert"
        public string genre;      // "synthwave", "dnb", "techno"
        public int bpm;
        public float duration;
        public float scrollSpeed;
        public float noteDensity;
        public float holdProbability;
        public int unlockScore;

        // External audio support
        public bool useExternalAudio;
        public string audioUrl;

        public string PhaseName
        {
            get
            {
                if (id <= 6) return "INITIATION";
                if (id <= 12) return "ACCELERATION";
                if (id <= 18) return "TRANSCENDENCE";
                return "MASTERY";
            }
        }

        public int PhaseNumber
        {
            get
            {
                if (id <= 6) return 1;
                if (id <= 12) return 2;
                if (id <= 18) return 3;
                return 4;
            }
        }
    }
}
