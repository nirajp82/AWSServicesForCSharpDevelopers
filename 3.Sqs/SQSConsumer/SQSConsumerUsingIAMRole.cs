﻿using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SQSConsumer
{
    internal class SQSConsumerUsingIAMRole
    {
        public static async Task ConsumeAsync(CancellationToken cts)
        {
            using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(3000));

            // Specify the name of the Amazon Simple Queue Service (SQS) queue
            string queueName = "customers";

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
            while (!cts.IsCancellationRequested && await timer.WaitForNextTickAsync(cts))
            {
                // Receive messages from the queue
                var response = await sqsClient.ReceiveMessageAsync(receiveMessageRequest, cts);

                Console.WriteLine($"Total received messages using IAMRole: {response.Messages.Count}");

                foreach (var message in response.Messages)
                {
                    Console.WriteLine($"Id: {message.MessageId}, Body: {message.Body}, " +
                        $"Attributes:{string.Join(',', message.Attributes?.Select(e => $"{e.Key}: {e.Value}") ?? new[] { "N/A" })}, " +
                        $"MessageAttributes:{string.Join(',', message.MessageAttributes?.Select(e => $"{e.Key}: {e.Value}") ?? new[] { "N/A" })}");

                    Console.WriteLine("");

                    // Delete the processed message from the queue
                    await sqsClient.DeleteMessageAsync(queueUrlResponse.QueueUrl, message.ReceiptHandle, cts);
                }
            }
        }
    }
}
