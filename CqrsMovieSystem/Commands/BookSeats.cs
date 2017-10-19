namespace CqrsMovieSystem.Commands
{
    public class BookSeats : Command
    {
        public string MovieId { get; set; }
        public int Seats { get; set; }
    }
}