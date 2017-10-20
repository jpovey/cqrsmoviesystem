namespace CqrsMovieSystem.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain;
    using Events;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Newtonsoft.Json;

    public class EventStoreRepository
    {
        private readonly RepositoryConfig _config;
        private readonly Dictionary<Guid, List<Event>> _current = new Dictionary<Guid, List<Event>>();

        public EventStoreRepository(RepositoryConfig config)
        {
            this._config = config;
        }

        public void Initialise()
        {
            using (var documentClient = new DocumentClient(new Uri(_config.Uri), _config.Key))
            {
                var database = new Database { Id = _config.Database };
                documentClient.CreateDatabaseIfNotExistsAsync(database).GetAwaiter().GetResult();

                var databaseUri = UriFactory.CreateDatabaseUri(_config.Database);
                var collection = new DocumentCollection { Id = _config.Collection };
                collection.PartitionKey.Paths.Add("/aggregateId");
                collection.IndexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/aggregateId/?" });
                collection.IndexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/eventIndex/?" });
                collection.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath { Path = "/*" });
                var requestOptions = new RequestOptions { OfferThroughput = 400 };
                documentClient.CreateDocumentCollectionIfNotExistsAsync(databaseUri, collection, requestOptions).GetAwaiter().GetResult();
            }
        }

        public void Save(AggregateRoot aggregate)
        {
            var newEvents = aggregate.Changes;
            var aggregateId = aggregate.Id;

            foreach (var @event in newEvents)
            {
                var eventStoreData = new EventStoreData
                {
                    EventIndex = @event.Index,
                    AggregateId = aggregateId,
                    EventType = @event.GetType().Name,
                    Body = JsonConvert.SerializeObject(@event)
                };

                StoreEventStoreData(eventStoreData);
            }
        }

        public void SaveSnapshot(AggregateRoot aggregate)
        {
            var eventStoreData = new EventStoreData
            {
                EventIndex = aggregate.EventIndex,
                AggregateId = $"{aggregate.Id}_aggregate",
                EventType = aggregate.GetType().Name,
                Body = JsonConvert.SerializeObject(aggregate)
            };

            StoreEventStoreData(eventStoreData);
        }

        private void StoreEventStoreData(EventStoreData eventStoreData)
        {
            using (var documentClient = new DocumentClient(new Uri(_config.Uri), _config.Key))
            {
                var collectionUri = UriFactory.CreateDocumentCollectionUri(_config.Database, _config.Collection);
                documentClient.CreateDocumentAsync(collectionUri, eventStoreData).GetAwaiter().GetResult();
            }
        }

        private List<EventStoreData> GetEventStoreData(string aggregateId, int index = 0)
        {
            using (var documentClient = new DocumentClient(new Uri(_config.Uri), _config.Key))
            {
                var collectionUri = UriFactory.CreateDocumentCollectionUri(_config.Database, _config.Collection);
                return documentClient.CreateDocumentQuery<EventStoreData>(collectionUri)
                     .Where(col => col.AggregateId.Equals(aggregateId) && col.EventIndex > index)
                     .OrderBy(col => col.EventIndex)
                     .ToList();
            }
        }

        public List<Event> GetEvents(string aggregateId, int eventIndex = 0)
        {
            var eventStoreData = GetEventStoreData(aggregateId, eventIndex);
            var events = new List<Event>();
            foreach (var eventStore in eventStoreData)
            {
                Event @event = null;
                var body = eventStore.Body;
                if (eventStore.EventType.Equals(typeof(MovieCreated).Name))
                {
                    @event = JsonConvert.DeserializeObject<MovieCreated>(body);
                }

                if (eventStore.EventType.Equals(typeof(SeatsBooked).Name))
                {
                    @event = JsonConvert.DeserializeObject<SeatsBooked>(body);
                }

                if (@event != null)
                {
                    events.Add(@event);
                }
            }

            return events;
        }

        public Movie GetMovie(string movieId)
        {
            var movie = GetSnapshot(movieId);
            var events = GetEvents(movieId, movie.EventIndex);
            movie.LoadFromEvents(events);
            return movie;
        }

        private Movie GetSnapshot(string movieId)
        {
            using (var documentClient = new DocumentClient(new Uri(_config.Uri), _config.Key))
            {
                var collectionUri = UriFactory.CreateDocumentCollectionUri(_config.Database, _config.Collection);
                var snapshot = documentClient.CreateDocumentQuery<EventStoreData>(collectionUri)
                    .Where(col => col.AggregateId.Equals($"{movieId}_aggregate"))
                    .OrderByDescending(col => col.EventIndex)
                    .ToList()
                    .FirstOrDefault();

                return snapshot == null ? new Movie() : JsonConvert.DeserializeObject<Movie>(snapshot.Body);
            }
        }
    }
}