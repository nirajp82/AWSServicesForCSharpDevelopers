# Setting up a Resource-Based Policy between AWS Accounts for Amazon S3 Access

## Table of Contents
1. [Configure the S3 Bucket in Account A](#step-1-configure-the-s3-bucket-in-account-a)
2. [Add a Resource-Based Policy to the S3 Bucket in Account A](#step-2-add-a-resource-based-policy-to-the-s3-bucket-in-account-a)
3. [Configure Access from Account B](#step-3-configure-access-from-account-b)
4. [Access the S3 Bucket from Account B](#step-4-access-the-s3-bucket-from-account-b)
5. [Sample code](#Sample-code)

---

## Step 1: Configure the S3 Bucket in Account A
1. Log in to the AWS Management Console using the credentials for Account A.
2. Navigate to the Amazon S3 service.
3. Create a new S3 bucket or select an existing bucket that you want to share with Account B.
4. Select the bucket, and then click on the "Permissions" tab.

## Step 2: Add a Resource-Based Policy to the S3 Bucket in Account A
1. In the "Permissions" tab, scroll down to the "Bucket policy" section and click on "Edit".
2. Add a policy document that grants permissions to Account B to access the bucket. Here's an example policy document:

```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Principal": {
                "AWS": "arn:aws:iam::<Account B ID>:root"
            },
            "Action": [
                "s3:GetObject",
                "s3:PutObject"
            ],
            "Resource": "arn:aws:s3:::<Your Bucket Name>/*"
        }
    ]
}
```
Replace `<Account B ID>` with the AWS account ID of Account B, and `<Your Bucket Name>` with the name of your S3 bucket.

3. Review the policy and click on "Save changes" to apply the resource-based policy to the S3 bucket.

## Step 3: Configure Access from Account B
1. **Log in to the AWS Management Console for Account B**:
   - Open a web browser and navigate to the AWS Management Console using the credentials associated with Account B.
2. **Navigate to the IAM Service**:
   - In the AWS Management Console, search for and select the "IAM" service to access the IAM dashboard.
3. **Create an IAM Role**:
   - From the IAM dashboard, select "Roles" from the left-hand navigation pane.
   - Click on the "Create role" button to start creating a new IAM role.
4. **Choose the Type of Trusted Entity**:
   - For the "Select type of trusted entity" step, choose "Another AWS account" as the trusted entity.
   - Enter the AWS account ID of Account A in the "Account ID" field. This is the account that owns the S3 bucket.
5. **Set Permissions for the Role**:
   - Next, attach a policy to the role that grants the necessary permissions for accessing the S3 bucket in Account A. Example policy is provided in Step 2.
6. **Review and Add Tags (Optional)**:
   - Review the role configuration to ensure it aligns with your requirements.
   - Optionally, you can add tags to the role for better organization and management.
7. **Name and Create the Role**:
   - Provide a name and optionally a description for the IAM role.
   - Click on the "Create role" button to create the IAM role.
8. **Define Trust Relationship**:
   - After creating the role, you will be redirected to the role summary page.
   - Click on the "Trust relationships" tab.
   - Review the trust relationship, which specifies that Account B is allowed to assume the role in Account A.
9. **Copy Role ARN**:
   - Once the role is created, copy the Amazon Resource Name (ARN) of the IAM role. You will need this ARN when configuring permissions in Account A.

## Step 4: Access the S3 Bucket from Account B
1. Use the IAM role created in Account B to assume the role in Account A that allows access to the S3 bucket.
2. Once the role is assumed, users, applications, or services in Account B can access the S3 bucket in Account A based on the permissions granted by the resource-based policy.

## Step 5: Sample code
If the code is running on a VM outside of AWS and does not have programmatic access to Account A, but it does have programmatic access to Account B, you can use the AWS Security Token Service (STS) to assume a role in Account B that has permissions to access resources in Account A via the resource-based policy.

Here's how you can accomplish this:

1. **Assume a Role in Account B**:
   - The code running on the VM needs to call the AssumeRole API to assume a role in Account B that has permission to access the S3 bucket in Account A.

2. **Retrieve Temporary Credentials**:
   - After successfully assuming the role in Account B, the AssumeRole API returns temporary security credentials.

3. **Use Temporary Credentials to Access S3 in Account A**:
   - With the temporary credentials obtained from assuming the role in Account B, the code can create an S3 client and access the S3 bucket in Account A as per the permissions granted by the resource-based policy.

Here's a general outline of the steps:

```csharp
using Amazon;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Amazon.S3;
using Amazon.S3.Model;

class Program
{
    static void Main(string[] args)
    {
        // Set up the AWS credentials for programmatic access to Account B
        var credentials = new BasicAWSCredentials("AccessKeyForAccountB", "SecretKeyForAccountB");

        // Create an STS client with the credentials for Account B
        var stsClient = new AmazonSecurityTokenServiceClient(credentials, RegionEndpoint.USWest2); // Change the region accordingly

        // Specify the ARN of the IAM role in Account B that allows access to Account A's S3 bucket
        string roleToAssumeArn = "arn:aws:iam::AccountB-ID:role/RoleName"; // Change this to the ARN of your IAM role in Account B

        // Create the request to assume the role in Account B
        var assumeRoleRequest = new AssumeRoleRequest
        {
            RoleArn = roleToAssumeArn,
            RoleSessionName = "AssumedRoleSession"
        };

        // Assume the role in Account B
        var assumeRoleResponse = stsClient.AssumeRoleAsync(assumeRoleRequest).GetAwaiter().GetResult();

        // Retrieve the temporary credentials from the AssumeRole response
        var temporaryCredentials = assumeRoleResponse.Credentials;

        // Create an S3 client with the temporary credentials
        var s3Client = new AmazonS3Client(temporaryCredentials, RegionEndpoint.USWest2); // Change the region accordingly

        // Specify the bucket name and object key in Account A
        string bucketName = "AccountA-S3-Bucket";
        string keyName = "your-object-key";

        // Retrieve the object from the S3 bucket in Account A
        var getObjectRequest = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = keyName
        };

        try
        {
            // Get the object using the temporary credentials
            using (var response = s3Client.GetObjectAsync(getObjectRequest).GetAwaiter().GetResult())
            {
                // Process the object response
                using (var responseStream = response.ResponseStream)
                {
                    using (var reader = new System.IO.StreamReader(responseStream))
                    {
                        string content = reader.ReadToEnd();
                        Console.WriteLine("Content of the object:");
                        Console.WriteLine(content);
                    }
                }
            }
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"Error accessing S3 object: {ex.Message}");
        }
    }
}
```

Make sure to replace the placeholders `AccessKeyForAccountB`, `SecretKeyForAccountB`, `AccountB-ID`, `RoleName`, `AccountA-S3-Bucket`, and `your-object-key` with the actual values corresponding to your AWS environment.

This approach allows the code running on the VM to use the programmatic access it has to Account B to assume a role in Account B that has permission to access the S3 bucket in Account A through the resource-based policy.
