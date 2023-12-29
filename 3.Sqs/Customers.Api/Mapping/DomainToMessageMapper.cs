using Customers.Api.Contracts.Data;
using Customers.Api.Contracts.Messages;
using Customers.Api.Domain;

namespace Customers.Api.Mapping
{
    public static class DomainToMessageMapper
    {
        public static CustomerCreated ToCustomerCreated(this Customer customer)
        {
            return new CustomerCreated
            {
                Id = customer.Id,
                Email = customer.Email,
                GitHubUsername = customer.GitHubUsername,
                FullName = customer.FullName,
                DateOfBirth = customer.DateOfBirth
            };
        }

        public static CustomerUpdated ToCustomerUpdated(this Customer customer)
        {
            return new CustomerUpdated
            {
                Id = customer.Id,
                Email = customer.Email,
                GitHubUsername = customer.GitHubUsername,
                FullName = customer.FullName,
                DateOfBirth = customer.DateOfBirth
            };
        }
    }
}
