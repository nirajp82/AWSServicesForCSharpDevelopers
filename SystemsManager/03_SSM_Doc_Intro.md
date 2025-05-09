# 📘 Chapter 3: Understanding SSM Documents in Depth

---

## 🧾 3.1: What is an SSM Document?

An **SSM Document** (also called an "Automation document" or "SSM doc") is a JSON or YAML file that defines **one or more actions** that AWS Systems Manager will run on your managed instances or resources.

Think of it as:

> 🧠 “A recipe that Systems Manager reads and executes step-by-step.”

Each document defines:

* What action to perform
* What parameters (inputs) it requires
* How to log results
* Conditions and control flow
* Outputs (optional)

---

## 🧱 3.2: Types of SSM Documents

SSM supports different **schema versions** and **document types**:

| Document Type    | Description                                     | Example                       |
| ---------------- | ----------------------------------------------- | ----------------------------- |
| `Command`        | Runs shell/PowerShell commands                  | `AWS-RunShellScript`          |
| `Automation`     | Executes multi-step workflows                   | `AWS-CreateImage`             |
| `Session`        | Configures interactive Session Manager sessions | `AWS-StartInteractiveCommand` |
| `Package`        | Defines package metadata for AWS Distributor    | Software distribution         |
| `Policy`         | Defines access control behavior                 | Restrict ports/scripts etc.   |
| `ChangeCalendar` | Defines change freeze windows                   | Block changes during holidays |

You’ll mostly use:

* `Command` documents for **State Manager and Run Command**
* `Automation` documents for **multi-step runbooks**

---

## 🔄 3.3: Schema Versions

| Version | Type       | Notes                                              |
| ------- | ---------- | -------------------------------------------------- |
| 1.2     | Command    | Default for most Run Command/State Manager scripts |
| 2.0     | Automation | Required for multi-step workflows                  |
| 0.3     | Session    | Used for customizing terminal access               |

---

## 📄 3.4: Structure of a Command Document (v1.2)

Let’s break down a simple YAML document that installs Apache on Linux:

```yaml
schemaVersion: "1.2"
description: "Install Apache Web Server"
parameters:
  packages:
    type: "StringList"
    default: ["httpd"]
mainSteps:
  - action: "aws:runShellScript"
    name: "installApache"
    inputs:
      runCommand:
        - "yum install -y {{ packages }}"
        - "systemctl enable httpd"
        - "systemctl start httpd"
```

### Breakdown:

| Section         | Purpose                                         |
| --------------- | ----------------------------------------------- |
| `schemaVersion` | Defines how the doc will be interpreted         |
| `parameters`    | Inputs passed during execution                  |
| `mainSteps`     | The **actions** — each is a step in the runbook |
| `action`        | The plugin used (like `aws:runShellScript`)     |
| `inputs`        | Data for that action — like commands or URLs    |

---

## 🔌 3.5: Plugins (The Real Power)

Each step in `mainSteps` uses a **plugin**. Here are some powerful ones:

| Plugin                                            | Use Case                          |
| ------------------------------------------------- | --------------------------------- |
| `aws:runShellScript`                              | Run shell commands (Linux)        |
| `aws:runPowerShellScript`                         | Run PowerShell commands (Windows) |
| `aws:downloadContent`                             | Download from S3, GitHub, etc.    |
| `aws:copyImage`                                   | Copy AMIs across regions          |
| `aws:createImage`                                 | Create EC2 AMIs                   |
| `aws:invokeLambdaFunction`                        | Call a Lambda mid-workflow        |
| `aws:branch`, `aws:choice`, `aws:waitForResource` | Conditional logic                 |

---

## 🛠️ 3.6: Creating Custom Documents

## 🔧 **Why Create a Custom SSM Document?**

AWS provides **default documents** (also called managed documents) for **common use cases**, but they don’t always match your **specific automation needs**.

### ✅ You create a custom document when:

| Use Case                      | Why Custom Document is Needed                                               |
| ----------------------------- | --------------------------------------------------------------------------- |
| 🛠 Run a `.exe` on Windows    | Default docs don’t support `.exe` execution with parameters                 |
| 🧪 Define your own parameters | You want to **prompt users or code** for inputs (like path, flags)          |
| 📜 Reuse custom logic         | You want to **version**, **reuse**, and **trigger** your scripts repeatedly |
| 🔐 Add safety/logic           | Add pre-checks, error handling, logging, or conditional behavior            |

## 📄 **Default (Managed) Documents Provided by AWS**

AWS gives you many ready-to-use documents. A few common ones:

