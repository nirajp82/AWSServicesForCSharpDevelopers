namespace Customers.Api.Contracts.Requests;
public class CustomerOrderRequest
{
    public string FullName { get; init; } = default!;
    public string Email { get; init; } = default!;
    public AddressRequest ShippingAddress { get; init; } = default!;
    public AddressRequest BillingAddress { get; init; } = default!;
    public IEnumerable<OrderItemRequest> OrderDetails { get; init; } = default!;
}

public class AddressRequest
{
    public string Address1 { get; init; } = default!;
    public string Address2 { get; init; } = default!;
    public string City { get; init; } = default!;
    public string State { get; init; } = default!;
    public string Zip { get; init; } = default!;
}

public class OrderItemRequest
{
    public Guid ProductId { get; init; } = default!;
    public double Price { get; init; } = default!;
}