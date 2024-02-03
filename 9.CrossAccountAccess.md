# Cross-Account Communication Setup Guide

This guide outlines the steps to enable cross-account communication between an EC2 instance in Account "B" and an Amazon Kinesis Data Firehose in Account "A". By following these instructions, you'll establish secure access using IAM roles and policies.

## Table of Contents
1. [Create an IAM Role in Account "A"](##Create-IAM-Role-in-Account-A)
2. [Update Trust Relationship for the Role](##Update-Trust-Relationship-for-the-Role)
3. [Retrieve the Role ARN](##Retrieve-the-Role-ARN)
4. [Create an IAM Policy in Account "B"](##Create-IAM-Policy-in-Account-B)
5. [Attach the IAM Policy to an IAM User/Role in Account "B"](##Attach-IAM-Policy-to-IAM-User/Role-in-Account-B)
6. [Implement STS to Assume Role in Code](##Implement-STS-to-Assume-Role-in-Code)
7. [Sample C# code](##Sample-C#-code)


## 1. Create an IAM Role in Account "A"
- Go to the IAM console in Account "A".
- Click on "Roles" in the left-hand navigation pane.
- Select "Create role".
- Choose "Another AWS account" as the trusted entity.
- Enter the Account ID of Account "B".
- Enable the option to Require external ID.
- Enter the External ID, which is a unique identifier agreed upon between Account "A" and Account "B".
- Attach the "AmazonKinesisFirehoseFullAccess" policy (or a custom policy with necessary permissions) to this role.

## 2. Update Trust Relationship for the Role
- After creating the role, select the role you just created.
- Under the "Trust relationships" tab, click "Edit trust relationship".
- Update the trust policy document to include Account "B" as a trusted entity along with the External ID. The trust policy should resemble the provided JSON structure.
```json
    {
      "Version": "2012-10-17",
      "Statement": [
        {
          "Effect": "Allow",
          "AWS": [
              "arn:aws:iam::ACCOUNT_B_ID:role/ROLE_NAME_IN_B",
              "arn:aws:iam::ACCOUNT_C_ID:user/USERNAME_IN_C"
          ],
          "Action": "sts:AssumeRole",
          "Condition": {
            "StringEquals": {
              "sts:ExternalId": "YOUR_EXTERNAL_ID"
            }
          }
        }
      ]
    }
```
The JSON policy you provided is an IAM policy that grants permission to both an IAM role in Account B and an IAM user in Account C to assume a specified IAM role, subject to a condition defined by the External ID. Let's break down its components:

- **Version**: This specifies the version of the policy language being used. In this case, it's "2012-10-17", which indicates the date format of the policy syntax.

- **Statement**: This is an array of statements that define the permissions granted or denied by the policy.

  - **Effect**: This specifies whether the statement allows or denies access. In this policy, "Allow" indicates that the action is permitted.

  - **AWS**: This specifies the AWS entities (principals) to which the permission applies. In this policy, it includes both the ARN of an IAM role in Account B (`arn:aws:iam::ACCOUNT_B_ID:role/ROLE_NAME_IN_B`) and the ARN of an IAM user in Account C (`arn:aws:iam::ACCOUNT_C_ID:user/USERNAME_IN_C`).

  - **Action**: This specifies the AWS service actions that the policy allows or denies. In this case, it allows the "sts:AssumeRole" action, which is used to request temporary security credentials to assume the specified role.

  - **Condition**: This specifies conditions under which the permission is granted. In this policy, it states that the "sts:ExternalId" must be equal to a specific value (`"YOUR_EXTERNAL_ID"`). This condition helps ensure that only requests originating from trusted sources (with the correct External ID) are allowed to assume the role.

In summary, the policy allows both the specified IAM role in Account B and the specified IAM user in Account C to assume a designated IAM role, subject to the condition that the External ID matches the expected value. This configuration is commonly used in scenarios where cross-account access is required with additional security measures, such as External ID validation.
Replace "ACCOUNT_B_ID" with the actual Account "B" ID, "ROLE_NAME_IN_B" with the name of the IAM role in Account "B", and "YOUR_EXTERNAL_ID" with the agreed-upon external ID.
## 3. Retrieve the Role ARN
- Once the role is created, make a note of the Role ARN. You will need it in Account "B".

## 4. Create an IAM Policy in Account "B"
- Proceed to the IAM console in Account "B".
- Click on "Policies" in the left-hand navigation pane.
- Select "Create policy".
- Choose the JSON tab and enter a policy document similar to the provided JSON structure.
```json
    {
      "Version": "2012-10-17",
      "Statement": [
        {
          "Effect": "Allow",
          "Action": "sts:AssumeRole",
          "Resource": "arn:aws:iam::ACCOUNT_A_ID:role/ROLE_NAME"
        }
      ]
    }
```
- **Version**: This specifies the version of the policy language being used. In this case, it's "2012-10-17", which indicates the date format of the policy syntax.

- **Statement**: This is an array of statements that define the permissions granted or denied by the policy.

  - **Effect**: This specifies whether the statement allows or denies access. In this policy, "Allow" indicates that the action is permitted.

  - **Action**: This specifies the AWS service actions that the policy allows or denies. In this case, it allows the "sts:AssumeRole" action, which is used to request temporary security credentials to assume the specified role.

  - **Resource**: This specifies the AWS resource or resources to which the action applies. In this policy, it specifies the ARN (Amazon Resource Name) of the IAM role in Account A that can be assumed. The ARN format includes the AWS account ID and the role name.

The policy allows entities with this policy attached (such as IAM users, groups, or roles) to assume the IAM role specified in Account A using the "sts:AssumeRole" action. This is commonly used in cross-account access scenarios where one account needs to delegate permissions to another account or service.

Replace "ACCOUNT_A_ID" with the actual Account "A" ID and "ROLE_NAME" with the name of the role created in Account "A".

## 5. Attach the IAM Policy to an IAM User/Role in Account "B"
### 5.1. Attach the IAM Policy to an IAM User in Account "B"
- Go to the IAM console in Account "B".
- Click on "Users" in the left-hand navigation pane if you want to attach the policy to an existing user, or click on "Add user" to create a new IAM user if needed.
- Select the IAM user to which you want to attach the policy, or create a new user and make note of the user's username.
- Under the "Permissions" tab, click on "Add permissions".
- Choose "Attach existing policies directly".
- Search for the IAM policy you created earlier (the one allowing AssumeRole) and select it.
- Click "Next: Review" and then "Add permissions" to attach the policy to the IAM user.

### 5.2. Attach the IAM Policy to an IAM Role in Account "B"
- Go to the IAM console in Account "B".
- Click on "Roles" in the left-hand navigation pane if you want to attach the policy to an existing role, or click on "Create role" to create a new IAM role if needed.
- Select the IAM role to which you want to attach the policy, or create a new role and make note of the role's name.
- Under the "Permissions" tab, click on "Add inline policies" or "Attach policies".
- Choose "Attach policies" if you want to attach an existing policy.
- Search for the IAM policy you created earlier (the one allowing AssumeRole) and select it.
- Click "Next: Review" and then "Add permissions" to attach the policy to the IAM role.

## 6. Implement STS to Assume Role in Code
- Within your EC2 instance in Account "B", utilize the AWS SDK to assume the IAM role created in Account "A" using STS.
- After assuming the role, you will receive temporary credentials to make requests to Amazon Kinesis Data Firehose.

Ensure to replace placeholders like ACCOUNT_A_ID, ACCOUNT_B_ID, ROLE_NAME_IN_B, and YOUR_EXTERNAL_ID with actual values specific to your AWS accounts and resources.

## 7. Sample C# code
Here's a sample C# code using the AWS SDK for .NET that runs on an EC2 instance in Account "B" to publish a message to Amazon Kinesis Data Firehose in Account "A":

```csharp
using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Amazon.KinesisFirehose;
using Amazon.KinesisFirehose.Model;

class Program
{
    static async Task Main(string[] args)
    {
        // Specify the AWS region where the Firehose is located
        var region = RegionEndpoint.USWest2; // Change this to your desired region

        // Replace these placeholders with your actual values
        string roleArn = "arn:aws:iam::ACCOUNT_A_ID:role/ROLE_NAME"; // ARN of the IAM role in Account "A"
        string externalId = "YOUR_EXTERNAL_ID"; // External ID for cross-account access
        string firehoseDeliveryStreamName = "YOUR_FIREHOSE_DELIVERY_STREAM_NAME"; // Name of the Firehose delivery stream

        // Create an STS client using the default credentials chain
        var stsClient = new AmazonSecurityTokenServiceClient(region);

        // Assume the IAM role in Account "A" to obtain temporary credentials
        var assumeRoleRequest = new AssumeRoleRequest
        {
            RoleArn = roleArn,
            ExternalId = externalId,
            RoleSessionName = "CrossAccountSession"
        };

        var assumeRoleResponse = await stsClient.AssumeRoleAsync(assumeRoleRequest);

        // Use the temporary credentials to create an Amazon Kinesis Firehose client
        var credentials = assumeRoleResponse.Credentials;
        var firehoseClient = new AmazonKinesisFirehoseClient(
            new BasicAWSCredentials(credentials.AccessKeyId, credentials.SecretAccessKey, credentials.SessionToken), 
            region);

        // Message to be sent to the Firehose delivery stream
        string message = "Hello from EC2 instance in Account B!";

        // Create a PutRecord request to send data to the Firehose delivery stream
        var putRecordRequest = new PutRecordRequest
        {
            DeliveryStreamName = firehoseDeliveryStreamName,
            Record = new Record { Data = System.Text.Encoding.UTF8.GetBytes(message) }
        };

        try
        {
            // Send the message to the Firehose delivery stream
            await firehoseClient.PutRecordAsync(putRecordRequest);
            Console.WriteLine("Message sent successfully to Amazon Kinesis Data Firehose.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message to Amazon Kinesis Data Firehose: {ex.Message}");
        }
    }
}
```

This code assumes that you have an IAM role in Account "A" with the necessary permissions to put records into the Amazon Kinesis Data Firehose delivery stream. Additionally, it assumes that you have set up the trust relationship and the necessary IAM policies between Account "A" and Account "B" as outlined in the previous steps.

Replace placeholders like `ACCOUNT_A_ID`, `ROLE_NAME`, `YOUR_EXTERNAL_ID`, and `YOUR_FIREHOSE_DELIVERY_STREAM_NAME` with actual values specific to your AWS accounts and resources.
