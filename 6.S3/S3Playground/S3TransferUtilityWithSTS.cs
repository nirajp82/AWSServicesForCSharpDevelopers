using Amazon.S3.Transfer;
using Amazon.S3;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;

namespace S3Playground
{
    internal class S3TransferUtilityWithSTS
    {
        const string bucketName = "npawstraining";
        const string fileName = "movies.csv";

        public async Task DownloadAsync()
        {
            var credentials = await GetSTSCredential();
            IAmazonS3 s3Client = new AmazonS3Client(credentials);
            using var transferUtility = new TransferUtility(s3Client);
            try
            {
                await transferUtility.DownloadAsync(fileName, bucketName, fileName);
                Console.WriteLine("File downloaded successfully.");
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error uploading file: {0}", e.Message);
            }
        }

        private async Task<AWSCredentials> GetSTSCredential()
        {
            // Create a client for Security Token Service (STS) operations
            using var stsClient = new AmazonSecurityTokenServiceClient();

            try
            {
                // Get the AWS account ID of the current caller
                var callerIdentity = await stsClient.GetCallerIdentityAsync(new GetCallerIdentityRequest());
                var accountId = callerIdentity.Account;

                // Specify the role to be assumed
                var roleName = "S3Role";

                // Create the request to assume the role
                var assumeRoleRequest = new AssumeRoleRequest
                {
                    RoleArn = $"arn:aws:iam::{accountId}:role/{roleName}",
                    RoleSessionName = "S3RoleSession"
                };

                // Assume the role and obtain temporary credentials
                var assumeRoleResponse = await stsClient.AssumeRoleAsync(assumeRoleRequest, CancellationToken.None);

                // Return the acquired STS credentials
                return assumeRoleResponse.Credentials;
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately, e.g., log errors and potentially rethrow
                Console.Error.WriteLine("Error assuming role: {0}", ex.Message);
                throw;
            }
        }
    }
}
