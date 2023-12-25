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
        internal static async Task Publish()
        {
            try
            {
                AWSCredentials tempCredentials = await STSHelper.GetSTSTempCredentials();
                using (var sqsClient = new AmazonSQSClient(tempCredentials))
                {
                    await SQSHelper.Publish(sqsClient);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing message: {ex.Message}");
            }
        }
    }
}
