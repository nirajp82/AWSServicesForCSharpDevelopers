using Customer.Consumer.Api.Contracts.Messages;
using MediatR;

namespace Customer.Consumer.Api.Handlers
{
    public class CustomerDeletedHandler : IRequestHandler<CustomerDeleted>
    {
        private readonly ILogger<CustomerDeletedHandler> _logger;

        public CustomerDeletedHandler(ILogger<CustomerDeletedHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(CustomerDeleted request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"CustomerDeleted: {request.Id}");
            return Unit.Task;
        }
    }
}