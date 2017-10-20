namespace CqrsMovieSystem.Repositories
{
    using System;
    using Newtonsoft.Json;

    public class EventStoreData
    {
        [JsonProperty(PropertyName = "id")]
        public string Id => Guid.NewGuid().ToString();

        [JsonProperty(PropertyName = "eventIndex")]
        public int EventIndex { get; set; }

        [JsonProperty(PropertyName = "aggregateId")]
        public string AggregateId { get; set; }
        
        public string Body { get; set; }

        public string EventType { get; set; }
    }
}