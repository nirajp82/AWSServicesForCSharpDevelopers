```cs
//************Part 1 - Assume proxy account role that has permission to assume cross account role*****************
// Creating credentials object using Access Key and Secret Key
var accessSecretKeyCred = new BasicAWSCredentials(_AccessKey, _SecretKey);

/*
The `FallbackCredentialsFactory.GetCredentials()` method attempts to retrieve AWS credentials using a fallback mechanism. It typically searches for credentials in the following order:

1. Environment variables
2. AWS credentials profile file (usually located at `~/.aws/credentials`)
3. EC2 instance profile credentials (if the code is running on an EC2 instance with an associated IAM role)

So, when `FallbackCredentialsFactory.GetCredentials()` is called, it will attempt to find AWS credentials following this fallback sequence and return the credentials it finds.
*/
// var fallbackCred = FallbackCredentialsFactory.GetCredentials();

// Request to assume the first role
var assumeRole1Req = new AssumeRoleRequest
{
    RoleArn = _AssumeRole1Arn,
    RoleSessionName = "AssumeProxyRoleSession",
    DurationSeconds = 3600
};

//************Part 2 - Use proxy credential to assume cross account role*****************
//Use security token service client to assume the proxy role that has permission to assume in role in cross account. 
Credentials proxyCred;
using (var stsClient = new AmazonSecurityTokenServiceClient(accessSecretKeyCred, new AmazonSecurityTokenServiceConfig() { RegionEndpoint = RegionEndpoint.GetBySystemName("us-east-1") }))
{
    var pubsubRoleResponse = stsClient.AssumeRole(assumeRole1Req);
    proxyCred = pubsubRoleResponse.Credentials;
}

// Creating session credentials object using the obtained temporary credentials
SessionAWSCredentials proxySessionCred = new SessionAWSCredentials(proxyCred.AccessKeyId, proxyCred.SecretAccessKey, proxyCred.SessionToken);

//Use ProxyCredentials To Assume External Role (Cross Account Role)
Credentials crossActCred = null;
// Creating SNS client using the obtained cross-account credentials
using (var stsCrossClient = new AmazonSecurityTokenServiceClient(proxySessionCred, new AmazonSecurityTokenServiceConfig() { RegionEndpoint = RegionEndpoint.GetBySystemName("us-east-1") }))
{
    var request = new AssumeRoleRequest()
    {
        RoleArn = _CrossAcctRoleArn,
        RoleSessionName = "AssumeCrossRoleSession",
        ExternalId = _ExternalId,
        DurationSeconds = 3600
    };
    var crossActAssumedRole = stsCrossClient.AssumeRole(request);
    crossActCred = crossActAssumedRole.Credentials;
}


//************Part 3 - Use cross account credential to assume cross account role*****************
// Creating SNS client using the obtained cross-account credentials
var snsClient = new AmazonSimpleNotificationServiceClient(crossActCred);
// Creating publish request to send message to SNS topic
PublishRequest publishRequest = new PublishRequest
{
    TopicArn = _CrossAcctTopicArn,
    Message = GetMessageBody()
};
// Sending publish request using SNS client
var response = snsClient.Publish(publishRequest);
```
