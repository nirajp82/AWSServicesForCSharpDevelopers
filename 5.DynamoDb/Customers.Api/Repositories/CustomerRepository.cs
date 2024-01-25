using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Customers.Api.Contracts.Data;
using System.Net;

using System.Text.Json;

namespace Customers.Api.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly IAmazonDynamoDB _dynamoDB;
    private const string _tableName = "customers";

    // Constructor to initialize the DynamoDB client
    public CustomerRepository(IAmazonDynamoDB dynamoDB)
    {
        _dynamoDB = dynamoDB;
    }

    // Create a new customer asynchronously
    public async Task<bool> CreateAsync(CustomerDto customer, CancellationToken cancellationToken)
    {
        // Set the updated timestamp
        customer.UpdatedAt = DateTime.UtcNow;

        // Serialize customer object to JSON and convert to DynamoDB attribute map
        var custAsJson = JsonSerializer.Serialize(customer);
        var custAttributes = Document.FromJson(custAsJson).ToAttributeMap();

        // Prepare the PutItem request with conditional expression
        var createItemReq = new PutItemRequest
        {
            TableName = _tableName,
            Item = custAttributes,
            ConditionExpression = "attribute_not_exists(pk) and attribute_not_exists(sk)"
        };

        // Execute PutItem request and return success status
        var response = await _dynamoDB.PutItemAsync(createItemReq, cancellationToken);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    // Get customer by ID asynchronously
    public async Task<CustomerDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        // Convert Guid to string
        var custId = id.ToString();

        // Prepare GetItem request with primary key attributes
        var getItemRequest = new GetItemRequest(_tableName, new Dictionary<string, AttributeValue>
        {
            { "pk", new AttributeValue(custId) },
            { "sk", new AttributeValue(custId) }
        });

        // Execute GetItem request and deserialize response to CustomerDto
        var response = await _dynamoDB.GetItemAsync(getItemRequest, cancellationToken);
        if (response.Item.Count == 0)
            return null;

        var itemDoc = Document.FromAttributeMap(response.Item);
        return JsonSerializer.Deserialize<CustomerDto?>(itemDoc.ToJson());
    }

    // Get all customers asynchronously
    public async Task<IEnumerable<CustomerDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        // Prepare Scan request to retrieve all items from the table
        var scanRequest = new ScanRequest()
        {
            TableName = _tableName
        };

        // Execute Scan request and deserialize items to CustomerDto list
        var response = await _dynamoDB.ScanAsync(scanRequest, cancellationToken);
        var customers = response.Items?.Select(item =>
        {
            var json = Document.FromAttributeMap(item).ToJson();
            return JsonSerializer.Deserialize<CustomerDto>(json);
        });
        return customers!;
    }

    // Update customer asynchronously with optimistic locking
    public async Task<bool> UpdateAsync(CustomerDto customer, DateTime requestStarted, CancellationToken cancellationToken)
    {
        // Set the updated timestamp
        customer.UpdatedAt = DateTime.UtcNow;

        // Serialize customer object to JSON and convert to DynamoDB attribute map
        var custAsJson = JsonSerializer.Serialize(customer);
        var custAttributes = Document.FromJson(custAsJson).ToAttributeMap();

        // Prepare PutItem request with conditional expression for optimistic locking
        var updateItemReq = new PutItemRequest
        {
            TableName = _tableName,
            Item = custAttributes,
            
            // Specify a condition for the DynamoDB update operation using optimistic locking.
            // This condition ensures that the update will only be performed if the 'UpdatedAt'
            // attribute in the database is less than the 'requestStarted' timestamp.

            // ConditionExpression defines the condition for the update operation.
            // It checks if the 'UpdatedAt' attribute in the item is less than the specified timestamp.
            ConditionExpression = "UpdatedAt < :requestStarted",

            // ExpressionAttributeValues provides the values to be substituted into the ConditionExpression.
            // In this case, it includes a placeholder ":requestStarted" with the corresponding timestamp value.
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":requestStarted", new AttributeValue{S = requestStarted.ToString("O")} }
            }
        };

        // Execute PutItem request and return success status
        var response = await _dynamoDB.PutItemAsync(updateItemReq, cancellationToken);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    // Delete customer by ID asynchronously
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        // Convert Guid to string
        var custId = id.ToString();

        // Prepare DeleteItem request with primary key attributes
        var deleteItemRequest = new DeleteItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {"pk",new AttributeValue(custId) },
                {"sk",new AttributeValue(custId) }
            }
        };

        // Execute DeleteItem request and return success status
        var response = await _dynamoDB.DeleteItemAsync(deleteItemRequest, cancellationToken);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }
}