using Amazon.SQS.Model;

namespace Customers.Publisher.Api.Messaging;

public interface ISqsMessenger
{
    Task<SendMessageResponse> SendMessageAsync<T>(T message);
}
