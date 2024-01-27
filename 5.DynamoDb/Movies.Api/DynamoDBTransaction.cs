using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using System.Text.Json;

namespace Movies.Api
{
    public class DynamoDBBatchWriteExample
    {
        private readonly IAmazonDynamoDB _dynamoDBClient;

        public DynamoDBBatchWriteExample(IAmazonDynamoDB dynamoDBClient)
        {
            _dynamoDBClient = dynamoDBClient;
        }

        // Method to perform batch write operations on DynamoDB
        public async Task BatchWriteOperationsAsync()
        {
            try
            {
                // Create instances of items to be written, updated, or deleted
                var movieYearTitleToAdd = new MovieYearTitle
                {
                    Id = Guid.NewGuid(),
                    Title = "New Movie Title",
                    ReleaseYear = 2024,
                    AgeRestriction = 16,
                    RottenTomatoesPercentage = 80
                };

                var movieYearTitleToUpdate = new MovieYearTitle
                {
                    // Assuming you have the ID of the item you want to update
                    Id = Guid.Parse("existing-movie-id"),
                    Title = "Updated Movie Title",
                    ReleaseYear = 2022,
                    AgeRestriction = 12,
                    RottenTomatoesPercentage = 90
                };

                var movieTitleScoreToDelete = new MovieTitleScore
                {
                    // Assuming you have the ID of the item you want to delete
                    Id = Guid.Parse("existing-movie-score-id"),
                    Title = "Movie Title to Delete",
                    ReleaseYear = 2021,
                    AgeRestriction = 18,
                    RottenTomatoesPercentage = 85
                };

                // Create a BatchWriteItemRequest specifying the operations
                var batchWriteRequest = new BatchWriteItemRequest
                {
                    RequestItems = new Dictionary<string, List<WriteRequest>>
                {
                    {
                        "YourDynamoDBTableName",
                        new List<WriteRequest>
                        {
                            // Add operation: PutRequest to add a new item
                            new WriteRequest
                            {
                                PutRequest = new PutRequest
                                {
                                    Item = Document.FromJson( JsonSerializer.Serialize(movieYearTitleToAdd)).ToAttributeMap()
                                }
                            },
                            // Update operation: PutRequest to update an existing item
                            new WriteRequest
                            {
                                PutRequest = new PutRequest
                                {
                                    Item = Document.FromJson(JsonSerializer.Serialize(movieYearTitleToUpdate)).ToAttributeMap()
                                }
                            },
                            // Delete operation: DeleteRequest to remove an existing item
                            new WriteRequest
                            {
                                DeleteRequest = new DeleteRequest
                                {
                                    Key = new Dictionary<string, AttributeValue>
                                    {
                                        {"pk", new AttributeValue {S = movieTitleScoreToDelete.Title }},
                                        {"sk", new AttributeValue {S = movieTitleScoreToDelete.RottenTomatoesPercentage.ToString() }}
                                    }
                                }
                            }
                        }
                    }
                }
                };

                // Execute the BatchWriteItem operation
                var response = await _dynamoDBClient.BatchWriteItemAsync(batchWriteRequest);

                // Handle response if necessary
                Console.WriteLine("Batch write operation completed.");
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error performing batch write operations: {ex.Message}");
            }
        }
    }
}