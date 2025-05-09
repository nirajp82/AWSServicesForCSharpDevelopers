# AWS Systems Manager: Executing .exe on EC2 Using SSM Documents and SendCommand (No Association Needed)

This guide explains how to **run a `.exe` file on an EC2 instance using AWS Systems Manager (SSM)**, triggered by a .NET application. It clarifies the use of `SendCommand`, explains why `CreateAssociationAsync` is not needed, and shows how to pass default and optional parameters effectively.

---

## 🧱 System Overview

* **SSM Document**: Created manually or via infrastructure automation. Contains command logic.
* **IAM Role**: Attached to EC2 instance and gives permission to run SSM commands.
* **.NET Application**: Sends the command using the AWS SDK for .NET.
* **NO Association Needed**: We use `SendCommand`, which does not require an association.

---

## 🎯 When to Use `SendCommand` vs `CreateAssociation`

| Use Case                         | Method                |
| -------------------------------- | --------------------- |
| One-time execution on demand     | ✅ `SendCommandAsync`  |
| Recurring/scheduled task         | ✅ `CreateAssociation` |
| Ad-hoc trigger from .NET backend | ✅ `SendCommandAsync`  |

---

## 📌 Why Association is NOT Needed

An **SSM Association** is a way to schedule or continuously enforce a document on an instance. It’s like a "binding" between an EC2 instance and a document with a schedule or compliance policy.

But in our case:

* We **do not want to schedule** the `.exe` file to run repeatedly.
* We only want to run it **when the .NET app requests it**.
* Therefore, we use `SendCommandAsync`, which executes the document **immediately**.

### 🔴 Do NOT use Association if:

* You want **instant control** from a web UI or .NET app.
* Your executions are **on-demand**.
* You are already handling the logic externally (from the app).

---

## 🧾 SSM Document Example with Default Parameters

```json
{
  "schemaVersion": "2.2",
  "description": "Run an EXE file",
  "parameters": {
    "exePath": {
      "type": "String",
      "default": "C:\\Tools\\MyApp.exe",
      "description": "Path to the .exe file"
    },
    "param1": {
      "type": "String",
      "default": "--help",
      "description": "First argument"
    },
    "param2": {
      "type": "String",
      "default": "",
      "description": "Second argument"
    }
  },
  "mainSteps": [
    {
      "action": "aws:runPowerShellScript",
      "name": "runExe",
      "inputs": {
        "runCommand": [
          "& '{{ exePath }}' '{{ param1 }}' '{{ param2 }}'"
        ]
      }
    }
  ]
}
```

This document:

* Has **default values**.
* Can still be overridden by your .NET app if needed.

---

## ✅ .NET Code: Triggering SSM Document Execution

```csharp
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

public class SsmService
{
    private readonly IAmazonSimpleSystemsManagement _ssmClient = new AmazonSimpleSystemsManagementClient();

    public async Task SendCommandToRunExeAsync(string instanceId, string exePath = null, string param1 = null, string param2 = null)
    {
        var request = new SendCommandRequest
        {
            DocumentName = "RunExeFromEC2",
            InstanceIds = new List<string> { instanceId },
            Parameters = new Dictionary<string, List<string>>()
        };

        if (exePath != null) request.Parameters["exePath"] = new List<string> { exePath };
        if (param1 != null) request.Parameters["param1"] = new List<string> { param1 };
        if (param2 != null) request.Parameters["param2"] = new List<string> { param2 };

        var response = await _ssmClient.SendCommandAsync(request);
        Console.WriteLine($"Command sent. ID: {response.Command.CommandId}");
    }
}
```

---

## 🔌 ASP.NET Controller Example

```csharp
public class RunController : Controller
{
    private readonly SsmService _ssmService = new SsmService();

    public async Task<IActionResult> Trigger()
    {
        string instanceId = "i-0123456789abcdef0";
        await _ssmService.SendCommandToRunExeAsync(instanceId);
        return Content("Command triggered.");
    }
}
```

---

## 🛡️ IAM Role Requirements

The EC2 instance must have an IAM role with these permissions:

```json
{
  "Effect": "Allow",
  "Action": [
    "ssm:SendCommand",
    "ssm:ListCommands",
    "ssm:ListCommandInvocations"
  ],
  "Resource": "*"
}
```

Also, your SSM document must be created with appropriate permissions, and the EC2 instance should have **SSM Agent installed and running**.

---

## ✅ Final Summary

| Component        | How It’s Used                                                             |
| ---------------- | ------------------------------------------------------------------------- |
| SSM Document     | Created once with default parameters.                                     |
| IAM Role         | Attached to EC2 with SSM permissions.                                     |
| .NET Application | Calls `SendCommandAsync` to trigger the command immediately.              |
| Association      | ❌ **Not used** — we are not scheduling anything; everything is on-demand. |
