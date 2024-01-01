using Amazon.SQS.Model;

namespace Customer.Publisher.Api.Messaging
{
    public interface ISqsMessenger
    {
        Task<SendMessageResponse> SendMessageAsync<T>(T message, CancellationToken cts);
    }
}