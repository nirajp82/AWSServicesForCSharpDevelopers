# 📘 Chapter 2: In-Depth Guide to AWS Systems Manager State Manager

---

## 🧩 2.1: What Is State Manager?

**State Manager** is a Systems Manager capability that automatically keeps your EC2 instances (or hybrid machines) in a *desired configuration state*. Think of it as a **continuous configuration enforcement engine**.

### ❓ Why is this useful?

Imagine you want to ensure:

* Apache is always installed and running
* Certain files or packages are always present
* A monitoring agent (like CloudWatch) is never removed

State Manager can:

* Apply these changes when a machine starts
* Periodically re-apply them
* Automatically correct **drift** when someone manually changes a setting

---

## 🏗️ 2.2: State Manager Core Concepts

### 💡 Concept: Association

The **Association** is the key object in State Manager. It’s the bridge between:

* **Document**: What to do (e.g., install software, run a script)
* **Target(s)**: Which resources should it be applied to
* **Schedule**: When or how often should it run
* **Parameters**: What values/configs to pass to the document

#### Example:

> “Apply document `AWS-InstallApache` to EC2s with tag `Role=WebServer` every 6 hours.”

That configuration is stored in an **Association**.

---

### 🔧 Concept: SSM Document

This is the automation script written in JSON/YAML that defines the steps. Examples:

* `AWS-RunShellScript` (run shell commands)
* `AWS-ApplyPatchBaseline`
* `AWS-ConfigureCloudWatch`
* Custom documents like `InstallMyCompanyAgent`

---

### 🎯 Concept: Target

State Manager supports multiple targeting mechanisms:

* **Instance IDs**
* **Tags** (e.g., `Environment=Prod`)
* **Resource Groups**
* **Accounts and OUs** (via Systems Manager Explorer for org-wide rollout)

---

### ⏰ Concept: Schedule

You can define when/how frequently to run associations:

* **At startup** (instance boot)
* **Recurring**: Hourly, daily, cron
* **One-time**: For provisioning-style automation

---

## 🔐 2.3: Association Structure (in Detail)

A full Association JSON structure includes:

```json
{
  "Name": "Install Apache Webserver",
  "DocumentName": "AWS-RunShellScript",
  "Targets": [
    {
      "Key": "tag:Role",
      "Values": ["WebServer"]
    }
  ],
  "Parameters": {
    "commands": ["yum install -y httpd", "systemctl start httpd"]
  },
  "ScheduleExpression": "rate(4 hours)",
  "ComplianceSeverity": "CRITICAL",
  "MaxConcurrency": "5",
  "MaxErrors": "1"
}
```

### 📌 Key Fields Breakdown:

* `DocumentName`: Which document to apply
* `Targets`: EC2s based on tag filtering
* `Parameters`: Values passed to the script (e.g., command arguments)
* `ScheduleExpression`: When to apply
* `ComplianceSeverity`: For tracking config drift
* `MaxConcurrency`: Limits blast radius
* `MaxErrors`: Abort threshold for failures

---

## 🛠️ 2.4: Step-by-Step: Creating an Association (Hands-On)

### 📍 Step 1: Tag EC2 Instances

```bash
aws ec2 create-tags --resources i-0123456789abcdef0 \
  --tags Key=Role,Value=WebServer
```

---

### 📍 Step 2: Create the Association

```bash
aws ssm create-association \
  --name "AWS-RunShellScript" \
  --targets "Key=tag:Role,Values=WebServer" \
  --parameters commands=["yum install -y httpd", "systemctl start httpd"] \
  --schedule-expression "rate(6 hours)" \
  --compliance-severity "CRITICAL"
```

---

### 📍 Step 3: Monitor Compliance

```bash
aws ssm list-association-compliance \
  --association-id <assoc-id>
```

This allows you to track which EC2s are **compliant** (i.e., have the desired state) and which are **non-compliant** (e.g., someone stopped the service manually).

---

## 🔁 2.5: How Reapplication Works

By default, State Manager:

* Applies the document at the defined schedule
* Every time it runs, it re-applies the document
* Ensures the config exists (idempotent by design)

If drift occurs:

* The association detects it
* The next run corrects the drift

This enables **self-healing infrastructure**.

---

## 🔍 2.6: Logging and Debugging

Logs can go to:

* **CloudWatch Logs** (if configured in the document)
* **S3 bucket** (optional)
* **SSM console** > *Association Execution History*

### Enable Logging:

```bash
aws ssm update-association \
  --association-id <assoc-id> \
  --output-location "S3Location={OutputS3BucketName=my-ssm-logs}"
```

---

## 🌐 2.7: Hybrid Use Cases

With **Hybrid Activations**, you can register on-prem servers and VMs with SSM.

Once registered:

* They show up like EC2 in SSM
* Associations can be applied
* Compliance tracked the same way

---

## 🔒 2.8: IAM and Security

You’ll need to grant your EC2s:

* `AmazonSSMManagedInstanceCore`

Optionally, use custom roles to:

* Control document access
* Isolate compliance logs
* Restrict document creation/updates

---

## 🧠 2.9: Advanced Features

### 📊 Compliance Dashboard

* Visual summary of which associations failed
* Severity-based filtering (CRITICAL, HIGH, MEDIUM, LOW)

### 🔄 Parameterization

* Dynamically inject SSM Parameter Store values into documents

### 📤 Pre/Post Hooks

* Run shell scripts before and after an association
* Validate or cleanup conditions

---

## 🧪 2.10: Real-World Use Case: Enforce NTP Sync

**Goal:** Ensure time sync is configured on all Linux machines

**Solution:** Use a shell script with `AWS-RunShellScript`:

```bash
commands:
- yum install -y chrony
- systemctl enable chronyd
- systemctl start chronyd
- chronyc tracking
```

Apply via State Manager every 24 hours to maintain compliance.

---

## ✅ Chapter Summary

* State Manager allows **automated, recurring configuration enforcement**.
* Associations tie documents, targets, parameters, and schedules together.
* It’s **declarative**, **resilient**, and works **at scale** across regions/accounts.
* It can be integrated with **CloudWatch, Config, and SNS** for alerts and compliance.


