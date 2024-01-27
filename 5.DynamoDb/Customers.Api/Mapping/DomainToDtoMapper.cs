using Customers.Api.Contracts.Data;
using Customers.Api.Contracts.Requests;
using Customers.Api.Domain;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Customers.Api.Mapping;

public static class DomainToDtoMapper
{
    public static CustomerDto ToCustomerDto(this Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            Email = customer.Email,
            GitHubUsername = customer.GitHubUsername,
            FullName = customer.FullName,
            DateOfBirth = customer.DateOfBirth
        };
    }

    public static IEnumerable<CustomerDto>? ToCustomersDto(this IEnumerable<CustomerBatchRequest> customers, CustomerBatchRequest.ActionType actionType)
    {
        ICollection<CustomerDto> customersDto = new List<CustomerDto>();
        foreach (var customer in customers.Where(c => c.Action == actionType))
        {
            var customerDto = new CustomerDto
            {
                Id = customer.CustomerId ?? new Guid(),
                Email = customer.Email,
                GitHubUsername = customer.GitHubUsername,
                FullName = customer.FullName,
                DateOfBirth = customer.DateOfBirth
            };
            customersDto.Add(customerDto);
        }
        return customersDto;
    }
}
