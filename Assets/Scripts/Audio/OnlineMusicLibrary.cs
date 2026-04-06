namespace PulseHighway.Audio
{
    [System.Serializable]
    public class OnlineTrack
    {
        public string title;
        public string artist;
        public string genre;
        public string url;
        public int estimatedBpm;
        public float estimatedDuration;
        public string license;

        public OnlineTrack(string title, string artist, string genre, string url,
            int bpm, float duration, string license)
        {
            this.title = title;
            this.artist = artist;
            this.genre = genre;
            this.url = url;
            this.estimatedBpm = bpm;
            this.estimatedDuration = duration;
            this.license = license;
        }
    }

    public static class OnlineMusicLibrary
    {
        // Curated royalty-free tracks (CC BY / CC0)
        // These are direct-download URLs to freely licensed music
        public static readonly OnlineTrack[] Tracks = new OnlineTrack[]
        {
            new OnlineTrack(
                "Synthwave Retrowave",
                "Music_Unlimited",
                "synthwave",
                "https://cdn.pixabay.com/audio/2022/05/27/audio_1808fbf07a.mp3",
                120, 120f, "Pixabay License (Free)"
            ),
            new OnlineTrack(
                "Cyberpunk Street",
                "Infraction",
                "synthwave",
                "https://cdn.pixabay.com/audio/2022/02/22/audio_d1718ab41b.mp3",
                128, 145f, "Pixabay License (Free)"
            ),
            new OnlineTrack(
                "Electronic Future Beats",
                "QubeSounds",
                "techno",
                "https://cdn.pixabay.com/audio/2022/10/25/audio_574462e0a6.mp3",
                130, 108f, "Pixabay License (Free)"
            ),
            new OnlineTrack(
                "Drive Breakbeat",
                "Rockot",
                "dnb",
                "https://cdn.pixabay.com/audio/2022/08/02/audio_884fe92c21.mp3",
                140, 125f, "Pixabay License (Free)"
            ),
            new OnlineTrack(
                "Powerful Beat",
                "Soundfield",
                "techno",
                "https://cdn.pixabay.com/audio/2023/07/19/audio_e552a1e1b5.mp3",
                126, 130f, "Pixabay License (Free)"
            ),
            new OnlineTrack(
                "Neon Gaming",
                "GoodBMusic",
                "synthwave",
                "https://cdn.pixabay.com/audio/2023/10/08/audio_9a09403832.mp3",
                132, 115f, "Pixabay License (Free)"
            ),
            new OnlineTrack(
                "Energetic Sport",
                "MusicField",
                "dnb",
                "https://cdn.pixabay.com/audio/2024/01/10/audio_c7d0e7c412.mp3",
                145, 120f, "Pixabay License (Free)"
            ),
            new OnlineTrack(
                "Abstract Future Bass",
                "ComaStudio",
                "techno",
                "https://cdn.pixabay.com/audio/2023/04/11/audio_79920e4735.mp3",
                135, 105f, "Pixabay License (Free)"
            ),
        };
    }
}
