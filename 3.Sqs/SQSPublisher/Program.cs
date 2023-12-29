using Amazon.SQS;
using Amazon.SQS.Model;
using SQSPublisher;

var cts = new CancellationTokenSource();

var tsk1 = SQSPublisherUsingIAMRole.PublishAsync(cts.Token);

// Create a linked token source, This is just for demo, No real need of creating Linked Token
var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token);
var tsk2 = SQSPublisherUsingSTS.PublishAsync(linkedCts.Token);

await Task.WhenAll(tsk1, tsk2);