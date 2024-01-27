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
                Id = customer.CustomerId ?? Guid.NewGuid(),
                Email = customer.Email,
                GitHubUsername = customer.GitHubUsername,
                FullName = customer.FullName,
                DateOfBirth = customer.DateOfBirth
            };
            customersDto.Add(customerDto);
        }
        return customersDto;
    }

    public static CustomerDto ToCustomerDto(this CustomerOrderRequest custOrderRequest)
    {
        var customerDto = new CustomerDto
        {
            Id = Guid.NewGuid(),
            Email = custOrderRequest.Email,
            FullName = custOrderRequest.FullName,
        };
        return customerDto;
    }

    public static OrderDto ToOrderDto(this CustomerOrderRequest custOrderRequest, Guid customerId)
    {
        var orderDto = new OrderDto
        {
            OrderId = Guid.NewGuid(),
            CustomerId = customerId,
            BillingAddress = custOrderRequest.BillingAddress.ToAddressDto(),
            ShippingAddress = custOrderRequest.ShippingAddress.ToAddressDto(),
            OrderDetails = custOrderRequest.OrderDetails.ToOrderItemsDto()
        };
        return orderDto;
    }

    public static AddressDto ToAddressDto(this AddressRequest addressRequest)
    {
        return new AddressDto
        {
            Address1 = addressRequest.Address1,
            Address2 = addressRequest.Address2,
            City = addressRequest.City,
            State = addressRequest.State,
            Zip = addressRequest.Zip
        };
    }

    public static IEnumerable<OrderItemDto> ToOrderItemsDto(this IEnumerable<OrderItemRequest> orderItems)
    {
        return from order in orderItems
               select new OrderItemDto
               {
                   Price = order.Price,
                   ProductId = order.ProductId
               };
    }
}
