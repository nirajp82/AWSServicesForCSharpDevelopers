using Amazon.SQS.Model;
using Amazon.SQS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SQSPublisher
{
    internal class SQSHelper
    {
        internal static async Task PublishAsync(AmazonSQSClient sqsClient, string source, CancellationToken cts)
        {
            string queueName = "customers";
            // Get the URL of the specified queue
            var queueUrlResponse = await sqsClient.GetQueueUrlAsync(queueName, cts);

            // Build the request to send the message
            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = queueUrlResponse.QueueUrl,
                MessageBody = JsonSerializer.Serialize(Customer.Create(source)),
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {
                        "MessageType", new MessageAttributeValue
                        {
                            DataType = "String",
                            StringValue = nameof(Customer)
                        }
                    }
                }
            };

            try
            {
                // Send the message to the SQS queue
                var response = await sqsClient.SendMessageAsync(sendMessageRequest, cts);
                Console.WriteLine($"Message sent with ID: {response.MessageId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing message: {ex.Message}");
            }
        }
    }
}
