using NUnit.Framework;
using PulseHighway.Input;

namespace PulseHighway.Tests.EditMode.Input
{
    [TestFixture]
    public class InputBufferTests
    {
        private InputBuffer buffer;

        [SetUp]
        public void SetUp()
        {
            buffer = new InputBuffer(10); // Small buffer for testing
        }

        [Test]
        public void NewBuffer_GetClosest_ReturnsNull()
        {
            Assert.IsNull(buffer.GetClosest(0f, 0, 1f));
        }

        [Test]
        public void Add_SingleEvent_CanRetrieve()
        {
            var evt = new InputEvent(2, InputEventType.Press, InputSource.Keyboard, 1.0f);
            buffer.Add(evt);

            var result = buffer.GetClosest(1.0f, 2, 0.5f);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Value.lane);
        }

        [Test]
        public void GetClosest_MatchesCorrectLane()
        {
            buffer.Add(new InputEvent(0, InputEventType.Press, InputSource.Keyboard, 1.0f));
            buffer.Add(new InputEvent(3, InputEventType.Press, InputSource.Keyboard, 1.0f));

            // Request lane 3
            var result = buffer.GetClosest(1.0f, 3, 0.5f);
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Value.lane);

            // Request lane 4 - not present
            var none = buffer.GetClosest(1.0f, 4, 0.5f);
            Assert.IsNull(none);
        }

        [Test]
        public void GetClosest_IgnoresReleaseEvents()
        {
            buffer.Add(new InputEvent(0, InputEventType.Release, InputSource.Keyboard, 1.0f));

            var result = buffer.GetClosest(1.0f, 0, 0.5f);
            Assert.IsNull(result, "Should ignore Release events");
        }

        [Test]
        public void GetClosest_ReturnsClosestToTarget()
        {
            buffer.Add(new InputEvent(0, InputEventType.Press, InputSource.Keyboard, 1.0f));
            buffer.Add(new InputEvent(0, InputEventType.Press, InputSource.Keyboard, 2.0f));
            buffer.Add(new InputEvent(0, InputEventType.Press, InputSource.Keyboard, 3.0f));

            var result = buffer.GetClosest(2.1f, 0, 1f);
            Assert.IsNotNull(result);
            Assert.AreEqual(2.0f, result.Value.timestamp, 0.01f);
        }

        [Test]
        public void GetClosest_RespectsMaxDiff()
        {
            buffer.Add(new InputEvent(0, InputEventType.Press, InputSource.Keyboard, 1.0f));

            // Target=5.0, maxDiff=0.5 — event at 1.0 is 4.0 away
            var result = buffer.GetClosest(5.0f, 0, 0.5f);
            Assert.IsNull(result, "Should not find event outside maxDiff");
        }

        [Test]
        public void Add_OverCapacity_StillWorks()
        {
            // Buffer size is 10, add 15 events
            for (int i = 0; i < 15; i++)
            {
                buffer.Add(new InputEvent(0, InputEventType.Press, InputSource.Keyboard, (float)i));
            }

            // Should still find recent events
            var result = buffer.GetClosest(14f, 0, 0.5f);
            Assert.IsNotNull(result);
            Assert.AreEqual(14f, result.Value.timestamp, 0.01f);
        }

        [Test]
        public void Clear_ResetsBuffer()
        {
            buffer.Add(new InputEvent(0, InputEventType.Press, InputSource.Keyboard, 1.0f));
            buffer.Clear();

            var result = buffer.GetClosest(1.0f, 0, 1f);
            Assert.IsNull(result, "Buffer should be empty after Clear");
        }
    }
}
