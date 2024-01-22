using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.Model.Internal.MarshallTransformations;
using Amazon.Runtime.Internal.Transform;
using Customers.Api.Contracts.Data;
using Microsoft.Extensions.Options;
using System.Net;

using System.Text.Json;

namespace Customers.Api.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly IAmazonDynamoDB _dynamoDB;
    const string _tableName = "customers";

    public CustomerRepository(IAmazonDynamoDB dynamoDB)
    {
        _dynamoDB = dynamoDB;
    }

    public async Task<bool> CreateAsync(CustomerDto customer, CancellationToken cancellationToken)
    {
        customer.UpdatedAt = DateTime.UtcNow;
        var custAsJson = JsonSerializer.Serialize(customer);
        var custAttributes = Document.FromJson(custAsJson).ToAttributeMap();
        var createItemReq = new PutItemRequest
        {
            TableName = _tableName,
            Item = custAttributes,
            ConditionExpression = "attribute_not_exists(pk) and attribute_not_exists(sk)"
        };
        var response = await _dynamoDB.PutItemAsync(createItemReq, cancellationToken);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task<CustomerDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var custId = id.ToString();
        var getItemRequest = new GetItemRequest(_tableName, new Dictionary<string, AttributeValue>
        {
            { "pk", new AttributeValue(custId) },
            { "sk", new AttributeValue(custId) }
        });
        var response = await _dynamoDB.GetItemAsync(getItemRequest, cancellationToken);
        if (response.Item.Count == 0)
            return null;

        var itemDoc = Document.FromAttributeMap(response.Item);
        return JsonSerializer.Deserialize<CustomerDto?>(itemDoc.ToJson());
    }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var scanRequest = new ScanRequest()
        {
            TableName = _tableName
        };
        var response = await _dynamoDB.ScanAsync(scanRequest, cancellationToken);
        var customers = response.Items?.Select(item =>
        {
            var json = Document.FromAttributeMap(item).ToJson();
            return JsonSerializer.Deserialize<CustomerDto>(json);
        });
        return customers!;
    }

    public async Task<bool> UpdateAsync(CustomerDto customer, DateTime requestStarted, CancellationToken cancellationToken)
    {
        customer.UpdatedAt = DateTime.UtcNow;
        var custAsJson = JsonSerializer.Serialize(customer);
        var custAttributes = Document.FromJson(custAsJson).ToAttributeMap();
        var updateItemReq = new PutItemRequest
        {
            TableName = _tableName,
            Item = custAttributes,
            ConditionExpression = "UpdatedAt < :requestStarted",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":requestStarted", new AttributeValue{S = requestStarted.ToString("O")} }
            }
        };
        var response = await _dynamoDB.PutItemAsync(updateItemReq, cancellationToken);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var custId = id.ToString();
        var deleteItemRequest = new DeleteItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {"pk",new AttributeValue(custId) },
                {"sk",new AttributeValue(custId) }
            }
        };
        var response = await _dynamoDB.DeleteItemAsync(deleteItemRequest, cancellationToken);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }
}