using Customer.Publisher.Api.Contracts.Requests;
using Domain = Customer.Publisher.Api.Domain;

namespace Customer.Publisher.Api.Mapping;

public static class ApiContractToDomainMapper
{
    public static Domain.Customer ToCustomer(this CustomerRequest request)
    {
        return new Domain.Customer
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            GitHubUsername = request.GitHubUsername,
            FullName = request.FullName,
            DateOfBirth = request.DateOfBirth
        };
    }

    public static Domain.Customer ToCustomer(this UpdateCustomerRequest request)
    {
        return new Domain.Customer
        {
            Id = request.Id,
            Email = request.Customer.Email,
            GitHubUsername = request.Customer.GitHubUsername,
            FullName = request.Customer.FullName,
            DateOfBirth = request.Customer.DateOfBirth
        };
    }
}
