using Customers.Api.Domain;

namespace Customers.Api.Services;

public interface ICustomerService
{
    Task<bool> CreateAsync(Customer customer, CancellationToken cts);

    Task<Customer?> GetAsync(Guid id);

    Task<IEnumerable<Customer>> GetAllAsync();

    Task<bool> UpdateAsync(Customer customer, CancellationToken cts);

    Task<bool> DeleteAsync(Guid id, CancellationToken cts);
}
