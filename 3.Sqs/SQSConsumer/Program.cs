using SQSConsumer;

var cts = new CancellationTokenSource();
var tsk1 = SQSConsumerUsingIAMRole.ConsumeAsync(cts.Token);
var tsk2 = SQSConsumerUsingSTS.ConsumeAsync(cts.Token);

await Task.WhenAll(tsk1, tsk2);