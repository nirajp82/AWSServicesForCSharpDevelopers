using Amazon.SQS;
using Amazon.SQS.Model;
using SQSPublisher;
using System.Text.Json;

await SendMessageUsingCLIConfiguredCredentialsAsync();

async Task SendMessageUsingCLIConfiguredCredentialsAsync()
{
    using (var sqsClient = new AmazonSQSClient())
    {
        string queueName = "customers";
        // Get the URL of the specified queue
        var queueUrlResponse = await sqsClient.GetQueueUrlAsync(queueName);

        // Build the request to send the message
        var sendMessageRequest = new SendMessageRequest
        {
            QueueUrl = queueUrlResponse.QueueUrl,
            MessageBody = JsonSerializer.Serialize(Customer.Create())
        };

        try
        {
            // Send the message to the SQS queue
            var response = await sqsClient.SendMessageAsync(sendMessageRequest);
            Console.WriteLine($"Message sent with ID: {response.MessageId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error publishing message: {ex.Message}");
        }
    }
}