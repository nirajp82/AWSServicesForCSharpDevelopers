using Amazon.SQS;
using Amazon.SQS.Model;
using Customer.Consumer.Api.Contracts.Messages;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Customer.Consumer.Api
{
    /// <summary>
    /// BackgroundService, runs long-running IHostedService tasks with a single method — ExecuteAsync method. 
    /// IHostedService is not used here because IHostedService handles short-running tasks using the StartAsync and StopAsync methods.
    /// </summary>
    public class QueueConsumerService : BackgroundService
    {
        private readonly IAmazonSQS _amazonSQS;
        private readonly QueueSettings _queueSettings;
        private readonly IMediator _mediator;
        private readonly ILogger<QueueConsumerService> _logger;

        public QueueConsumerService(IAmazonSQS amazonSQS, IMediator mediator, ILogger<QueueConsumerService> logger, IOptions<QueueSettings> queueSettings)
        {
            _amazonSQS = amazonSQS;
            _mediator = mediator;
            _logger = logger;
            _queueSettings = queueSettings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Retrieve the URL of the specified queue
                var queueUrlResponse = await _amazonSQS.GetQueueUrlAsync(_queueSettings.Name, stoppingToken);

                ReceiveMessageRequest receiveMessageRequest = CreateMessageRequest(queueUrlResponse);

                // Receive messages from the queue
                var response = await _amazonSQS.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);

                Console.WriteLine($"Total received messages: {response.Messages.Count}");

                foreach (var message in response.Messages)
                {
                    string messageType = message.MessageAttributes["MessageType"].StringValue;
                    if (string.IsNullOrEmpty(messageType))
                    {
                        _logger.LogWarning($"Unknown message type: {messageType}");
                        continue;
                    }

                    Type? type = Type.GetType(typeName: $"Customer.Consumer.Api.Contracts.Messages.{messageType}");
                    if (type is null)
                    {
                        _logger.LogWarning($"Unknown message type: {type}");
                    }
                    else
                    {
                        ISqsMessage sqsMessage = (JsonSerializer.Deserialize(message.Body, type) as ISqsMessage)!;
                        try
                        {
                            await _mediator.Send(sqsMessage, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Message failed during processing.");
                            continue;
                        }
                        await _amazonSQS.DeleteMessageAsync(queueUrlResponse.QueueUrl, message.ReceiptHandle, stoppingToken);
                    }
                }
                await Task.Delay(3000, stoppingToken);
            }
        }

        private static ReceiveMessageRequest CreateMessageRequest(GetQueueUrlResponse queueUrlResponse)
        {
            // Create a request to receive messages from the queue
            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = queueUrlResponse.QueueUrl,
                //A list of attributes that need to be returned along with each message. All – Returns all the attributes.
                AttributeNames = ["All"],
                //The name of the message attribute. -  All – Returns all the attributes.
                MessageAttributeNames = ["All"],
                //The maximum number of messages to return. Default value is 1.
                MaxNumberOfMessages = 3,
            };
            return receiveMessageRequest;
        }
    }
}
