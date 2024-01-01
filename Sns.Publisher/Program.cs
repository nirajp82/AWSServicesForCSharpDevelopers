using SNS.Publisher;

var cancelToken = new CancellationTokenSource();
await SNSPublisherUsingIAmRole.PublishAsync(cancelToken.Token);