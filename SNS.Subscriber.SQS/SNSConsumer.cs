using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNS.Subscriber.SQS
{
    internal class SNSConsumer
    {
        public static async Task ConsumeAsync(string queueName, CancellationToken cts)
        {
            using var sqsClient = new AmazonSQSClient();

            // Retrieve the URL of the specified queue
            var queueUrlResponse = await sqsClient.GetQueueUrlAsync(queueName);

            // Create a request to receive messages from the queue
            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = queueUrlResponse.QueueUrl,
                //A list of attributes that need to be returned along with each message. All – Returns all the attributes.
                AttributeNames = ["All"],
                //The name of the message attribute. -  All – Returns all the attributes.
                MessageAttributeNames = ["All"],
                MaxNumberOfMessages = 3,
            };

            // Continue processing messages until a cancellation is requested
            while (!cts.IsCancellationRequested)
            {
                // Receive messages from the queue
                var response = await sqsClient.ReceiveMessageAsync(receiveMessageRequest, cts);

                Console.WriteLine($"Total received messages: {response.Messages.Count}, QueueName:{queueName}");

                foreach (var message in response.Messages)
                {
                    Console.WriteLine($"Id: {message.MessageId}, Body: {message.Body}, " +
                        $"Attributes:{string.Join(',', message.Attributes?.Select(e => $"{e.Key}: {e.Value}") ?? new[] { "N/A" })}, " +
                        $"MessageAttributes:{string.Join(',', message.MessageAttributes?.Select(e => $"{e.Key}: {e.Value}") ?? new[] { "N/A" })}");

                    Console.WriteLine("");

                    // Delete the processed message from the queue
                    await sqsClient.DeleteMessageAsync(queueUrlResponse.QueueUrl, message.ReceiptHandle, cts);
                }

                await Task.Delay(3000, cts);
            }
        }
    }
}
