using Customer.Publisher.Api.Contracts.Data;
using Customer.Publisher.Api.Contracts.Messages;
using Customer.Publisher.Api.Domain;

namespace Customer.Publisher.Api.Mapping
{
    public static class DomainToMessageMapper
    {
        public static CustomerCreated ToCustomerCreated(this Domain.Customer customer)
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

        public static CustomerUpdated ToCustomerUpdated(this Domain.Customer customer)
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
