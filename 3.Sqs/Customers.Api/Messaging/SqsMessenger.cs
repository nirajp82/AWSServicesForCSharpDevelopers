using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Customers.Api.Messaging
{
    public class SqsMessenger : ISqsMessenger
    {
        private readonly IAmazonSQS _amazonSQS;
        private readonly IOptions<QueueSettings> _queueSettings;
        private string? _queueUrl;

        public SqsMessenger(IAmazonSQS amazonSQS, IOptions<QueueSettings> queueSettings)
        {
            _amazonSQS = amazonSQS;
            _queueSettings = queueSettings;
        }

        public async Task<SendMessageResponse> SendMessageAsync<T>(T message, CancellationToken cts)
        {
            // Get the URL of the specified queue
            var queueUrl = await GetQueueUrlAsync(cts);

            // Build the request to send the message
            var sendMessageRequest = new SendMessageRequest()
            {
                QueueUrl = queueUrl,
                MessageBody = JsonSerializer.Serialize(message),
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

            // Send the message to the SQS queue
            var response = await _amazonSQS.SendMessageAsync(sendMessageRequest, cts);
            return response;
        }

        private async Task<string> GetQueueUrlAsync(CancellationToken cts)
        {
            if (string.IsNullOrWhiteSpace(_queueUrl))
            {
                var queueUrlResponse = await _amazonSQS.GetQueueUrlAsync(_queueSettings.Value.Name, cts);
                _queueUrl = queueUrlResponse.QueueUrl;
            }
            return _queueUrl;
        }
    }
}