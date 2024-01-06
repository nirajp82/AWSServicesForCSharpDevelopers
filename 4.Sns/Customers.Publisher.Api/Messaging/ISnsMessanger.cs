using Amazon.SimpleNotificationService.Model;

namespace Customers.Publisher.Api.Messaging
{
    public interface ISnsMessanger
    {
        Task<PublishResponse> PublishAsync<T>(T message, CancellationToken cancellationToken);
    }
}
