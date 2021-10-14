using System;

namespace BaseListener.Boundary
{
    /// <summary>
    /// TODO
    /// Suggested model of the event message received by the function
    /// This will of course be dependent on the specific message expected.
    /// </summary>
    public class EntityEventSns
    {
        public Guid Id { get; set; }

        public string EventType { get; set; }

        public string SourceDomain { get; set; }

        public string SourceSystem { get; set; }

        public string Version { get; set; }

        public Guid CorrelationId { get; set; }

        public DateTime DateTime { get; set; }

        public User User { get; set; }

        public Guid EntityId { get; set; }

        public EventData EventData { get; set; } = new EventData();
    }
}
