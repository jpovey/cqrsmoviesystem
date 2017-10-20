namespace CqrsMovieSystem.Snapshot
{
    using Repositories;

    public class SnapshotGenerator
    {
        private readonly EventStoreRepository _repository;

        public SnapshotGenerator(EventStoreRepository repository)
        {
            _repository = repository;
        }

        public void SnapshotMovieAggregate(string movieId)
        {
            var movie = _repository.GetMovie(movieId);
            _repository.SaveSnapshot(movie);

            // This should be done carefully and gradually as event are your source of truth. Just doing this for this test implementation
        }
    }
}