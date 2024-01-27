using System.Text.Json.Serialization;

namespace Customers.Api.Contracts.Data;

public class OrderDto
{
    [JsonPropertyName("pk")]
    public string Pk => OrderId.ToString();

    [JsonPropertyName("sk")]
    public string Sk => CustomerId.ToString();

    public Guid OrderId { get; init; } = default!;
    public Guid CustomerId { get; init; } = default!;
    public DateTime OrderDate { get; init; } = DateTime.UtcNow;
    public double OrderTotal
    {
        get
        {
            return OrderDetails.Sum(o => o.Price);
        }
    }
    public AddressDto ShippingAddress { get; init; } = default!;
    public AddressDto BillingAddress { get; init; } = default!;
    public IEnumerable<OrderItemDto> OrderDetails { get; init; } = default!;
}

public class OrderItemDto
{
    public Guid ProductId { get; init; } = default!;
    public double Price { get; init; } = default!;
}

public class AddressDto
{
    public string Address1 { get; init; } = default!;
    public string Address2 { get; init; } = default!;
    public string City { get; init; } = default!;
    public string State { get; init; } = default!;
    public string Zip { get; init; } = default!;
}