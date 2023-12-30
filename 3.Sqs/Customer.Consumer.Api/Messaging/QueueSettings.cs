using Microsoft.AspNetCore.Http;

namespace Customer.Consumer.Api
{
    public class QueueSettings
    {
        public const string Key = "Queue";
        public required string Name { get; init; }
    }
}
