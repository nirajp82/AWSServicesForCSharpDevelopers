using SNS.Subscriber.SQS;

var cts = new CancellationTokenSource();

// Specify the name of the Amazon Simple Queue Service (SQS) queue
var queueName = "customers";

if (args?.Length > 0)
    queueName = args[0];

await SNSConsumer.ConsumeAsync(queueName, cts.Token);