| Document Name             | Type       | Description                            |
| ------------------------- | ---------- | -------------------------------------- |
| `AWS-RunShellScript`      | Command    | Run Bash scripts on **Linux**          |
| `AWS-RunPowerShellScript` | Command    | Run PowerShell on **Windows**          |
| `AWS-UpdateSSMAgent`      | Command    | Update the SSM agent on instance       |
| `AWS-ApplyPatchBaseline`  | Command    | Apply OS patches                       |
| `AWS-ConfigureAWSPackage` | Command    | Install or uninstall software packages |
| `AWS-StartEC2Instance`    | Automation | Start EC2 instance                     |
| `AWS-StopEC2Instance`     | Automation | Stop EC2 instance                      |

➡️ **You do NOT need to write these yourself**. They’re created and maintained by AWS.

Perfect — here's a **complete and clean rewrite** that brings all your elements together:

---
## 🧾 **Custom SSM Document with Logging, Exit Code, and Error Capture**

* Waits for the process to complete using `-Wait`.
* Captures standard output and standard error to a file.
* Handles exit codes for success/failure reporting.
* Includes **detailed comments** in both JSON and .NET code.

```json
{
  "schemaVersion": "2.2",
  "description": "Run a Windows .exe with parameters, capture output and exit status.",
  "parameters": {
    "exePath": {
      "type": "String",
      "description": "Full path to the .exe file",
      "default": "C:\\Tools\\mytool.exe"
    },
    "mode": {
      "type": "String",
      "description": "Mode parameter to pass (e.g., safe, fast)",
      "default": "safe"
    },
    "logPath": {
      "type": "String",
      "description": "Path to write tool logs",
      "default": "C:\\output.txt"
    },
    "outPath": {
      "type": "String",
      "description": "Standard output file path",
      "default": "C:\\stdout.txt"
    },
    "errPath": {
      "type": "String",
      "description": "Standard error file path",
      "default": "C:\\stderr.txt"
    }
  },
  "mainSteps": [
    {
      "action": "aws:runPowerShellScript",
      "name": "runExecutableWithLogging",
      "inputs": {
        "runCommand": [
          "$exePath = '{{ exePath }}'",
          "$mode = '{{ mode }}'",
          "$logPath = '{{ logPath }}'",
          "$outPath = '{{ outPath }}'",
          "$errPath = '{{ errPath }}'",

          # Start the process and capture output and error
          "$processInfo = New-Object System.Diagnostics.ProcessStartInfo",
          "$processInfo.FileName = $exePath",
          "$processInfo.Arguments = \"--mode $mode --log $logPath\"",
          "$processInfo.RedirectStandardOutput = $true",
          "$processInfo.RedirectStandardError = $true",
          "$processInfo.UseShellExecute = $false",
          "$processInfo.CreateNoWindow = $true",

          "$process = New-Object System.Diagnostics.Process",
          "$process.StartInfo = $processInfo",
          "$process.Start() | Out-Null",

          "$stdOut = $process.StandardOutput.ReadToEnd()",
          "$stdErr = $process.StandardError.ReadToEnd()",

          "$process.WaitForExit()",

          # Write output to files
          "Set-Content -Path $outPath -Value $stdOut",
          "Set-Content -Path $errPath -Value $stdErr",

          # Return exit code (SSM logs it)
          "Write-Output \"Exit Code: $($process.ExitCode)\"",
          "if ($process.ExitCode -ne 0) { exit $process.ExitCode }"
        ]
      }
    }
  ]
}
```


## 💻 .NET Code to Run the Above SSM Document

```csharp
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SsmExecutor
{
    private readonly IAmazonSimpleSystemsManagement _ssm;

    public SsmExecutor()
    {
        // Uses AWS credentials from environment, IAM role, or SDK default chain
        _ssm = new AmazonSimpleSystemsManagementClient();
    }

    /// <summary>
    /// Sends a command to the target EC2 instance to run a .exe file with parameters.
    /// Captures stdout, stderr, and exit code.
    /// </summary>
    public async Task RunExecutableAsync(string instanceId)
    {
        var request = new SendCommandRequest
        {
            DocumentName = "RunExeWithLoggingAndExitCheck", // Replace with your SSM document name
            InstanceIds = new List<string> { instanceId },
            Comment = "Run tool.exe with logging and error capture",

            // Parameters to pass to the custom document
            Parameters = new Dictionary<string, List<string>>
            {
                { "exePath", new List<string> { "C:\\Tools\\mytool.exe" } },
                { "mode", new List<string> { "safe" } },
                { "logPath", new List<string> { "C:\\tool-logs\\log.txt" } },
                { "outPath", new List<string> { "C:\\tool-logs\\stdout.txt" } },
                { "errPath", new List<string> { "C:\\tool-logs\\stderr.txt" } }
            }
        };

        try
        {
            var response = await _ssm.SendCommandAsync(request);
            Console.WriteLine($"✅ Command sent. Command ID: {response.Command.CommandId}");

            // Optional: poll GetCommandInvocationAsync to check for success/failure
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to send command: {ex.Message}");
        }
    }
}
```
##### 📌 Summary of Enhancements

