# **Run .exe on EC2 Using AWS Systems Manager (SSM) from ASP.NET Web Application**

This guide provides a detailed, step-by-step explanation of how to run a `.exe` file on an EC2 instance using AWS Systems Manager (SSM) and trigger the execution from an ASP.NET web application.

The goal is to:

* Set up SSM on the EC2 instance.
* Create a custom SSM document that runs a PowerShell script to invoke the `.exe`.
* Create an SSM association that links the EC2 instance to the document.
* Trigger the execution of the SSM document from an ASP.NET web application.

---

## **Table of Contents**

1. [Prerequisites](#1-prerequisites)
2. [Step 1: Set Up AWS Systems Manager (SSM) on EC2 Instances](#2-set-up-aws-systems-manager-ssm-on-ec2-instances)

   * IAM Role with Least Privilege Access
   * Install and Verify SSM Agent
3. [Step 2: Create SSM Document to Run PowerShell Script](#3-create-ssm-document-to-run-powershell-script)

   * Create Document
   * Example PowerShell Script in Document
4. [Step 3: Create and Use SSM Association](#4-create-and-use-ssm-association)

   * Create Association
   * Best Practices
5. [Step 4: Trigger the SSM Association from an ASP.NET Web Application](#5-trigger-the-ssm-association-from-an-aspnet-web-application)

   * Install AWS SDK for .NET
   * C# Code to Trigger SSM Association
   * Invoke from ASP.NET Controller
6. [Step 5: Monitoring and Logging Execution](#6-monitoring-and-logging-execution)

   * Enable CloudWatch Logs
   * Best Practices for Monitoring
7. [Step 6: Security Best Practices](#7-security-best-practices)

   * IAM Role Policies
   * Encryption for Sensitive Data
   * CloudTrail for Audit Logging
8. [Step 7: Enterprise-Wide Best Practices](#8-enterprise-wide-best-practices)

   * Use Tags for Scalability
   * Automate with Infrastructure as Code (IaC)
   * Scaling the Architecture
9. [Conclusion](#9-conclusion)

---

## **1. Prerequisites**

Before you begin, make sure you have:

* An **EC2 instance** running with Windows or Linux as the OS.
* A **.NET Web Application** hosted in your preferred environment (e.g., AWS, on-prem).
* **AWS IAM Role** with `AmazonSSMManagedInstanceCore` permissions for EC2.
* **AWS SDK for .NET** installed in your .NET application.
* **AWS Systems Manager (SSM)** properly set up and configured in your AWS account.

---

## **2. Set Up AWS Systems Manager (SSM) on EC2 Instances**

### **IAM Role with Least Privilege Access**

1. **Create IAM Role**:

   * Go to **IAM** > **Roles** > **Create Role**.
   * Select **AWS Service** > **EC2**.
   * Attach the `AmazonSSMManagedInstanceCore` managed policy.
   * If the EC2 needs additional permissions, such as for accessing S3, attach those policies.

2. **Attach Role to EC2**:

   * Assign the newly created IAM role to your EC2 instance.

### **Install and Verify SSM Agent**

* **Windows EC2**: The SSM agent should already be installed by default. Verify:

  ```bash
  Get-Service AmazonSSMAgent
  ```
  ![image](https://github.com/user-attachments/assets/2770205f-054c-46e8-b624-d3649eb9502d)


* **Linux EC2**: Install the SSM agent manually (if not installed):

  ```bash
  sudo yum install -y amazon-ssm-agent
  sudo systemctl enable amazon-ssm-agent
  sudo systemctl start amazon-ssm-agent
  ```

* **Verify**: In the **SSM Console**, ensure your EC2 instance is listed under **Managed Instances**.

---

## **3. Create SSM Document to Run PowerShell Script**

### **Create Document**

1. Go to **Systems Manager** > **Documents** > **Create Document**.
2. Choose **Command Document** > **Custom**.
3. Enter a descriptive name (e.g., `RunExeFromEC2`).
4. Choose **JSON** as the content type.

### **Example PowerShell Script in Document**

```json
{
  "schemaVersion": "2.2",
  "description": "Run .exe via PowerShell script",
  "mainSteps": [
    {
      "action": "aws:runPowerShellScript",
      "name": "runPowerShellScript",
      "inputs": {
        "runCommand": [
          "$exePath = '{{ exePath }}'",
          "$param1 = '{{ param1 }}'",
          "$param2 = '{{ param2 }}'",
          "Start-Process -FilePath $exePath -ArgumentList $param1, $param2"
        ]
      }
    }
  ]
}
```

### **Explanation**:

* The document contains a **runPowerShellScript** action.
* Parameters `exePath`, `param1`, and `param2` are placeholders for dynamic values that will be provided when the document is executed.

---

## **4. Create and Use SSM Association**

### **Create Association**

1. Go to **Systems Manager** > **State Manager** > **Create Association**.
2. Choose the document you created (e.g., `RunExeFromEC2`).
3. Specify the **targets** (EC2 instances). You can target instances by ID or tags.
4. **Parameters**: Provide values for the parameters (`exePath`, `param1`, `param2`) that will be used during execution.
5. **Schedule**: You can specify a cron schedule for recurring executions or leave it blank for on-demand execution.

### **Best Practices**:

* **Dynamic Targeting**: Use **tags** to target instances dynamically. Example: Target instances with a `Role:AppServer` tag.
* **Error Handling**: Implement error handling in your PowerShell script, such as logging errors to CloudWatch.

---

## **5. Trigger the SSM Association from an ASP.NET Web Application**

### **Install AWS SDK for .NET**

1. Use NuGet Package Manager:

   ```bash
   Install-Package AWSSDK.SimpleSystemsManagement
   ```

### **C# Code to Trigger SSM Association**

```csharp
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SsmService
{
    private readonly IAmazonSimpleSystemsManagement _ssmClient;

    public SsmService()
    {
        _ssmClient = new AmazonSimpleSystemsManagementClient();
    }

    public async Task CreateAssociationAndRunExeAsync(string instanceId, string exePath, string param1, string param2)
    {
        var request = new CreateAssociationRequest
        {
            Name = "RunExeFromEC2",  // Your SSM document name
            Targets = new List<AssociationTarget>
            {
                new AssociationTarget
                {
                    Key = "instanceIds",
                    Values = new List<string> { instanceId }
                }
            },
            Parameters = new Dictionary<string, List<string>>
            {
                { "exePath", new List<string> { exePath } },
                { "param1", new List<string> { param1 } },
                { "param2", new List<string> { param2 } }
            }
        };

        try
        {
            var response = await _ssmClient.CreateAssociationAsync(request);
            Console.WriteLine("Association created: " + response.AssociationDescription.AssociationId);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error creating association: " + ex.Message);
        }
    }
}
```

### **Invoke from ASP.NET Controller**

```csharp
public class MyController : Controller
{
    private readonly SsmService _ssmService;

    public MyController()
    {
        _ssmService = new SsmService();
    }

    public async Task<IActionResult> RunExe()
    {
        string instanceId = "i-0123456789abcdef0"; // Your EC2 instance ID
        string exePath = @"C:\path\to\your\app.exe";
        string param1 = "param1_value";
        string param2 = "param2_value";

        await _ssmService.CreateAssociationAndRunExeAsync(instanceId, exePath, param1, param2);
        return View();
    }
}
```

---

## **6. Monitoring and Logging Execution**

### **Enable CloudWatch Logs**

Ensure that your SSM document includes a **CloudWatch log group** for easy monitoring and troubleshooting.

### **Best Practices for Monitoring**:

* Set up **CloudWatch Alarms** to notify you of failures or performance issues.
* Use **AWS CloudTrail** to track every action and maintain an audit trail for compliance purposes.
* Integrate **Amazon SNS** to send notifications for successful or failed executions.

---

## **7. Security Best Practices**

### **IAM Role Policies**

* **Use least privilege principles**: Ensure the EC2 instance role has only the permissions needed to execute the specific SSM document.

### **Encryption for Sensitive Data**

* Use **AWS KMS** to encrypt sensitive parameters (such as passwords or private keys) passed into the SSM document.

### **CloudTrail for Audit Logging**

* Enable **AWS CloudTrail** to log every action and event. This helps with compliance and security auditing.

---

## **8. Enterprise-Wide Best Practices**

### **Use Tags for Scalability**

* Dynamically target EC2 instances using tags like `Role:AppServer` or `Environment:Production` instead of hardcoding instance IDs.

### **Automate with Infrastructure as Code (IaC)**

* Use **CloudFormation** or **Terraform** to automate the creation and management of EC2 instances, IAM roles, SSM documents, and associations.

### **Scaling the Architecture**

* **Auto Scaling Groups**: Automatically scale EC2 instances, ensuring that the right IAM roles and SSM agents are applied as instances are added or removed.

---

## **9. Conclusion**

This guide demonstrates how to run a `.exe` on an EC2 instance using AWS Systems Manager (SSM) from an ASP.NET web application. Key best practices for enterprise applications have been incorporated, such as security, scalability, and maintainability.

---

### **Key Takeaways**:

* **Security**: Use IAM roles with least privilege, encryption for sensitive data, and CloudTrail for auditing.
* **Scalability**: Use tags for dynamic targeting and automate infrastructure with IaC tools like CloudFormation or Terraform.
* **Monitoring**: Leverage CloudWatch for logging and set up alerts for real-time monitoring.
