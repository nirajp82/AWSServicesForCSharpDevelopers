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

Let's break this down clearly into **detailed explanations**, **code comments**, and highlight what each line of the `mainSteps` section is doing inside the SSM document.

---

## 🧾 `mainSteps` Section – Full Breakdown & Comments

### JSON Snippet:

```json
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
```

---

### 🧠 What this block does:

* This defines a **step** that AWS SSM should perform when the document is triggered.
* It runs a **PowerShell script** on the target EC2 instance using the `aws:runPowerShellScript` plugin.
* The `inputs.runCommand` section is the actual PowerShell command that gets executed.

---

### 🔍 Line-by-line Explanation

```json
"action": "aws:runPowerShellScript",
```

* **This tells SSM to run a PowerShell script** on the instance.
* AWS provides several plugins like `aws:runShellScript`, `aws:copyFile`, `aws:restart`, etc. Here, we’re specifically using PowerShell for Windows-based EC2.

---

```json
"name": "runPowerShellScript",
```

* This is just an identifier for this step.
* Useful when there are multiple steps in a document—you can refer to this step by name in logs or later conditions.

---

```json
"inputs": {
  "runCommand": [
    "$exePath = '{{ exePath }}'",
    "$param1 = '{{ param1 }}'",
    "$param2 = '{{ param2 }}'",
    "Start-Process -FilePath $exePath -ArgumentList $param1, $param2"
  ]
}
```

Let’s break down each line inside the `runCommand` array:

### ✅ `$exePath = '{{ exePath }}'`

* This takes the parameter named `exePath` from the **SSM document input** and assigns it to a PowerShell variable called `$exePath`.
* Example: if `exePath = "C:\\Tools\\runme.exe"`, this line becomes:

  ```powershell
  $exePath = 'C:\Tools\runme.exe'
  ```

### ✅ `$param1 = '{{ param1 }}'`

* Same idea: pulls the first argument and assigns to `$param1`.

### ✅ `$param2 = '{{ param2 }}'`

* Second argument for the executable.

### ✅ `Start-Process -FilePath $exePath -ArgumentList $param1, $param2`

* **Runs the `.exe`** located at `$exePath`, passing both parameters.
* Equivalent to launching:

  ```powershell
  Start-Process -FilePath "C:\Tools\runme.exe" -ArgumentList "arg1", "arg2"
  ```
* If no parameters are passed, it uses the **default values** from the `parameters` section of the document.

---

## 💡 Important Notes:

* You can **add more parameters** if your `.exe` needs them—just add them to the JSON document and to the `runCommand` array.
* PowerShell will interpret this exactly as if you typed it manually into a Windows terminal on the EC2 machine.

---

## ✅ Benefits of This Setup

* No need to SSH/RDP into instances.
* **Default values** help avoid null reference errors if parameters aren’t passed.

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
        // Initialize the AWS SSM client using default credentials and region from the environment or IAM role
        _ssmClient = new AmazonSimpleSystemsManagementClient();
    }

    /// <summary>
    /// Triggers execution of an SSM Association that runs a custom document on a target EC2 instance.
    /// This document runs a .exe file with optional parameters using PowerShell.
    /// </summary>
    /// <param name="instanceId">The EC2 instance ID where the document should be executed.</param>
    /// <param name="exePath">Full path to the executable (optional). If null, default value from the document is used.</param>
    /// <param name="param1">First argument to pass to the .exe (optional).</param>
    /// <param name="param2">Second argument to pass to the .exe (optional).</param>
    public async Task CreateAssociationAndRunExeAsync(
        string instanceId,
        string exePath = null,
        string param1 = null,
        string param2 = null)
    {
        // Define the CreateAssociationRequest, which binds the document to a specific instance and optionally overrides parameters
        var request = new CreateAssociationRequest
        {
            // This should match the name of the custom SSM document you created in the AWS Console
            Name = "RunExeFromEC2",

            // Target a specific EC2 instance by its instance ID
            Targets = new List<AssociationTarget>
            {
                new AssociationTarget
                {
                    Key = "InstanceIds", // Can also use tags if you want to target by tag instead
                    Values = new List<string> { instanceId }
                }
            },

            // Optional parameters to override document defaults
            Parameters = new Dictionary<string, List<string>>()
        };

        // Only include parameters if they are explicitly passed; otherwise, the SSM document's default will be used
        if (exePath != null)
            request.Parameters["exePath"] = new List<string> { exePath };

        if (param1 != null)
            request.Parameters["param1"] = new List<string> { param1 };

        if (param2 != null)
            request.Parameters["param2"] = new List<string> { param2 };

        try
        {
            // Call AWS to create the association and trigger execution of the document on the instance
            // This is asynchronous: AWS will start running the document shortly after the association is created
            var response = await _ssmClient.CreateAssociationAsync(request);

            // Output some basic information for confirmation/debugging
            Console.WriteLine("✅ Association created:");
            Console.WriteLine($"AssociationId: {response.AssociationDescription.AssociationId}");
        }
        catch (Exception ex)
        {
            // Print the error if something goes wrong (e.g., bad instance ID, permissions issue, document doesn't exist)
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