| Feature            | Description                                                                |
| ------------------ | -------------------------------------------------------------------------- |
| `-Wait`            | Ensures the command completes before moving on                             |
| `stdout`, `stderr` | Captured and saved to files (`outPath`, `errPath`)                         |
| Exit Code Handling | Returns non-zero codes to SSM, enabling failure detection                  |
| Parameterization   | You can override any input (`exePath`, `mode`, `logPath`, etc.) at runtime |



## 🛠 **When You Should Use a Custom Document Instead of a Default One**



### 🔁 Why Custom > Default Here?

The default `AWS-RunPowerShellScript`:

* Makes you **pass the full string manually**
* Provides **no structure or UI** for your parameters
* Offers **no validation or reuse**

Your **custom document**:

* Lets you re-use across instances
* Supports **parameter defaults**
* Integrates with `CreateAssociation`, `SendCommand`, or `State Manager`
---
## 📥 How to Create One (YAML or JSON)

### 🔧 CLI Example (YAML File)

```bash
aws ssm create-document \
  --name "MyCustomInstaller" \
  --document-type "Command" \
  --content file://install-script.yaml \
  --document-format YAML
```

### 📁 Sample install-script.yaml

```yaml
schemaVersion: "2.2"
description: "Install Apache HTTPD"
parameters: {}
mainSteps:
  - action: aws:runShellScript
    name: installApache
    inputs:
      runCommand:
        - yum install -y httpd
        - systemctl start httpd
```

## 🧠 Summary

| Topic                | Explanation                                                                    |
| -------------------- | ------------------------------------------------------------------------------ |
| **Default Document** | Prebuilt AWS-managed documents for common tasks like scripting, patching       |
| **Custom Document**  | User-defined document to run scripts, define parameters, and customize logic   |
| **Why Custom**       | When you need parameters, versioning, input defaults, or custom business logic |
| **How to Create**    | Via console, CLI (`aws ssm create-document`), or SDKs                          |
| **Document Types**   | `Command`, `Automation`, `Policy`, `Session`, `Package`                        |

---

## 🔁 3.7: Versioning and Sharing

* **Documents support versioning**: each update creates a new version
* Use `default-version` to control which version new associations will use
* You can **share** custom documents across:

  * Accounts
  * Organizations (with AWS Resource Access Manager)
  * Public (make global tools)

```bash
aws ssm update-document-default-version \
  --name MyApacheInstaller \
  --document-version 2
```

---

## 🔐 3.8: Permissions and Access Control

Use IAM policies to control:

* Who can create/edit documents
* Who can associate them with instances
* What documents an EC2 can run (via instance profile)

📌 Best Practice:

> Use **least privilege** IAM policies and limit sensitive documents (like those installing software or accessing Parameter Store secrets).

---

## 🔭 3.9: Real-World Document Examples

### ✅ Example 1: Install and Start Docker

```yaml
schemaVersion: "1.2"
description: "Install Docker"
mainSteps:
  - action: "aws:runShellScript"
    name: "installDocker"
    inputs:
      runCommand:
        - "yum install -y docker"
        - "systemctl start docker"
        - "systemctl enable docker"
```

### ✅ Example 2: Automation to Create AMI

```yaml
schemaVersion: "2.2"
description: "Create AMI from Instance"
parameters:
  InstanceId:
    type: String
mainSteps:
  - name: createImage
    action: aws:createImage
    inputs:
      InstanceId: "{{ InstanceId }}"
      ImageName: "MyBackup-{{ global:DATE_TIME }}"
```

---

## 🧪 3.10: Testing Documents

Test documents safely with:

```bash
aws ssm send-command \
  --document-name "MyApacheInstaller" \
  --targets "Key=InstanceIds,Values=i-0123456789abcdef0"
```

Watch output logs in:

* Console (SSM > Run Command)
* CloudWatch Logs (if configured)
* Output S3 bucket (if defined)

---

## ✅ Chapter Summary

* **SSM Documents** define *what to execute* — shell scripts, workflows, automation, or interactive sessions.
* Documents use **schema + plugins + parameters** to define workflows.
* You can create **custom documents**, version them, and apply them to groups of machines with **State Manager** or **Run Command**.
* They are the building blocks of automation in Systems Manager.
