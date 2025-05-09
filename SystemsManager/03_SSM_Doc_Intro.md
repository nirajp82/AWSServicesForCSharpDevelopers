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
---

## 🛠 **When You Should Use a Custom Document Instead of a Default One**

Here’s a clear and practical breakdown of **🛠 When You Should Use a Custom SSM Document Instead of a Default Managed One** like `AWS-RunPowerShellScript` or `AWS-RunShellScript`:


### 🔧 Default Managed Documents Are Great When:

| Situation                                                            | Why They Work                                                                             |
| -------------------------------------------------------------------- | ----------------------------------------------------------------------------------------- |
| You want to run **basic shell or PowerShell scripts**                | `AWS-RunShellScript` or `AWS-RunPowerShellScript` let you pass raw script lines directly. |
| You **don’t need parameter UI**, input validation, or default values | Defaults don’t support structured input — you pass raw text only.                         |
| You’re okay with **manually constructing commands**                  | You have to build the full command-line yourself and pass it as a string.                 |
| You’re doing **one-off troubleshooting or automation**               | These documents are easy to use from the console or API.                                  |


### 🛠 You Should Use a **Custom Document** When:

| Situation                                                                                  | Why Custom Docs Are Better                                              |
| ------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------- |
| ✅ You want to reuse logic with **different parameters**                                    | Define named parameters like `exePath`, `mode`, etc., and reuse easily. |
| ✅ You want to provide a **UI with input fields** in the AWS Console                        | Custom documents allow parameter UI in the SSM Console.                 |
| ✅ You want to set **defaults and validations** (e.g., required fields, allowed values)     | Great for safety and DRY automation.                                    |
| ✅ You want to avoid **shell-escaping or string-building manually**                         | Instead of building a big quoted string, parameters are passed cleanly. |
| ✅ You need **clean integration** with `CreateAssociation`, `SendCommand`, or State Manager | Custom documents are treated as reusable objects.                       |

---
## ✅ Custom SSM Document (JSON)  

```json
{
  "schemaVersion": "2.2", // This defines the schema format version SSM expects
  "description": "Run mytool.exe with parameters on demand",
  "parameters": {
    "exePath": {
      "type": "String",
      "description": "Full path to the executable (e.g., C:\\Tools\\mytool.exe)",
      "default": "C:\\Tools\\mytool.exe"
    },
    "mode": {
      "type": "String",
      "description": "Execution mode for the tool",
      "default": "safe"
    },
    "logPath": {
      "type": "String",
      "description": "Path where logs should be written",
      "default": "C:\\output.txt"
    }
  },
  "mainSteps": [
    {
      "action": "aws:runPowerShellScript", // Use PowerShell to run commands on Windows
      "name": "runMyExe",
      "inputs": {
        "runCommand": [
          // Assign parameters to local PowerShell variables
          "$exePath = '{{ exePath }}'",
          "$mode = '{{ mode }}'",
          "$logPath = '{{ logPath }}'",

          // Launch the EXE with parameters, don't wait for completion
          "Start-Process -FilePath $exePath -ArgumentList '--mode', $mode, '--log', $logPath"
        ]
      }
    }
  ]
}
```

### 🔍 Why this is important:

* `Start-Process` is **asynchronous** — it doesn't wait for the `.exe` to finish (which you wanted).
* You can trigger it **on demand** using `SendCommand`.
* You **don't need to hard-code anything**; this document is reusable with new inputs anytime.


## 🚀 C# Code to Trigger It (With Command ID Returned)

