
using MediatR;

namespace Customer.Consumer.Api.Contracts.Messages
{
    public class CustomerDeleted : ISqsMessage
    {
        public required Guid Id { get; init; }
    }
}
