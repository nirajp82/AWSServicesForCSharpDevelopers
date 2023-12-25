# Table of Contents

1. [IAM Roles vs. AWS Security Token Service (STS)](#iam-roles-vs-aws-security-token-service-sts)
2. [IAM Roles](#iam-roles)
   - [Long-Lived Credentials](#long-lived-credentials)
   - [Role-Based Access Control](#role-based-access-control)
   - [Permanent Association](#permanent-association)
3. [AWS Security Token Service (STS)](#aws-security-token-service-sts)
   - [Temporary Credentials](#temporary-credentials)
   - [Dynamic Access](#dynamic-access)
   - [Cross-Account Access](#cross-account-access)
4. [When to Use Which?](#when-to-use-which)
   - [Use IAM Roles](#use-iam-roles)
   - [Use STS](#use-sts)
5. [Use IAM roles for applications and AWS services which require Amazon SQS access](#use-iam-roles-for-applications-and-aws-services-which-require-amazon-sqs-access)
6. [IAM Role Usage Guide](#iam-role-usage-guide)
   - [IAM Role Overview](#iam-role-overview)
   - [How IAM Roles Work](#how-iam-roles-work)
   - [Usage in Non-EC2 Instances (Local Development)](#usage-in-non-ec2-instances-local-development)
   - [Usage in EC2 Instances](#usage-in-ec2-instances)
   - [Security Best Practices](#security-best-practices)

# IAM Roles vs. AWS Security Token Service (STS)

## IAM Roles

IAM roles in AWS are like documents that define who (such as your application or an AWS service) can perform what actions (a list of API calls) under specific conditions (optional conditions specific to the service). These roles are managed by the AWS Identity and Access Management (IAM) service.

### Long-Lived Credentials

IAM roles provide long-lived credentials, meaning they have a continuous lifespan.

### Role-Based Access Control

IAM roles define permissions for entities, specifying the actions they can perform on AWS services.

### Permanent Association

IAM roles can be permanently associated with entities like EC2 instances or Lambda functions, and AWS SDKs can seamlessly pick up these roles when used within AWS.

## AWS Security Token Service (STS)

AWS Security Token Service (STS) is an AWS service designed to provide temporary, limited-privilege credentials.

### Temporary Credentials

STS issues temporary credentials that have an expiration time. These credentials are obtained by requesting the AssumeRole API to assume an IAM role.

### Dynamic Access

STS allows dynamic access to AWS resources, enabling entities to assume roles and obtain temporary credentials. It is particularly useful when dynamic access is required.

### Cross-Account Access

STS facilitates cross-account access, enabling IAM roles in one AWS account to be assumed by principals in another account.

## When to Use Which?

The decision between IAM roles and STS depends on your specific use case and requirements. Here's a general guideline:

### Use IAM Roles

- If your application is running on AWS (EC2, Elastic Beanstalk, Lambda, etc.), consider attaching IAM roles directly to instances. AWS SDKs can seamlessly pick up these roles.
- Exception: Use STS directly when performing actions in a completely different AWS account.

### Use STS

- If your application is running outside of AWS, consider using STS to obtain temporary credentials.

In summary, IAM roles are suitable for long-lived entities within AWS, while STS is recommended for obtaining temporary credentials, especially for applications running outside of the AWS environment. Always consider the specific needs of your application and follow security best practices.

# Use IAM roles for applications and AWS services which require Amazon SQS access

For applications or AWS services such as Amazon EC2 to access Amazon SQS queues, they must use valid AWS credentials in their AWS API requests. Because these credentials aren't rotated automatically, you shouldn't store AWS credentials directly in the application or EC2 instance.

You should use an IAM role to manage temporary credentials for applications or services that need to access Amazon SQS. When you use a role, you don't have to distribute long-term credentials (such as a username, password, and access keys) to an EC2 instance or AWS service such as AWS Lambda. Instead, the role supplies temporary permissions that applications can use when they make calls to other AWS resources.

# IAM Role Usage Guide

This guide provides instructions on how to use IAM roles for managing AWS credentials in your project. IAM roles are useful for providing secure and temporary access to AWS services without embedding long-term credentials directly in your code.

## IAM Role Overview

IAM roles are AWS Identity and Access Management (IAM) entities that define a set of permissions for making AWS service requests. Roles are not associated with a specific user or group but can be assumed by users, applications, or AWS services.

## How IAM Roles Work

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
   - How to generate AWS Access Key ID, Secret Access Key:
     - If you already have an existing IAM user and need to generate new AWS Access Key ID and Secret Access Key, you can follow these steps:
       - Sign in to the AWS Management Console.
       - Navigate to the IAM Console.
       - Access IAM Users and select the existing user.
       - Under the "Security credentials" tab, generate a new access key.
       - Download the new credentials and update your applications.
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