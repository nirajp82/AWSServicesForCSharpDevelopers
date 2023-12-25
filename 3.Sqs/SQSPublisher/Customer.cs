using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQSPublisher
{
    internal class Customer
    {
        public required Guid Id { get; init; }
        public required string FullName { get; init; }
        public required string Email { get; init; }
        public required string GithubUserName { get; init; }
        public required DateTime DateOfBirth { get; init; }

        internal static Customer Create() 
        {
            return new Customer
            {
                Id = Guid.NewGuid(),
                Email = "John.Doe@email.com",
                FullName = "John Doe",
                DateOfBirth = new DateTime(1900, 1, 1),
                GithubUserName = "nirajp82"
            };
        }
    }
}
