using Customers.Api.Contracts.Data;

namespace Customers.Api.Repositories;

public interface ICustomerRepository
{
    Task<bool> CreateAsync(CustomerDto customer, CancellationToken cancellationToken);

    Task<CustomerDto?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<IEnumerable<CustomerDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<bool> UpdateAsync(CustomerDto customer, DateTime requestStarted, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ProcessCustomerBatchAsync(IEnumerable<CustomerDto>? customersToCreate, IEnumerable<CustomerDto>? customersToUpdate,
            IEnumerable<CustomerDto>? customersToDelete, CancellationToken cancellationToken);
    Task<bool> CreateOrderAsync(CustomerDto customer, OrderDto order, CancellationToken cancellationToken);
}