```csharp
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SsmOnDemandRunner
{
    private readonly IAmazonSimpleSystemsManagement _ssmClient;

    public SsmOnDemandRunner()
    {
        // SSM client initialized using IAM credentials in your environment (Instance Profile, User, or Role)
        _ssmClient = new AmazonSimpleSystemsManagementClient();
    }

    /// <summary>
    /// Sends a command using your custom SSM document to run an exe file on a target EC2 instance.
    /// </summary>
    public async Task<string> RunExeViaSendCommandAsync(
        string instanceId,        // The EC2 instance you want to run the command on
        string exePath = null,    // Optional override for the exe path
        string mode = null,       // Optional override for mode
        string logPath = null     // Optional override for log output path
    )
    {
        var parameters = new Dictionary<string, List<string>>();

        // Only add parameter if it's not null — otherwise default in document will be used
        if (exePath != null)
            parameters["exePath"] = new List<string> { exePath };
        if (mode != null)
            parameters["mode"] = new List<string> { mode };
        if (logPath != null)
            parameters["logPath"] = new List<string> { logPath };

        var request = new SendCommandRequest
        {
            DocumentName = "MyCustomExeRunner", // Must match the name of your custom SSM document
            InstanceIds = new List<string> { instanceId }, // The EC2 instance(s) to target
            Parameters = parameters,
            Comment = "Trigger .exe run via SendCommand" // Optional comment for traceability
        };

        try
        {
            var response = await _ssmClient.SendCommandAsync(request);

            Console.WriteLine($"✅ Command sent. Command ID: {response.Command.CommandId}");

            // Return the Command ID so the caller can poll the command status later
            return response.Command.CommandId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to send command: {ex.Message}");
            return null;
        }
    }
}
```

## 🧪 Optional: C# Code to Check Command Status Later

```csharp
public async Task CheckCommandStatusAsync(string commandId, string instanceId)
{
    var request = new GetCommandInvocationRequest
    {
        CommandId = commandId,
        InstanceId = instanceId
    };

    var response = await _ssmClient.GetCommandInvocationAsync(request);

    Console.WriteLine($"Status: {response.Status}"); // e.g., Pending, InProgress, Success, Failed
    Console.WriteLine("StdOut:");
    Console.WriteLine(response.StandardOutputContent);
    Console.WriteLine("StdErr:");
    Console.WriteLine(response.StandardErrorContent);
}
```

## 💡 Final Summary

| Feature                              | Implemented In                      |
|  | -- |
| Reusable parameterized `.exe` call   | Custom SSM Document                 |
| Trigger **on demand**                | `SendCommandAsync()`                |
| Do **not wait** for `.exe` to finish | `Start-Process` (no `-Wait`)        |
| Capture status later                 | Command ID + `GetCommandInvocation` |
| Safe defaults                        | Included in document                |

---
### ✅ Real-World Example: Why Custom > Default

**Let’s say you want to run:**

```powershell
C:\Tools\mytool.exe --mode safe --log C:\output.txt
```

With **`AWS-RunPowerShellScript`**, you'd need to send:

```json
"commands": [ "Start-Process -FilePath 'C:\\Tools\\mytool.exe' -ArgumentList '--mode safe', '--log C:\\output.txt'" ]
```

⚠️ That works, **but:**

* No default values
* No structured input
* Hard to validate
* Repetitive boilerplate

### ✅ With a **Custom Document**:

```json
"parameters": {
  "exePath": { "type": "String", "default": "C:\\Tools\\mytool.exe" },
  "mode": { "type": "String", "default": "safe" },
  "logPath": { "type": "String", "default": "C:\\output.txt" }
}
```

Then in your script:

```powershell
Start-Process -FilePath $exePath -ArgumentList "--mode $mode", "--log $logPath"
```

💡 **You now have:**

* Input UI for humans
* Default values
* Cleaner scripts
* Easy versioning and reuse

### 📌 Summary:

| Feature                                 | Default Doc       | Custom Doc         |
| --------------------------------------- | ----------------- | ------------------ |
| Parameter UI in console                 | ❌ No              | ✅ Yes              |
| Default values                          | ❌ No              | ✅ Yes              |
| Input validation                        | ❌ No              | ✅ Yes              |
| Easy reuse with SendCommand/Association | ⚠️ Manual         | ✅ Structured       |
| Clean shell logic                       | ❌ Escaped strings | ✅ Native variables |

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
