using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Customers.Publisher.Api.Messaging
{
    public class SnsMessanger : ISnsMessanger
    {
        private readonly TopicSettings _topicSettings;
        private readonly IAmazonSimpleNotificationService _snsService;
        private string? _topicArn;

        public SnsMessanger(IOptions<TopicSettings> topicOpsions, IAmazonSimpleNotificationService amazonSimpleNotificationService)
        {
            _topicSettings = topicOpsions.Value;
            _snsService = amazonSimpleNotificationService;
        }

        public async Task<PublishResponse> PublishAsync<T>(T message, CancellationToken cancellationToken)
        {
            var topicArn = await GetTopicArnAsync();
            var publishRequest = new PublishRequest
            {
                TopicArn = topicArn,
                Message = JsonSerializer.Serialize(message),
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {
                        "MessageType", new MessageAttributeValue
                        {
                            DataType = "String",
                            StringValue = typeof(T).Name
                        }
                    }
                }
            };
            var response = await _snsService.PublishAsync(publishRequest, cancellationToken);
            return response;
        }

        public async ValueTask<string> GetTopicArnAsync()
        {
            if (string.IsNullOrWhiteSpace(_topicArn))
            {
                var response = await _snsService.FindTopicAsync(_topicSettings.Name);
                _topicArn = response.TopicArn;
            }
            return _topicArn;
        }
    }
}
