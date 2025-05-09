# 📘 Chapter 5: Deep Dive into Associations in State Manager

---

## 🧩 5.1: What Is an Association in State Manager?

An **Association** is the glue between:

* A **Document** (defines *what* to do)
* A **Target** (defines *where* to do it)
* **Parameters** (defines *how* to customize it)
* **Schedule** (defines *when* to do it)

> 🧠 Think of an Association as:
> “Apply this document, with these parameters, to these instances, on this schedule.”

---

## 🧱 5.2: Core Components of an Association

| Component            | Purpose                                      | Example                         |
| -------------------- | -------------------------------------------- | ------------------------------- |
| `DocumentName`       | Which SSM Document to apply                  | `AWS-RunShellScript`            |
| `Targets`            | Which EC2s or resources to apply it to       | Tags, Instance IDs              |
| `Parameters`         | Inputs to the document                       | Script arguments, configuration |
| `Schedule`           | CRON or rate expression to run it repeatedly | `cron(0 2 * * ? *)`             |
| `ComplianceSeverity` | Marks failures as Minor, Medium, Critical    | For reporting                   |
| `OutputLocation`     | S3 bucket for logs                           | `s3://my-logs/`                 |

---

## 🎯 5.3: Defining Targets

You specify **what machines** get the automation.

### You can target by:

* Instance ID
* Resource Group
* AWS Tags
* All managed instances

### Example by Tag:

```json
"Targets": [
  {
    "Key": "tag:Environment",
    "Values": ["Production"]
  }
]
```

This lets you apply policies dynamically to groups of machines.

---

## 🧪 5.4: Creating an Association

### Via AWS Console:

* Go to **Systems Manager > State Manager > Create Association**
* Choose Document (e.g., `MyPatchInstaller`)
* Set targets
* Add parameters
* Define schedule (optional)

### Via CLI:

```bash
aws ssm create-association \
  --name "MyCustomDoc" \
  --targets "Key=tag:Role,Values=Web" \
  --parameters '{"packages":["nginx"]}' \
  --schedule-expression "cron(0 0 * * ? *)" \
  --output-location "S3Location={OutputS3BucketName=my-ssm-logs}"
```

---

## ⏰ 5.5: Scheduling with CRON Expressions

You can schedule recurring execution using:

* `cron()` expressions (like UNIX)
* `rate()` expressions

### Examples:

* `cron(0 2 * * ? *)` → Every day at 2 AM UTC
* `rate(1 day)` → Every day

> 🧠 All schedules run in **UTC**

---

## 🔍 5.6: Association Compliance and Status

Each association has a **status**:

* `Success` — document ran and exited 0
* `Failed` — document ran but failed
* `Pending`, `InProgress`, etc.

You can check compliance in:

* **State Manager > Associations**
* **SSM > Compliance Dashboard**
* **AWS Config** (if integrated)

This is powerful for **auditing** and **policy enforcement**.

---

## 📦 5.7: Using Parameter Store in Associations

Let’s say you want a document to restart a service, but the service name varies per environment.

Create a Parameter:

```bash
aws ssm put-parameter \
  --name "/dev/serviceName" \
  --value "nginx" \
  --type "String"
```

Now create a **State Manager Association** that uses this value:

```bash
aws ssm create-association \
  --name "RestartService" \
  --targets "Key=tag:Environment,Values=dev" \
  --parameters '{"service":["{{ssm:/dev/serviceName}}"]}'
```

---

## 📁 5.8: Logging and Outputs

You can specify where the outputs go:

```json
"OutputLocation": {
  "S3Location": {
    "OutputS3BucketName": "my-ssm-logs",
    "OutputS3KeyPrefix": "logs/"
  }
}
```

Additionally, you can stream logs to **CloudWatch Logs** by enabling logging in the document.

---

## 🛡️ 5.9: IAM Permissions for Associations

To **create or run associations**, the calling identity needs:

```json
{
  "Effect": "Allow",
  "Action": [
    "ssm:CreateAssociation",
    "ssm:UpdateAssociation",
    "ssm:ListAssociations"
  ],
  "Resource": "*"
}
```

The **instance profile role** also needs permission to run the SSM Document and access Parameter Store.

---

## 🌍 5.10: Global Use Case: Per-Tenant State Enforcement

Imagine your architecture supports **multi-tenant workloads** (e.g., SaaS), and each tenant gets custom software or scripts.

You can:

1. Store per-tenant settings in Parameter Store: `/tenants/alpha/featureFlag`
2. Use **State Manager Association per tenant**
3. Reference tenant-specific documents or configs dynamically

Now you have tenant-level enforcement of:

* Agent installation
* Backup scheduling
* Policy compliance
* Patching cycles

---

## ✅ Chapter Summary

* Associations are the **runtime connection** between documents, parameters, targets, and schedules.
* They allow **automated, recurring, targeted, and parameterized** execution of your SSM Documents.
* They support compliance tracking, output logging, IAM control, and environment-specific behavior.
