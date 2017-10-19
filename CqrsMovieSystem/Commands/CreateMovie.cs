namespace CqrsMovieSystem.Commands
{
    public class CreateMovie : Command
    {
        public string MovieId { get; set; }
        public string Title { get; set; }
        public int Seats { get; set; }
    }
}