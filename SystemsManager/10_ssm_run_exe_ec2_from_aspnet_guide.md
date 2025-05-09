Here's an updated and expanded version of the **guide** you asked for. It now includes:

* Complete **C# code explanation**, especially for `CreateAssociationAsync`.
* How to **set default parameters** in the SSM document so they are optional.
* Line-by-line **code comments** to make everything easier to understand.

---

# **📘 Run `.exe` on EC2 Using AWS Systems Manager (SSM) from ASP.NET Web Application**

## Goal:

Enable a `.NET` web application to trigger the execution of a `.exe` file on an EC2 instance **without SSH or RDP**, using **AWS Systems Manager (SSM)**.

---

## **✅ Key Flow**

1. Prepare the EC2 instance with SSM.
2. Create a **custom SSM document** with **default parameters**.
3. Use SSM Association to run PowerShell that triggers `.exe`.
4. Call `CreateAssociationAsync` from ASP.NET to invoke the command.
5. Monitor execution via CloudWatch.

---

## **1. Prerequisites**

* Windows EC2 instance.
* `.exe` file already present on the EC2.
* SSM agent installed and running.
* IAM role with `AmazonSSMManagedInstanceCore` attached to EC2.
* AWS SDK for .NET installed in your ASP.NET application.

---

## **2. Set Up EC2 with SSM**

### 🛡️ IAM Role

Attach an IAM role with **AmazonSSMManagedInstanceCore** to your EC2 instance:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "ssm:*",
        "ec2messages:*",
        "cloudwatch:*"
      ],
      "Resource": "*"
    }
  ]
}
```

### 🔧 SSM Agent

* **Windows**: Check agent status:

```powershell
Get-Service AmazonSSMAgent
```

* **Linux**:

```bash
sudo yum install -y amazon-ssm-agent
sudo systemctl start amazon-ssm-agent
```

Confirm instance appears under **SSM > Managed Instances**.

---

## **3. Create a Custom SSM Document with Default Parameters**

### 📄 JSON Definition

Go to **SSM > Documents > Create Document**, choose **Custom**, and paste this JSON:

```json
{
  "schemaVersion": "2.2",
  "description": "Run .exe via PowerShell script with optional parameters",
  "parameters": {
    "exePath": {
      "type": "String",
      "description": "Path to the executable",
      "default": "C:\\Program Files\\MyApp\\app.exe"
    },
    "param1": {
      "type": "String",
      "description": "First parameter",
      "default": "default1"
    },
    "param2": {
      "type": "String",
      "description": "Second parameter",
      "default": "default2"
    }
  },
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

### 🧠 What This Document Does

* Accepts 3 parameters (`exePath`, `param1`, `param2`) with **default values**.
* Runs a **PowerShell** command on the target EC2 instance.
* Starts the specified `.exe` with optional arguments.

If parameters are **not passed**, the **defaults** will be used automatically.

---

## **4. Trigger from ASP.NET Using AWS SDK**

### 📦 Install AWS SDK

Install via NuGet:

```bash
Install-Package AWSSDK.SimpleSystemsManagement
```

---

### 🧩 C# Service Class

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
        // Initialize SSM client with default credentials and region
        _ssmClient = new AmazonSimpleSystemsManagementClient();
    }

    /// <summary>
    /// Triggers the SSM document to run a .exe file on a specific EC2 instance.
    /// </summary>
    public async Task CreateAssociationAndRunExeAsync(
        string instanceId,
        string exePath = null,
        string param1 = null,
        string param2 = null)
    {
        var request = new CreateAssociationRequest
        {
            Name = "RunExeFromEC2", // The name of your custom SSM document

            Targets = new List<AssociationTarget>
            {
                new AssociationTarget
                {
                    Key = "InstanceIds", // This means you're targeting a specific EC2 instance
                    Values = new List<string> { instanceId }
                }
            },

            // Parameters to pass to the document — only override defaults if needed
            Parameters = new Dictionary<string, List<string>>()
        };

        if (exePath != null)
            request.Parameters["exePath"] = new List<string> { exePath };
        if (param1 != null)
            request.Parameters["param1"] = new List<string> { param1 };
        if (param2 != null)
            request.Parameters["param2"] = new List<string> { param2 };

        try
        {
            // This triggers execution of the SSM document on the target instance
            var response = await _ssmClient.CreateAssociationAsync(request);

            Console.WriteLine("✅ Association created:");
            Console.WriteLine($"AssociationId: {response.AssociationDescription.AssociationId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error running SSM association: " + ex.Message);
        }
    }
}
```

---

### 🎮 Controller Example (ASP.NET MVC)

```csharp
public class RunController : Controller
{
    private readonly SsmService _ssmService = new SsmService();

    public async Task<IActionResult> Trigger()
    {
        string instanceId = "i-0123456789abcdef0"; // Replace with your EC2 ID

        // These are optional. If you omit them, the SSM document uses default values.
        string exePath = null;
        string param1 = null;
        string param2 = null;

        await _ssmService.CreateAssociationAndRunExeAsync(instanceId, exePath, param1, param2);

        return Content("Execution triggered.");
    }
}
```

---

## 🔍 What Does `CreateAssociationAsync` Do?

* It's an **AWS SDK method** that creates a **one-time or scheduled execution link** (called an *Association*) between:

  * An EC2 instance (target)
  * A Systems Manager Document (command template)
* AWS then **runs the command on your behalf**, logs the result, and you can track the run in SSM Console or CloudWatch.

**It's the trigger that makes your EC2 "do something" like run your `.exe`**, securely and without needing RDP.

---

## **5. Monitoring and Logging**

* Add logging to CloudWatch Logs from within your PowerShell script.
* You can also use:

  * `DescribeAssociationExecutions`
  * `DescribeAssociationExecutionTargets`
  * CloudTrail + CloudWatch for auditing and alerting.

---

## **6. Security Best Practices**

* **Use IAM roles** with least privilege (no `*:*` in real apps).
* Use **KMS encryption** if you're passing sensitive parameters.
* Track everything with **CloudTrail**.

---

## **7. Scaling and Automation**

* Use **tags** instead of hardcoding instance IDs (e.g., `Environment:Production`).
* Automate everything using **Terraform**, **CloudFormation**, or **SSM Automation documents**.
* Combine with **Auto Scaling** groups and **EventBridge** for high scale automation.

---

## ✅ Final Thoughts

This guide enables you to build a secure, auditable, and highly scalable mechanism to remotely execute binaries on EC2 instances via a simple web interface. Everything is built around AWS SSM — no SSH or custom agents needed.

---

Would you like me to generate a downloadable PDF version or add an end-to-end code sample with logging and unit tests?
