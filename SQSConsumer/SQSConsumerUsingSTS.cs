using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQSConsumer
{
    internal class SQSConsumerUsingSTS
    {
        public static async Task ConsumeAsync(CancellationToken cts)
        {
            var queueName = "customers";
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

                Console.WriteLine($"Total received messages using STS: {response.Messages.Count}");

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

        private static async Task<AWSCredentials> GetSTSCredentialAsync(CancellationToken cts)
        {
            // Create an instance of AmazonSecurityTokenServiceClient using the default credentials provider chain.
            // Note: The SDK will use the default credentials provider chain for STSClient as credentials are not passed explicitly.
            using var stsClient = new AmazonSecurityTokenServiceClient();
            //Get details about the IAM user or role whose credentials are used to make a call
            var callerIdentity = await stsClient.GetCallerIdentityAsync(new GetCallerIdentityRequest(), cts);
            var accountId = callerIdentity.Account;

            // Define the IAM role name assumed for SQS operations
            var roleName = "SQSSendMessageRole";

            var assumeRoleRequest = new AssumeRoleRequest
            {
                //ARN of the role that needs to be assume.
                RoleArn = $"arn:aws:iam::{accountId}:role/{roleName}",
                RoleSessionName = "SQSConsumerUsingSTS"
            };

            //Get of temporary security credentials that we can use to access Amazon Resources.
            AssumeRoleResponse assumeRoleResponse = await stsClient.AssumeRoleAsync(assumeRoleRequest, cts);

            // The temporary security credentials, which include an access key ID, a secret access key, and a security (or session) token.
            return assumeRoleResponse.Credentials;
        }
    }
}
