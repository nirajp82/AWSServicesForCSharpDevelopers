using Customers.Api.Domain;
using System.Threading;

namespace Customers.Api.Services;

public interface ICustomerService
{
    Task<bool> CreateAsync(Customer customer, CancellationToken cancellationToken);

    Task<Customer?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken);

    Task<bool> UpdateAsync(Customer customer, DateTime requestStarted, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}