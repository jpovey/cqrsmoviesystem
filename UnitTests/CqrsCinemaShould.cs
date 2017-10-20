namespace UnitTests
{
    using System;
    using CqrsMovieSystem.Commands;
    using CqrsMovieSystem.Handlers;
    using CqrsMovieSystem.Repositories;
    using CqrsMovieSystem.Snapshot;
    using CqrsMovieSystem.Views;
    using NUnit.Framework;

    [TestFixture]
    public class CqrsCinemaShould
    {
        private MovieHandler _movieHandler;
        private Views _views;
        private SnapshotGenerator _snapshotGenerator;

        [SetUp]
        public void Setup()
        {
            var config = new RepositoryConfig
            {
                Collection = "Movie",
                Database = "MovieSystem",
                Key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                Uri = "https://localhost:8081/"
            };
            var repository = new EventStoreRepository(config);
            repository.Initialise();

            _movieHandler = new MovieHandler(repository);
            _views = new Views(repository);
            _snapshotGenerator = new SnapshotGenerator(repository);
        }

        [Test]
        public void CreateMovie()
        {
            //Create movie
            var movieId = Guid.NewGuid().ToString();
            var createMovieCommand = new CreateMovie
            {
                MovieId = movieId,
                Title = "Batman",
                Seats = 10
            };
            var movie = _movieHandler.Handle(createMovieCommand);

            Assert.AreEqual(movie.Id, movieId);
            Assert.AreEqual(movie.Title, "Batman");
            Assert.AreEqual(movie.AvaliableSeats, 10);

            // Book 2 seats
            var bookTwoSeatsCommand = new BookSeats
            {
                MovieId = movieId,
                Seats = 2
            };
            movie = _movieHandler.Handle(bookTwoSeatsCommand);

            Assert.AreEqual(movie.AvaliableSeats, 8);

            // Cant book more seats than available
            var bookTooManySeats = new BookSeats
            {
                MovieId = movieId,
                Seats = 10
            };
            movie = _movieHandler.Handle(bookTooManySeats);

            Assert.AreEqual(movie.AvaliableSeats, 8);

            // Book 4 seats
            var bookFourSeatsCommand = new BookSeats
            {
                MovieId = movieId,
                Seats = 4
            };
            _movieHandler.Handle(bookFourSeatsCommand);

            // How many seats booked
            var seatsBooked = _views.GetSeatsBookedFor(movieId);
            Assert.AreEqual(seatsBooked, 6);

            // Create a snapshot
             _snapshotGenerator.SnapshotMovieAggregate(movieId);

            // Get events from snapshot aggregate
            seatsBooked = _views.GetSeatsBookedFor(movieId);
            Assert.AreEqual(seatsBooked, 6);

            // Book 1 seats
            var bookOneSeatCommand = new BookSeats
            {
                MovieId = movieId,
                Seats = 1
            };
            movie = _movieHandler.Handle(bookOneSeatCommand);
            Assert.AreEqual(movie.AvaliableSeats, 3);

            // Book 1 seats
            movie = _movieHandler.Handle(bookOneSeatCommand);
            Assert.AreEqual(movie.AvaliableSeats, 2);

            // Create a new snapshot
            _snapshotGenerator.SnapshotMovieAggregate(movieId);

            // Book 1 seats
            movie = _movieHandler.Handle(bookOneSeatCommand);
            Assert.AreEqual(movie.AvaliableSeats, 1);
        }
    }
}
