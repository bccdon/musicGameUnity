namespace PulseHighway.Audio
{
    public abstract class GenreGenerator
    {
        protected int sampleRate;
        protected int bpm;
        protected float duration;
        protected System.Random rng;

        protected float beatDuration => 60f / bpm;
        protected float barDuration => beatDuration * 4f;

        public GenreGenerator(int sampleRate, int bpm, float duration, int seed)
        {
            this.sampleRate = sampleRate;
            this.bpm = bpm;
            this.duration = duration;
            this.rng = new System.Random(seed);
        }

        public abstract void FillBuffer(float[] buffer, int channels);

        protected float GetSongSection(float time)
        {
            float progress = time / duration;
            if (progress < 0.15f) return 0f;      // Intro
            if (progress < 0.40f) return 1f;      // Build
            if (progress < 0.80f) return 2f;      // Drop
            return 3f;                             // Outro
        }

        protected float GetSectionIntensity(float time)
        {
            float section = GetSongSection(time);
            return section switch
            {
                0f => 0.4f,  // Intro
                1f => 0.65f, // Build
                2f => 1.0f,  // Drop
                3f => 0.5f,  // Outro
                _ => 0.5f
            };
        }

        protected void MixSample(float[] buffer, int index, float value, int channels)
        {
            for (int ch = 0; ch < channels; ch++)
            {
                int i = index * channels + ch;
                if (i < buffer.Length)
                    buffer[i] += value;
            }
        }

        protected void AddStereoSample(float[] buffer, int index, float left, float right, int channels)
        {
            int i = index * channels;
            if (channels >= 2 && i + 1 < buffer.Length)
            {
                buffer[i] += left;
                buffer[i + 1] += right;
            }
            else if (i < buffer.Length)
            {
                buffer[i] += (left + right) * 0.5f;
            }
        }

        protected float NextFloat()
        {
            return (float)rng.NextDouble();
        }

        protected float NextFloat(float min, float max)
        {
            return min + (float)rng.NextDouble() * (max - min);
        }
    }
}
