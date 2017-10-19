namespace CqrsMovieSystem.Events
{
    public class SeatsBooked : Event
    {
        public readonly int SeatsToBook;

        public SeatsBooked(int seatsToBook)
        {
            SeatsToBook = seatsToBook;
        }
    }
}