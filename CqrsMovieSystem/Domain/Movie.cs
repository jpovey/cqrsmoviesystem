namespace CqrsMovieSystem.Domain
{
    using System;
    using Events;

    public class Movie : AggregateRoot
    {
        public string MovieId { get; set; }
        public string Title { get; set; }
        public int AvaliableSeats { get; set; }
        public override string Id => MovieId;

        public Movie()
        {
            
        }

        public void ApplyEvent(Event @event)
        {
            var eventType = @event.GetType();
            if (eventType == typeof(MovieCreated))
            {
                Apply((MovieCreated)@event);
            }

            if (eventType == typeof(SeatsBooked))
            {
                Apply((SeatsBooked)@event);
            }
        }

        public Movie(string movieId, string title, int seats)
        {
            var movieCreated = new MovieCreated(movieId, title, seats);
            ApplyChange(movieCreated);
        }

        private void Apply(MovieCreated moviedCreated)
        {
            MovieId = moviedCreated.MovieId;
            Title = moviedCreated.Title;
            AvaliableSeats = moviedCreated.Seats;
        }

        public void BookSeats(int seatsToBook)
        {
            if (AvaliableSeats < seatsToBook)
            {
               return;
            }

            var seatsBooked = new SeatsBooked(seatsToBook);
            ApplyChange(seatsBooked);
        }

        private void Apply(SeatsBooked moviedCreated)
        {
            AvaliableSeats = AvaliableSeats - moviedCreated.SeatsToBook;
        }
    }
}