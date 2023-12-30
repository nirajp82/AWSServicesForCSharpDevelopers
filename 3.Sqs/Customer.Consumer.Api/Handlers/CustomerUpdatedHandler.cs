using Customer.Consumer.Api.Contracts.Messages;
using MediatR;

namespace Customer.Consumer.Api.Handlers
{
    public class CustomerUpdatedHandler : IRequestHandler<CustomerUpdated>
    {
        private readonly ILogger<CustomerUpdatedHandler> _logger;

        public CustomerUpdatedHandler(ILogger<CustomerUpdatedHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(CustomerUpdated request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"CustomerUpdated: {request.GitHubUsername}");
            return Unit.Task;
        }
    }
}