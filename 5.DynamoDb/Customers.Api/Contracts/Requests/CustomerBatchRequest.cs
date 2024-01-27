namespace Customers.Api.Contracts.Requests;

public class CustomerBatchRequest
{
    public enum ActionType 
    {
        Create,
        Update,
        Delete
    }

    public ActionType Action { get; init; } = default!;

    public Guid? CustomerId { get; init; } = default;

    public string GitHubUsername { get; init; } = default!;

    public string FullName { get; init; } = default!;

    public string Email { get; init; } = default!;

    public DateTime DateOfBirth { get; init; } = default!;
}
