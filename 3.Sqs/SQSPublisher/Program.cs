using Amazon.SQS;
using Amazon.SQS.Model;
using SQSPublisher;

await SQSPublisherUsingIAMRole.Publish();
await SQSPublisherUsingSTS.Publish();
