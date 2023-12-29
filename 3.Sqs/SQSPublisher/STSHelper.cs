using Amazon.Runtime;
using Amazon.SecurityToken.Model;
using Amazon.SecurityToken;

namespace SQSPublisher
{
    internal class STSHelper
    {
        internal static async Task<AWSCredentials> GetSTSTempCredentialsAsync(CancellationToken cts)
        {
            // Create an instance of AmazonSecurityTokenServiceClient using the default credentials provider chain.
            // Note: The SDK will use the default credentials provider chain for STSClient as credentials are not passed explicitly.
            using var stsClient = new AmazonSecurityTokenServiceClient();
            //Get details about the IAM user or role whose credentials are used to make a call
            var callerIdentity = await stsClient.GetCallerIdentityAsync(new GetCallerIdentityRequest(), cts);
            var accountId = callerIdentity.Account;

            // Define the IAM role name assumed for SQS operations
            var roleName = "SQSSendMessageRole";

            var assumeRoleRequest = new AssumeRoleRequest
            {
                //ARN of the role that needs to be assume.
                RoleArn = $"arn:aws:iam::{accountId}:role/{roleName}",
                RoleSessionName = "SQSConsumerUsingSTS"
            };

            //Get of temporary security credentials that we can use to access Amazon Resources.
            AssumeRoleResponse assumeRoleResponse = await stsClient.AssumeRoleAsync(assumeRoleRequest, cts);

            // The temporary security credentials, which include an access key ID, a secret access key, and a security (or session) token.
            return assumeRoleResponse.Credentials;
        }
    }
}
