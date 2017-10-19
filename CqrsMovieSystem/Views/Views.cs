namespace CqrsMovieSystem.Views
{
    using System.Linq;
    using Events;
    using Repositories;

    public class Views
    {
        private readonly EventStoreRepository _eventStoreRepository;

        public Views(EventStoreRepository eventStoreRepository)
        {
            _eventStoreRepository = eventStoreRepository;
        }

        public int GetSeatsBookedFor(string movieId)
        {
            var seatsBookedEvents = _eventStoreRepository.GetEvents(movieId).Where(x => x.GetType() == typeof(SeatsBooked));
            return seatsBookedEvents.Cast<SeatsBooked>().Sum(seatBookedEvent => seatBookedEvent.SeatsToBook);
        }
    }
}