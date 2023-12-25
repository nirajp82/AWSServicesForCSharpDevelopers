# Use IAM roles for applications and AWS services which require Amazon SQS access

For applications or AWS services such as Amazon EC2 to access Amazon SQS queues, they must use valid AWS credentials in their AWS API requests. Because these credentials aren't rotated automatically, you shouldn't store AWS credentials directly in the application or EC2 instance.

You should use an IAM role to manage temporary credentials for applications or services that need to access Amazon SQS. When you use a role, you don't have to distribute long-term credentials (such as a username, password, and access keys) to an EC2 instance or AWS service such as AWS Lambda. Instead, the role supplies temporary permissions that applications can use when they make calls to other AWS resources.

# IAM Role Usage Guide

This guide provides instructions on how to use IAM roles for managing AWS credentials in your project. IAM roles are useful for providing secure and temporary access to AWS services without embedding long-term credentials directly in your code.

## Table of Contents
- [IAM Role Overview](#iam-role-overview)
- [Usage in Non-EC2 Instances (Local Development)](#usage-in-non-ec2-instances-local-development)
- [Usage in EC2 Instances](#usage-in-ec2-instances)

## IAM Role Overview

IAM roles are AWS Identity and Access Management (IAM) entities that define a set of permissions for making AWS service requests. Roles are not associated with a specific user or group but can be assumed by users, applications, or AWS services.

### How IAM Roles Work
1. **Create IAM Role:** Create an IAM role in the AWS Management Console with the necessary permissions for your application or service.

2. **Assign Role:** Assign the IAM role to the entities that need temporary credentials, such as EC2 instances or AWS Lambda functions.

3. **Temporary Credentials:** IAM roles provide temporary security credentials that applications or services can use to sign requests to AWS services.

## Usage in Non-EC2 Instances (Local Development)

When working on a local development machine, you can use the AWS CLI to configure credentials.

1. **Install AWS CLI:** If not installed, [install the AWS CLI](https://aws.amazon.com/cli/).

2. **Configure AWS CLI:**
   ```bash
   aws configure
   ```
   This command prompts you to enter your AWS Access Key ID, Secret Access Key, default region, and output format.

   *. How to generate AWS Access Key ID, Secret Access Key:
   If you already have an existing IAM user and need to generate new AWS Access Key ID and Secret Access Key, you can follow these steps:
		*. Sign in to the AWS Management Console.
		*. Navigate to the IAM Console.
		*. Access IAM Users and select the existing user.
		*. Under the "Security credentials" tab, generate a new access key.
		*. Download the new credentials and update your applications.

3. **Run Your Application:** Your application will automatically use the configured credentials.

## Usage in EC2 Instances

When running your application on an EC2 instance, IAM roles can be associated with the instance, and your code will automatically pick up the credentials.

1. **Create IAM Role for EC2:**
   - In the AWS Management Console, navigate to IAM.
   - Create an IAM role with the necessary permissions (e.g., `AmazonSNSFullAccess`).

2. **Assign IAM Role to EC2 Instance:**
   - In the EC2 Dashboard, select your instance.
   - Under the "Actions" menu, navigate to "Security," and then "Modify IAM Role."
   - Assign the IAM role you created.

3. **Run Your Application on EC2:** Your application running on the EC2 instance will automatically use the IAM role's credentials.

## Security Best Practices

- Regularly review and update IAM policies to follow the principle of least privilege.
- Avoid hardcoding AWS credentials in your code.
- Use IAM roles for EC2 instances to automatically manage temporary credentials.

**Note:** Ensure that your IAM roles have the necessary permissions for the specific AWS services your application uses.
