using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretsManager.ConApp;

internal class SecretsManagerUtil
{
    // Define the name of the secret and the region
    const string SECRET_NAME = "NP_SECRET";
    const string REGION = "us-east-1";

    // Method to execute various operations related to AWS Secrets Manager
    internal static async Task ExecuteSecretManagerOperationsAsync()
    {
        // Create an instance of the AWS Secrets Manager client
        IAmazonSecretsManager secretsManagerClient = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(REGION));

        // Delete the secret
        //await DeleteSecretAsync(secretsManagerClient);
        // Create a new secret
        await CreateSecretAsync(secretsManagerClient);

        // Update the secret
        await UpdateSecretAsync(secretsManagerClient);

        // Describe the secret
        await DescribeSecretAsync(secretsManagerClient);

        // Retrieve previous and current secrets
        await GetPreviousAndCurrentSecretsAsync(secretsManagerClient);

        // Retrieve all secrets
        await GetAllSecretsAsync(secretsManagerClient);

        // Retrieve a secret by version ID
        await GetSecretByVersionIdAsync(secretsManagerClient);

        // Delete the secret
        await DeleteSecretAsync(secretsManagerClient);
    }

    static async Task CreateSecretAsync(IAmazonSecretsManager secretsManagerClient)
    {
        var createRequest = new CreateSecretRequest
        {
            Name = SECRET_NAME,
            SecretString = "{\"username\":\"admin\", \"password\":\"supersecret\"}",
            Description = "Create secret using .net."
        };

        var response = await secretsManagerClient.CreateSecretAsync(createRequest);
        Console.WriteLine($"Secret created successfully. {response.ARN}");
    }

    static async Task UpdateSecretAsync(IAmazonSecretsManager secretsManagerClient)
    {
        var updateRequest = new UpdateSecretRequest
        {
            SecretId = SECRET_NAME,
            SecretString = "{\"username\":\"admin\", \"password\":\"newsupersecret\"}",
            Description = "Update secret using .net."
        };
        var response = await secretsManagerClient.UpdateSecretAsync(updateRequest);
        Console.WriteLine($"Secret updated successfully. {response.ARN}");
    }

    static async Task DescribeSecretAsync(IAmazonSecretsManager secretsManagerClient)
    {
        var describeRequest = new DescribeSecretRequest
        {
            SecretId = SECRET_NAME,
        };
        var describeResponse = await secretsManagerClient.DescribeSecretAsync(describeRequest);
        Console.WriteLine($"Description: {describeResponse.Description}");
    }

    static async Task GetPreviousAndCurrentSecretsAsync(IAmazonSecretsManager secretsManagerClient)
    {
        var getPreviousRequest = new GetSecretValueRequest
        {
            SecretId = SECRET_NAME,
            VersionStage = "AWSPREVIOUS"
        };
        var previousResponse = await secretsManagerClient.GetSecretValueAsync(getPreviousRequest);
        Console.WriteLine($"Previous Secret: {previousResponse.SecretString}");

        var getCurrentRequest = new GetSecretValueRequest
        {
            SecretId = SECRET_NAME,
            VersionStage = "AWSCURRENT"
        };
        var currentResponse = await secretsManagerClient.GetSecretValueAsync(getCurrentRequest);
        Console.WriteLine($"Current Secret: {currentResponse.SecretString}");
    }

    static async Task GetAllSecretsAsync(IAmazonSecretsManager secretsManagerClient)
    {
        var allSecretsRequest = new ListSecretsRequest() { MaxResults = 10 };
        var allSecretsResponse = await secretsManagerClient.ListSecretsAsync(allSecretsRequest);
        foreach (var secret in allSecretsResponse.SecretList)
        {
            Console.WriteLine($"GetAllSecretsAsync: Secret: {secret.Name}, Stages: {string.Join(',', secret.SecretVersionsToStages.Keys)}");
        }
    }

    static async Task GetSecretByVersionIdAsync(IAmazonSecretsManager secretsManagerClient)
    {
        var describeSecretResponse = await secretsManagerClient.DescribeSecretAsync(new DescribeSecretRequest { SecretId = SECRET_NAME });
        var getVersionRequest = new GetSecretValueRequest
        {
            SecretId = SECRET_NAME,
            VersionId = describeSecretResponse.VersionIdsToStages.Keys.First(),
        };
        var getVersionResponse = await secretsManagerClient.GetSecretValueAsync(getVersionRequest);
        Console.WriteLine($"Secret by VersionId: {getVersionResponse.SecretString}");
    }

    static async Task DeleteSecretAsync(IAmazonSecretsManager secretsManagerClient)
    {
        var deleteRequest = new DeleteSecretRequest
        {
            SecretId = SECRET_NAME,
            //delete the secret without any recovery window
            ForceDeleteWithoutRecovery = true,
        };
        var deleteResponse = await secretsManagerClient.DeleteSecretAsync(deleteRequest);
        Console.WriteLine($"Secret deleted successfully. {deleteResponse.DeletionDate}");
    }

    /*
     
//Secret deleted successfully. 2/17/2024 5:40:17 PM
Secret created successfully. arn:aws:secretsmanager:us-east-1:584940157141:secret:NP_SECRET-O3EnUv
Secret updated successfully. arn:aws:secretsmanager:us-east-1:584940157141:secret:NP_SECRET-O3EnUv
Description: Update secret using .net.
Previous Secret: {"username":"admin", "password":"supersecret"}
Current Secret: {"username":"admin", "password":"newsupersecret"}
GetAllSecretsAsync: Secret: NP_SECRET, Stages: b8a090f8-6f3a-4415-b542-5cc3d4f04b19,c29d2214-cfee-4d2a-8fc6-e036e1dc307a
Secret by VersionId: {"username":"admin", "password":"newsupersecret"}
Secret deleted successfully. 2/17/2024 5:40:32 PM

     */

}
