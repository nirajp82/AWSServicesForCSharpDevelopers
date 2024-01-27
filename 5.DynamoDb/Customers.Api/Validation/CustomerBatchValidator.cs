using Customers.Api.Contracts.Requests;
using FluentValidation;
using System.Text.RegularExpressions;

namespace Customers.Api.Validation;

public partial class CustomerBatchValidator : AbstractValidator<List<CustomerBatchRequest>>
{
    public CustomerBatchValidator()
    {
        RuleFor(x => x).NotEmpty();
        RuleForEach(x => x).SetValidator(new CustomerBatchRequestValidator());
    }
}

public partial class CustomerBatchRequestValidator : AbstractValidator<CustomerBatchRequest>
{
    public CustomerBatchRequestValidator()
    {
        RuleFor(x => x.FullName)
            .Matches(FullNameRegex());

        RuleFor(x => x.Email)
            .EmailAddress();

        RuleFor(x => x.GitHubUsername)
            .Matches(UsernameRegex());

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Now)
            .WithMessage("Your date of birth cannot be in the future");
    }

    [GeneratedRegex("^[a-z ,.'-]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-GB")]
    private static partial Regex FullNameRegex();

    [GeneratedRegex("^[a-z\\d](?:[a-z\\d]|-(?=[a-z\\d])){0,38}$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-GB")]
    private static partial Regex UsernameRegex();
}