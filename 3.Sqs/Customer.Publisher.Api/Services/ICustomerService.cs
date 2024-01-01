using Customer.Publisher.Api.Domain;

namespace Customer.Publisher.Api.Services;

public interface ICustomerService
{
    Task<bool> CreateAsync(Domain.Customer customer, CancellationToken cts);

    Task<Domain.Customer?> GetAsync(Guid id);

    Task<IEnumerable<Domain.Customer>> GetAllAsync();

    Task<bool> UpdateAsync(Domain.Customer customer, CancellationToken cts);

    Task<bool> DeleteAsync(Guid id, CancellationToken cts);
}
