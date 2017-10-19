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
                    AggregateId = aggregateId,
                    EventType = @event.GetType().Name,
                    Body = JsonConvert.SerializeObject(@event)
                };

                StoreEvent(eventStoreData);
            }
        }

        private void StoreEvent(EventStoreData eventStoreData)
        {
            using (var documentClient = new DocumentClient(new Uri(_config.Uri), _config.Key))
            {
                var collectionUri = UriFactory.CreateDocumentCollectionUri(_config.Database, _config.Collection);
                documentClient.CreateDocumentAsync(collectionUri, eventStoreData).GetAwaiter().GetResult();
            }
        }

        private List<EventStoreData> GetEventStoreData(string aggregateId)
        {
            using (var documentClient = new DocumentClient(new Uri(_config.Uri), _config.Key))
            {
                var collectionUri = UriFactory.CreateDocumentCollectionUri(_config.Database, _config.Collection);
                return documentClient.CreateDocumentQuery<EventStoreData>(collectionUri)
                     .Where(col => col.AggregateId.Equals(aggregateId))
                     .ToList();
            }
        }

        public List<Event> GetEvents(string aggregateId)
        {
            var eventStoreData = GetEventStoreData(aggregateId);
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
            var events = GetEvents(movieId);

            var movie = new Movie();
            movie.LoadFromEvents(events);
            return movie;
        }

    }
}