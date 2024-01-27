using Customers.Api.Contracts.Requests;
using Customers.Api.Domain;
using System.Threading;

namespace Customers.Api.Services;

public interface ICustomerService
{
    Task<bool> CreateAsync(Customer customer, CancellationToken cancellationToken);

    Task<Customer?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<Customer?> GetByEmail(string email, CancellationToken cancellationToken);

    Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken);

    Task<bool> UpdateAsync(Customer customer, DateTime requestStarted, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ProcessCustomerBatchAsync(IEnumerable<CustomerBatchRequest> batch, CancellationToken cancellationToken);
    
    Task<bool> CreateOrderAsync(CustomerOrderRequest custOrderRequest, CancellationToken cancellationToken);  
}