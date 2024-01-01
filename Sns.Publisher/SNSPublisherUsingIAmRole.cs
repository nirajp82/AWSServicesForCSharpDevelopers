using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace SNS.Publisher
{
    internal class SNSPublisherUsingIAmRole
    {
        public static async Task PublishAsync(CancellationToken cancellationToken)
        {
            AmazonSimpleNotificationServiceClient snsClient = new();
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
    }
}
