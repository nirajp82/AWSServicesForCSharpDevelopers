using Customer.Publisher.Api.Contracts.Data;
using Customer.Publisher.Api.Domain;

namespace Customer.Publisher.Api.Mapping;

public static class DtoToDomainMapper
{
    public static Domain.Customer ToCustomer(this CustomerDto customerDto)
    {
        return new Domain.Customer
        {
            Id = customerDto.Id,
            Email = customerDto.Email,
            GitHubUsername = customerDto.GitHubUsername,
            FullName = customerDto.FullName,
            DateOfBirth = customerDto.DateOfBirth
        };
    }
}
