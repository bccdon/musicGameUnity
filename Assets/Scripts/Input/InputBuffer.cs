using UnityEngine;

namespace PulseHighway.Input
{
    public class InputBuffer
    {
        private InputEvent[] buffer;
        private int head;
        private int count;
        private int maxSize;

        public InputBuffer(int maxSize = 180)
        {
            this.maxSize = maxSize;
            buffer = new InputEvent[maxSize];
            head = 0;
            count = 0;
        }

        public void Add(InputEvent evt)
        {
            buffer[head] = evt;
            head = (head + 1) % maxSize;
            if (count < maxSize) count++;
        }

        public InputEvent? GetClosest(float targetTime, int lane, float maxDiff)
        {
            InputEvent? best = null;
            float bestDiff = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                int idx = (head - 1 - i + maxSize) % maxSize;
                var evt = buffer[idx];

                if (evt.lane != lane) continue;
                if (evt.type != InputEventType.Press) continue;

                float diff = Mathf.Abs(evt.timestamp - targetTime);
                if (diff <= maxDiff && diff < bestDiff)
                {
                    bestDiff = diff;
                    best = evt;
                }
            }

            return best;
        }

        public void Clear()
        {
            head = 0;
            count = 0;
        }
    }
}
