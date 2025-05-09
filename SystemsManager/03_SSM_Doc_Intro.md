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

You can create a document via:

### AWS CLI:

```bash
aws ssm create-document \
  --name "MyApacheInstaller" \
  --document-type "Command" \
  --content file://install-apache.yaml \
  --document-format YAML
```

### AWS Console:

* Go to **Systems Manager > Documents**
* Click **Create Document**
* Choose type and upload JSON/YAML

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
