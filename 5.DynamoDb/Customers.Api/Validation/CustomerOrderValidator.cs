using Customers.Api.Contracts.Requests;
using FluentValidation;
using System.Text.RegularExpressions;

namespace Customers.Api.Validation
{
    public partial class CustomerOrderValidator : AbstractValidator<CustomerOrderRequest>
    {
        public CustomerOrderValidator()
        {
            RuleFor(x => x.FullName)
                .Matches(FullNameRegex());

            RuleFor(x => x.Email)
                .EmailAddress();

            RuleFor(x => x.OrderDetails).NotEmpty();
            RuleFor(x => x.ShippingAddress).NotNull();
            RuleFor(x => x.BillingAddress).NotNull();
        }

        [GeneratedRegex("^[a-z ,.'-]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-GB")]
        private static partial Regex FullNameRegex();

        [GeneratedRegex("^[a-z\\d](?:[a-z\\d]|-(?=[a-z\\d])){0,38}$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-GB")]
        private static partial Regex UsernameRegex();
    }
}