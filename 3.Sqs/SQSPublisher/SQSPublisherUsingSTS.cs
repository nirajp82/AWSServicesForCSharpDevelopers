using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Net.Mail;
using System.Net;
using System.Text.Json;

namespace SQSPublisher
{
    internal class SQSPublisherUsingSTS
    {
        internal static async Task PublishAsync(CancellationToken cts)
        {
            try
            {
                AWSCredentials tempCredentials = await STSHelper.GetSTSTempCredentialsAsync(cts);
                using var sqsClient = new AmazonSQSClient(tempCredentials);
                await SQSHelper.PublishAsync(sqsClient, "STS", cts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing message: {ex.Message}");
            }
        }
    }
}
