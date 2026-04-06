namespace PulseHighway.Input
{
    public enum InputEventType
    {
        Press,
        Release
    }

    public enum InputSource
    {
        Keyboard,
        Mouse,
        Touch
    }

    public struct InputEvent
    {
        public int lane;           // 0-4
        public float timestamp;    // Time.time
        public float audioTime;    // AudioSource.time
        public InputEventType type;
        public InputSource source;

        public InputEvent(int lane, InputEventType type, InputSource source, float timestamp, float audioTime = 0f)
        {
            this.lane = lane;
            this.type = type;
            this.source = source;
            this.timestamp = timestamp;
            this.audioTime = audioTime;
        }
    }
}
