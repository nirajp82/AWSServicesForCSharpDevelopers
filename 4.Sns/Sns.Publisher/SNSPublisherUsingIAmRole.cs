using System.Text.Json;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace SNS.Publisher
{
    internal class SNSPublisherUsingIAmRole
    {
        public static async Task PublishAsync(CancellationToken cancellationToken)
        {
            try
            {
                var snsConfig = new AmazonSimpleNotificationServiceConfig
                {
                    //  RetryMode = Amazon.Runtime.RequestRetryMode.Legacy,
                    //   MaxErrorRetry = 4,
                    //   Timeout = TimeSpan.FromMicroseconds(1000),
                };

                AmazonSimpleNotificationServiceClient snsClient = new(snsConfig);
                var topicResponse = await snsClient.FindTopicAsync("customers");
                PublishRequest publishRequest = new PublishRequest
                {
                    TopicArn = topicResponse.TopicArn,
                    Message = JsonSerializer.Serialize(Customer.Create("IAMRole")),
                    MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {
                        "MessageType", new MessageAttributeValue
                        {
                            DataType = "String",
                            StringValue = "CustomerCreated"
                        }
                    }
                }
                };
                var response = snsClient.PublishAsync(publishRequest, cancellationToken);
                Console.WriteLine(JsonSerializer.Serialize(response));
            }
            catch (AmazonSimpleNotificationServiceException ex)
            {
                await Console.Out.WriteLineAsync(ex.ToString());
            }
            catch (Exception e)
            {
                await Console.Out.WriteLineAsync(e.ToString());
            }
        }
    }
}
