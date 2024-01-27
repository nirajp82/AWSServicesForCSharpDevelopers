using Customers.Api.Contracts.Requests;
using Customers.Api.Domain;
using Customers.Api.Mapping;
using Customers.Api.Repositories;
using FluentValidation;
using FluentValidation.Results;

namespace Customers.Api.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IGitHubService _gitHubService;

    public CustomerService(ICustomerRepository customerRepository,
        IGitHubService gitHubService)
    {
        _customerRepository = customerRepository;
        _gitHubService = gitHubService;
    }

    public async Task<bool> CreateAsync(Customer customer, CancellationToken cancellationToken)
    {
        var existingUser = await _customerRepository.GetAsync(customer.Id, cancellationToken);
        if (existingUser is not null)
        {
            var message = $"A user with id {customer.Id} already exists";
            throw new ValidationException(message, GenerateValidationError(nameof(Customer), message));
        }

        var isValidGitHubUser = await _gitHubService.IsValidGitHubUser(customer.GitHubUsername);
        if (!isValidGitHubUser)
        {
            var message = $"There is no GitHub user with username {customer.GitHubUsername}";
            throw new ValidationException(message, GenerateValidationError(nameof(customer.GitHubUsername), message));
        }

        var customerDto = customer.ToCustomerDto();
        return await _customerRepository.CreateAsync(customerDto, cancellationToken);
    }

    public async Task<Customer?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var customerDto = await _customerRepository.GetAsync(id, cancellationToken);
        return customerDto?.ToCustomer();
    }

    public async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken)
    {
        var customerDtos = await _customerRepository.GetAllAsync(cancellationToken);
        return customerDtos.Select(x => x.ToCustomer());
    }

    public async Task<bool> UpdateAsync(Customer customer, DateTime requestStarted, CancellationToken cancellationToken)
    {
        var customerDto = customer.ToCustomerDto();

        var isValidGitHubUser = await _gitHubService.IsValidGitHubUser(customer.GitHubUsername);
        if (!isValidGitHubUser)
        {
            var message = $"There is no GitHub user with username {customer.GitHubUsername}";
            throw new ValidationException(message, GenerateValidationError(nameof(customer.GitHubUsername), message));
        }

        return await _customerRepository.UpdateAsync(customerDto, requestStarted, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _customerRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<bool> ProcessCustomerBatchAsync(IEnumerable<CustomerBatchRequest> batch, CancellationToken cancellationToken)
    {
        var customersToCreate = batch.ToCustomersDto(CustomerBatchRequest.ActionType.Create);
        var customersToUpdate = batch.ToCustomersDto(CustomerBatchRequest.ActionType.Update);
        var customersToDelete = batch.ToCustomersDto(CustomerBatchRequest.ActionType.Delete);
        return await _customerRepository.ProcessCustomerBatchAsync(customersToCreate, customersToUpdate, customersToDelete, cancellationToken);
    }

    private static ValidationFailure[] GenerateValidationError(string paramName, string message)
    {
        return new[]
        {
            new ValidationFailure(paramName, message)
        };
    }
}
