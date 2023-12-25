using Amazon.Runtime;
using Amazon.SecurityToken.Model;
using Amazon.SecurityToken;

namespace SQSPublisher
{
    internal class STSHelper
    {
        internal static async Task<AWSCredentials> GetSTSTempCredentials()
        {
            AWSCredentials tempCredentials;
            // Create an instance of AmazonSecurityTokenServiceClient using the default credentials provider chain.
            // Note: The SDK will use the default credentials provider chain for STSClient as credentials are not passed explicitly.
            using (var stsClient = new AmazonSecurityTokenServiceClient())
            {
                // Get the AWS account ID dynamically for constructing the IAM role ARN
                var accountIdResponse = await stsClient.GetCallerIdentityAsync(new GetCallerIdentityRequest());
                var accountId = accountIdResponse.Account;

                // Define the IAM role name assumed for SQS operations
                var roleName = "SQSSendMessageRole";

                // Assume the IAM role with necessary permissions for SQS operations
                var assumeRoleResponse = await stsClient.AssumeRoleAsync(new AssumeRoleRequest
                {
                    RoleArn = $"arn:aws:iam::{accountId}:role/{roleName}",
                    RoleSessionName = "SQSPublisherUsingSTS"
                });

                // Use temporary credentials to create an instance of AmazonSQSClient
                tempCredentials = assumeRoleResponse.Credentials;
            }

            return tempCredentials;
        }
    }
}
