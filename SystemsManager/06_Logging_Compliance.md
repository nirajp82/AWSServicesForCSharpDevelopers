# 📘 Chapter 6: Compliance Reporting, Execution History, and Auditing in State Manager

---

## 🎯 6.1: Why This Chapter Matters

You cannot manage what you can’t measure. Automation is powerful — but only if:

* You **know it succeeded**
* You **can prove it** to auditors
* You **can trace failures** and take action

> This chapter gives you **full visibility** into the state of your fleet and automation.

---

## 🔍 6.2: What Is Compliance in State Manager?

Every **Association** in State Manager reports a **compliance status** after each run:

| Status              | Meaning                                |
| ------------------- | -------------------------------------- |
| `COMPLIANT`         | Document ran and succeeded on target   |
| `NON-COMPLIANT`     | Document failed on one or more targets |
| `INSUFFICIENT_DATA` | No run occurred (instance down, etc.)  |

This feeds into:

* **AWS Config compliance dashboard**
* **SSM compliance summary**
* **CloudWatch Events / Alarms**

---

## 📊 6.3: Where Do You View Compliance?

### AWS Console:

**Systems Manager > Compliance > State Manager Associations**

* Filter by status (Compliant / Non-Compliant)
* Drill down into each instance’s output
* View historical compliance trends

### CLI:

```bash
aws ssm list-association-compliance
```

### Example:

```bash
aws ssm list-compliance-summaries \
  --filters "Key=ComplianceType,Values=Association"
```

---

## 🧾 6.4: Understanding Execution History

Each association execution is logged with:

* **Start/End Time**
* **Status**
* **Execution Summary**
* **Output (in S3 or console)**

### View Execution History:

```bash
aws ssm describe-association-executions \
  --association-id "abc-123..."
```

### View Output of a Specific Execution:

```bash
aws ssm get-association-execution-target \
  --association-id "abc-123" \
  --execution-id "exec-456"
```

This helps with:

* Root cause analysis
* SLA tracking
* Proactive alerting

---

## 📥 6.5: Logging to S3 and CloudWatch

When you create an Association, you can define:

* `OutputLocation` → store stdout/stderr in S3
* `CloudWatchLogGroupName` → send to log streams

### Example:

```json
"OutputLocation": {
  "S3Location": {
    "OutputS3BucketName": "my-ssm-logs",
    "OutputS3KeyPrefix": "association-logs/"
  }
},
"CloudWatchLogGroupName": "/ssm/my-app/logs"
```

> This gives you a **centralized and searchable log** trail of all SSM activity.

---

## 🛡️ 6.6: Auditing with CloudTrail

All SSM activities are logged in **AWS CloudTrail**.

### What CloudTrail records:

| Event                      | Example                         |
| -------------------------- | ------------------------------- |
| `CreateAssociation`        | User started automation         |
| `UpdateDocument`           | Document was modified           |
| `StartAutomationExecution` | Document was manually triggered |

This helps you:

* Track **who made changes**
* Detect **unauthorized usage**
* Perform **forensic analysis**

### Query via Athena:

You can even write SQL queries to:

```sql
SELECT *
FROM cloudtrail_logs
WHERE eventName = 'CreateAssociation'
AND userIdentity.userName = 'devops-admin'
```

---

## 🛑 6.7: Alerting on Non-Compliant Associations

Use **EventBridge (CloudWatch Events)** to monitor failures:

### Example Rule:

Trigger when association becomes non-compliant:

```json
{
  "source": ["aws.ssm"],
  "detail-type": ["EC2 Compliance State Change"],
  "detail": {
    "complianceType": ["Association"],
    "complianceStatus": ["NON_COMPLIANT"]
  }
}
```

Send to:

* SNS (email)
* Lambda (remediation)
* Slack (via webhook)

---

## 📦 6.8: Integration with AWS Config

If you’ve enabled AWS Config, **SSM Associations** can:

* Report compliance into Config
* Show up in **Config Rules dashboards**
* Be used to **enforce policies**

For example:

> "Every EC2 tagged `Env=Prod` must have the `InstallMonitoringAgent` document associated and be compliant."

This creates a **closed loop**:

1. Detect drift
2. Remediate with State Manager
3. Verify with Config

---

## 📈 6.9: Long-Term Retention and Audits

* S3 Logging ensures **long-term visibility** (S3 lifecycle policies apply)
* CloudTrail offers **proof for audits** (security, compliance, SOC2, ISO)
* Association logs are versioned and queryable

You can retain:

* Execution results
* Parameter values (without secure strings)
* Target information

---

## ✅ Chapter Summary

* State Manager tracks every execution with status and logs.
* It marks resources as **Compliant or Non-Compliant**, enabling enforcement.
* Output logs can go to **S3** or **CloudWatch**, giving full traceability.
* All activity is captured in **CloudTrail**, making it fully auditable.
* Use **EventBridge** to trigger alerts and remediation from failures.

