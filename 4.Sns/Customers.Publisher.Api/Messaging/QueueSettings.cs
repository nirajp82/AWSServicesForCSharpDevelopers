namespace Customers.Publisher.Api.Messaging;

public class QueueSettings
{
    public const string Key = "Queue";
    
    public required string Name { get; set; }
}
