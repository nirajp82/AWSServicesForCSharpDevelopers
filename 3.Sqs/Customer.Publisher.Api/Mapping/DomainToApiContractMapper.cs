using Customer.Publisher.Api.Contracts.Responses;
using Domain = Customer.Publisher.Api.Domain;

namespace Customer.Publisher.Api.Mapping;

public static class DomainToApiContractMapper
{
    public static CustomerResponse ToCustomerResponse(this Domain.Customer customer)
    {
        return new CustomerResponse
        {
            Id = customer.Id,
            Email = customer.Email,
            GitHubUsername = customer.GitHubUsername,
            FullName = customer.FullName,
            DateOfBirth = customer.DateOfBirth
        };
    }

    public static GetAllCustomersResponse ToCustomersResponse(this IEnumerable<Domain.Customer> customers)
    {
        return new GetAllCustomersResponse
        {
            Customers = customers.Select(x => new CustomerResponse
            {
                Id = x.Id,
                Email = x.Email,
                GitHubUsername = x.GitHubUsername,
                FullName = x.FullName,
                DateOfBirth = x.DateOfBirth
            })
        };
    }
}
