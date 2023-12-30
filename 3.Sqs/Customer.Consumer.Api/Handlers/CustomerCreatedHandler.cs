using Customer.Consumer.Api.Contracts.Messages;
using MediatR;

namespace Customer.Consumer.Api.Handlers
{
    public class CustomerCreatedHandler : IRequestHandler<CustomerCreated>
    {
        private readonly ILogger<CustomerCreated> _logger;

        public CustomerCreatedHandler(ILogger<CustomerCreated> logger)
        {
            _logger = logger;
        }

        public Task Handle(CustomerCreated request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"CustomerCreated: {request.FullName}");
            return Unit.Task;
        }
    }
}