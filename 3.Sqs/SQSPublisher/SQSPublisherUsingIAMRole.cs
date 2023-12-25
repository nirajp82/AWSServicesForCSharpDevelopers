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
    internal class SQSPublisherUsingIAMRole
    {
        /// <summary>
        /// When you create an instance of AmazonSQSClient without passing in explicit credentials, 
        /// the SDK will attempt to use the default credentials provider chain, which includes environment variables,
        /// shared credentials file, and IAM roles associated with an EC2 instance (if applicable).
        /// </summary>
        /// <returns></returns>
        internal static async Task Publish()
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
    }
}
