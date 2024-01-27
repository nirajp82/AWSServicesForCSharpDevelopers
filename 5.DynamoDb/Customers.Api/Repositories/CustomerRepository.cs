using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Customers.Api.Contracts.Data;
using System.ComponentModel;
using System.Net;

using System.Text.Json;

namespace Customers.Api.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly IAmazonDynamoDB _dynamoDB;
    private const string _custTableName = "customers";
    private const string _orderTableName = "orders";

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
            TableName = _custTableName,
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
        var getItemRequest = new GetItemRequest(_custTableName, new Dictionary<string, AttributeValue>
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
            TableName = _custTableName
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
            TableName = _custTableName,
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
            TableName = _custTableName,
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

    #region Batch Operation

    public async Task<bool> ProcessCustomerBatchAsync(IEnumerable<CustomerDto>? customersToCreate, IEnumerable<CustomerDto>? customersToUpdate,
        IEnumerable<CustomerDto>? customersToDelete, CancellationToken cancellationToken)
    {
        //This request encapsulates multiple write operations that are sent to DynamoDB in a single request.
        //This is more efficient than making individual requests for each operation.
        List<WriteRequest> writeRequests = new List<WriteRequest>();

        //To add items, you use `PutRequest` within `WriteRequest`.
        BatchPutRequests(customersToCreate, writeRequests);
        //use a `PutRequest` within `WriteRequest` to update an existing item.
        BatchPutRequests(customersToUpdate, writeRequests);
        //For deletion, use `DeleteRequest` within `WriteRequest`.
        BatchDeleteRequests(customersToDelete, writeRequests);

        // Create a batch write item request
        var batchItemRequest = new BatchWriteItemRequest
        {
            RequestItems = new Dictionary<string, List<WriteRequest>>
            {
                { _custTableName, writeRequests }
            }
        };

        // Execute the batch write item request asynchronously
        /*
            * Batch operations in DynamoDB allow you to put or delete multiple items across multiple tables in a single request.
                - Each operation within a batch can succeed or fail independently of the others.
                - Batch operations are mainly used for efficiency and reducing the number of network round trips when dealing with multiple items or tables.
                - Batch operations do not support transactional guarantees, meaning if one operation fails within the batch, 
                    the others may still succeed.
         */
        var response = await _dynamoDB.BatchWriteItemAsync(batchItemRequest, cancellationToken);

        // Return true if the batch operation was successful
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    void BatchPutRequests(IEnumerable<CustomerDto>? customersToCreate, List<WriteRequest> writeRequests)
    {
        if (customersToCreate != null)
        {
            foreach (var customer in customersToCreate)
            {
                writeRequests.Add(new WriteRequest
                {
                    PutRequest = new PutRequest
                    {
                        Item = Document.FromJson(JsonSerializer.Serialize(customer)).ToAttributeMap(),
                    }
                });
            }
        }
    }

    void BatchDeleteRequests(IEnumerable<CustomerDto>? customersToDelete, List<WriteRequest> writeRequests)
    {
        if (customersToDelete != null)
        {
            foreach (var customer in customersToDelete)
            {
                writeRequests.Add(new WriteRequest
                {
                    DeleteRequest = new DeleteRequest
                    {
                        Key = new Dictionary<string, AttributeValue>
                        {
                            {"pk", new AttributeValue { S = customer.Pk }},
                            {"sk", new AttributeValue { S = customer.Sk }}
                        }
                    }
                });
            }
        }
    }
    #endregion


    #region Transaction
    public async Task<bool> CreateOrderAsync(CustomerDto customer, OrderDto order, CancellationToken cancellationToken)
    {
        // Create a request to perform a transactional write operation
        var transactionWriteRequest = new TransactWriteItemsRequest
        {
            // Define the list of transactional items to be performed
            TransactItems = new List<TransactWriteItem>
        {
            // Add a transactional item to put customer data into the customer table
            new TransactWriteItem
            {
                Put = new Put
                {
                    TableName = _custTableName,
                    // Serialize the customer object to JSON and convert it to DynamoDB Document format
                    Item = Document.FromJson(JsonSerializer.Serialize(customer)).ToAttributeMap(),
                    // Optionally, you can specify a condition for this write operation
                    // ConditionExpression = ""
                }
            },
            // Add a transactional item to put order data into the order table
            new TransactWriteItem
            {
                Put = new Put
                {
                    TableName  = _orderTableName,
                    // Serialize the order object to JSON and convert it to DynamoDB Document format
                    Item  = Document.FromJson(JsonSerializer.Serialize(order)).ToAttributeMap()
                }
            }
        }
        };

        // Execute the transactional write operation asynchronously
        var response = await _dynamoDB.TransactWriteItemsAsync(transactionWriteRequest, cancellationToken);

        // Return true if the batch operation was successful
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    #endregion
}