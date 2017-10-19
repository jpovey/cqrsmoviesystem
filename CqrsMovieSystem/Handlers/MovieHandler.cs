namespace CqrsMovieSystem.Handlers
{
    using Commands;
    using Domain;
    using Repositories;

    public class MovieHandler
    {
        private readonly EventStoreRepository _eventStoreRepository;

        public MovieHandler(EventStoreRepository eventStoreRepository)
        {
            _eventStoreRepository = eventStoreRepository;
        }

        public Movie Handle(CreateMovie command)
        {
            var movie = _eventStoreRepository.GetMovie(command.MovieId);
            if (!string.IsNullOrEmpty(movie.Id))
            {
                return movie;
            }

            movie = new Movie(command.MovieId, command.Title, command.Seats);
            _eventStoreRepository.Save(movie);

            return movie;
        }

        public Movie Handle(BookSeats command)
        {
            var movie = _eventStoreRepository.GetMovie(command.MovieId);
            movie.BookSeats(command.Seats);
            _eventStoreRepository.Save(movie);

            return movie;
        }
    }
}