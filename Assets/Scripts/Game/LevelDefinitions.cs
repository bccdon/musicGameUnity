namespace PulseHighway.Game
{
    public static class LevelDefinitions
    {
        private static LevelConfig[] levels;

        public static LevelConfig[] AllLevels
        {
            get
            {
                if (levels == null) Initialize();
                return levels;
            }
        }

        public static LevelConfig GetLevel(int id)
        {
            if (levels == null) Initialize();
            if (id < 1 || id > levels.Length) return levels[0];
            return levels[id - 1];
        }

        private static void Initialize()
        {
            levels = new LevelConfig[]
            {
                // Phase 1: Initiation (Easy)
                new LevelConfig {
                    id = 1, name = "Ignition", description = "Your journey begins. Feel the pulse.",
                    songTitle = "First Light", difficulty = "easy", genre = "synthwave",
                    bpm = 110, duration = 60f, scrollSpeed = 30f, noteDensity = 0.6f,
                    holdProbability = 0f, unlockScore = 1000
                },
                new LevelConfig {
                    id = 2, name = "Neon Dawn", description = "The city awakens with light.",
                    songTitle = "Neon Dawn", difficulty = "easy", genre = "synthwave",
                    bpm = 115, duration = 70f, scrollSpeed = 32f, noteDensity = 0.7f,
                    holdProbability = 0.05f, unlockScore = 1500
                },
                new LevelConfig {
                    id = 3, name = "Grid Runner", description = "Navigate the digital grid.",
                    songTitle = "Grid Runner", difficulty = "easy", genre = "synthwave",
                    bpm = 118, duration = 75f, scrollSpeed = 34f, noteDensity = 0.75f,
                    holdProbability = 0.08f, unlockScore = 2000
                },
                new LevelConfig {
                    id = 4, name = "Voltage", description = "Electric energy flows through you.",
                    songTitle = "Voltage", difficulty = "easy", genre = "techno",
                    bpm = 120, duration = 75f, scrollSpeed = 36f, noteDensity = 0.8f,
                    holdProbability = 0.1f, unlockScore = 2500
                },
                new LevelConfig {
                    id = 5, name = "Synth Rider", description = "Ride the waves of sound.",
                    songTitle = "Synth Rider", difficulty = "easy", genre = "synthwave",
                    bpm = 122, duration = 80f, scrollSpeed = 38f, noteDensity = 0.85f,
                    holdProbability = 0.1f, unlockScore = 3000
                },
                new LevelConfig {
                    id = 6, name = "Phase Shift", description = "Reality bends around you.",
                    songTitle = "Phase Shift", difficulty = "easy", genre = "techno",
                    bpm = 125, duration = 80f, scrollSpeed = 40f, noteDensity = 0.9f,
                    holdProbability = 0.12f, unlockScore = 3500
                },

                // Phase 2: Acceleration (Medium)
                new LevelConfig {
                    id = 7, name = "Turbo", description = "Speed increases. Stay focused.",
                    songTitle = "Turbo Boost", difficulty = "medium", genre = "dnb",
                    bpm = 128, duration = 80f, scrollSpeed = 45f, noteDensity = 1.0f,
                    holdProbability = 0.15f, unlockScore = 5000
                },
                new LevelConfig {
                    id = 8, name = "Bass Cannon", description = "The bass drops hard.",
                    songTitle = "Bass Cannon", difficulty = "medium", genre = "dnb",
                    bpm = 130, duration = 85f, scrollSpeed = 48f, noteDensity = 1.1f,
                    holdProbability = 0.15f, unlockScore = 6000
                },
                new LevelConfig {
                    id = 9, name = "Warp Drive", description = "Breaking through the sound barrier.",
                    songTitle = "Warp Drive", difficulty = "medium", genre = "techno",
                    bpm = 132, duration = 85f, scrollSpeed = 50f, noteDensity = 1.2f,
                    holdProbability = 0.18f, unlockScore = 7000
                },
                new LevelConfig {
                    id = 10, name = "Cyber Assault", description = "Digital warfare intensifies.",
                    songTitle = "Cyber Assault", difficulty = "medium", genre = "dnb",
                    bpm = 135, duration = 90f, scrollSpeed = 52f, noteDensity = 1.3f,
                    holdProbability = 0.18f, unlockScore = 8000
                },
                new LevelConfig {
                    id = 11, name = "Overdrive", description = "Push past your limits.",
                    songTitle = "Overdrive", difficulty = "medium", genre = "synthwave",
                    bpm = 138, duration = 90f, scrollSpeed = 55f, noteDensity = 1.4f,
                    holdProbability = 0.2f, unlockScore = 9000
                },
                new LevelConfig {
                    id = 12, name = "Event Horizon", description = "The point of no return.",
                    songTitle = "Event Horizon", difficulty = "medium", genre = "techno",
                    bpm = 140, duration = 95f, scrollSpeed = 58f, noteDensity = 1.5f,
                    holdProbability = 0.2f, unlockScore = 10000
                },

                // Phase 3: Transcendence (Hard)
                new LevelConfig {
                    id = 13, name = "Neural Storm", description = "Your mind expands beyond limits.",
                    songTitle = "Neural Storm", difficulty = "hard", genre = "dnb",
                    bpm = 142, duration = 95f, scrollSpeed = 62f, noteDensity = 1.6f,
                    holdProbability = 0.22f, unlockScore = 12000
                },
                new LevelConfig {
                    id = 14, name = "Quantum Flux", description = "Probability collapses in your favor.",
                    songTitle = "Quantum Flux", difficulty = "hard", genre = "techno",
                    bpm = 144, duration = 100f, scrollSpeed = 65f, noteDensity = 1.7f,
                    holdProbability = 0.25f, unlockScore = 14000
                },
                new LevelConfig {
                    id = 15, name = "Plasma Core", description = "Pure energy unleashed.",
                    songTitle = "Plasma Core", difficulty = "hard", genre = "dnb",
                    bpm = 146, duration = 100f, scrollSpeed = 68f, noteDensity = 1.8f,
                    holdProbability = 0.25f, unlockScore = 16000
                },
                new LevelConfig {
                    id = 16, name = "Singularity", description = "Time and space converge.",
                    songTitle = "Singularity", difficulty = "hard", genre = "techno",
                    bpm = 148, duration = 105f, scrollSpeed = 72f, noteDensity = 2.0f,
                    holdProbability = 0.28f, unlockScore = 18000
                },
                new LevelConfig {
                    id = 17, name = "Dark Matter", description = "The invisible force that binds all.",
                    songTitle = "Dark Matter", difficulty = "hard", genre = "synthwave",
                    bpm = 150, duration = 105f, scrollSpeed = 75f, noteDensity = 2.2f,
                    holdProbability = 0.3f, unlockScore = 20000
                },
                new LevelConfig {
                    id = 18, name = "Supernova", description = "Explode with the power of a dying star.",
                    songTitle = "Supernova", difficulty = "hard", genre = "dnb",
                    bpm = 152, duration = 110f, scrollSpeed = 78f, noteDensity = 2.4f,
                    holdProbability = 0.3f, unlockScore = 22000
                },

                // Phase 4: Mastery (Expert)
                new LevelConfig {
                    id = 19, name = "Hyperdrive", description = "Faster than light.",
                    songTitle = "Hyperdrive", difficulty = "expert", genre = "dnb",
                    bpm = 154, duration = 110f, scrollSpeed = 82f, noteDensity = 2.5f,
                    holdProbability = 0.32f, unlockScore = 25000
                },
                new LevelConfig {
                    id = 20, name = "Void Walker", description = "Between dimensions you find clarity.",
                    songTitle = "Void Walker", difficulty = "expert", genre = "techno",
                    bpm = 155, duration = 115f, scrollSpeed = 85f, noteDensity = 2.6f,
                    holdProbability = 0.35f, unlockScore = 28000
                },
                new LevelConfig {
                    id = 21, name = "Omega Pulse", description = "The ultimate heartbeat.",
                    songTitle = "Omega Pulse", difficulty = "expert", genre = "synthwave",
                    bpm = 156, duration = 115f, scrollSpeed = 90f, noteDensity = 2.7f,
                    holdProbability = 0.35f, unlockScore = 32000
                },
                new LevelConfig {
                    id = 22, name = "Infinity Loop", description = "The cycle never ends.",
                    songTitle = "Infinity Loop", difficulty = "expert", genre = "dnb",
                    bpm = 158, duration = 120f, scrollSpeed = 95f, noteDensity = 2.8f,
                    holdProbability = 0.38f, unlockScore = 36000
                },
                new LevelConfig {
                    id = 23, name = "Ascension", description = "Transcend the mortal plane.",
                    songTitle = "Ascension", difficulty = "expert", genre = "techno",
                    bpm = 159, duration = 120f, scrollSpeed = 100f, noteDensity = 2.9f,
                    holdProbability = 0.4f, unlockScore = 40000
                },
                new LevelConfig {
                    id = 24, name = "God Mode", description = "You have become one with the rhythm.",
                    songTitle = "God Mode", difficulty = "expert", genre = "synthwave",
                    bpm = 160, duration = 120f, scrollSpeed = 110f, noteDensity = 3.0f,
                    holdProbability = 0.5f, unlockScore = 50000
                }
            };
        }
    }
}
