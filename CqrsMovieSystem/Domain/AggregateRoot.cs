namespace CqrsMovieSystem.Domain
{
    using System.Collections.Generic;
    using Events;

    public abstract class AggregateRoot
    {
        public readonly List<Event> Changes = new List<Event>();
        public abstract string Id { get; }

        public void ApplyChange(Event @event)
        {
            ApplyChange(@event, true);
        }

        private void ApplyChange(Event @event, bool isNew)
        {
            this.Apply(@event);
            if (isNew) Changes.Add(@event);
        }

        private void Apply(Event @event)
        {
            dynamic aggregate = this;
            aggregate.ApplyEvent(@event);
        }

        public void LoadFromEvents(IEnumerable<Event> history)
        {
            foreach (var e in history) ApplyChange(e, false);
        }
    }
}