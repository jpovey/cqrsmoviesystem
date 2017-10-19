namespace CqrsMovieSystem.Events
{
    public class MovieCreated : Event
    {
        public string MovieId { get; set; }
        public string Title { get; set; }
        public int Seats { get; set; }

        public MovieCreated(string movieId, string title, int seats)
        {
            this.MovieId = movieId;
            this.Title = title;
            this.Seats = seats;
        }
    }
}