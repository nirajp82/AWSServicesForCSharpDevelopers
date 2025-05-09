# 📘 Chapter 7: Real-World Design Patterns and Architectures Using State Manager

---

## 🧠 7.1: Why Patterns Matter

Knowing how a tool works is great. But knowing **how to apply it effectively at scale** is what separates an engineer from an architect.

Here, we’ll cover real deployment models that use:

* **Parameter Store for configuration**
* **SSM Documents for logic**
* **State Manager Associations for orchestration**
* **CloudWatch, S3, and EventBridge for observability**

---

## 🏗️ 7.2: Pattern 1 – Per-Tenant Configuration and Automation (SaaS)

### 📦 Use Case:

A SaaS platform hosts isolated workloads for **multiple tenants**, each with:

* Unique configs (like backup frequency)
* Optional services (e.g., Monitoring agent ON/OFF)
* Separate environments (dev, stage, prod)

### 🛠️ Architecture:

1. Use Parameter Store:

   ```
   /tenants/alpha/backupFrequency = rate(1 day)
   /tenants/alpha/agentEnabled = true
   ```

2. Create tenant-specific SSM Documents:

   * `Tenant-Bootstrap-Alpha`
   * `Tenant-Backup-Policy`

3. Create Associations:

   * Use tag-based targeting (e.g., `tag:tenant=alpha`)
   * Pull parameters dynamically via `{{ssm:/tenants/alpha/backupFrequency}}`

### ✅ Benefits:

* No code duplication
* Isolated logic per tenant
* Dynamic execution via parameter injection
* Scalable onboarding for new tenants

---

## 🌐 7.3: Pattern 2 – Global DR/Failover State Enforcement

### 📦 Use Case:

In DR scenarios, every EC2 must:

* Install specific DR agents
* Update `/etc/hosts` to route to DR site
* Run validation checks every hour

### 🛠️ Architecture:

* Create SSM Document: `DR-Preparation-Agent`

* Use Parameter Store:

  ```
  /dr/dnsOverride = 10.0.0.200
  /dr/heartbeatInterval = 60
  ```

* Use State Manager to:

  * Associate `DR-Preparation-Agent` with all `tag:dr_enabled=true` instances
  * Pull runtime config via `{{ssm:/dr/*}}`

* Use **cron()** expressions for periodic revalidation

### 🧩 Bonus:

Trigger opt-in/opt-out from the DR plan by toggling tags:

```bash
aws ec2 create-tags --resources i-1234 --tags Key=dr_enabled,Value=true
```

---

## 🛡️ 7.4: Pattern 3 – CIS/OS Hardening with Compliance Tracking

### 📦 Use Case:

Security team mandates:

* CIS-compliant kernel parameters
* SSH hardening
* User cleanup every 24 hours

### 🛠️ Architecture:

1. Create secure documents like `CIS-Harden-SSH`, `CIS-Kernel-Params`
2. Set up **State Manager Associations** with:

   * `ComplianceSeverity: CRITICAL`
   * `cron(0 3 * * ? *)`
3. Enable logging to S3 + CloudWatch
4. Connect to AWS Config for drift detection

### 🔒 Result:

* Every instance is hardened on schedule
* Any failure shows as `NON_COMPLIANT` and triggers alerts
* All executions are logged and auditable

---

## 🔁 7.5: Pattern 4 – Zero-Touch Agent Bootstrapping

### 📦 Use Case:

When a new EC2 is launched, you want to:

* Install the CloudWatch Agent
* Attach it to the right namespace
* Pull region/tenant-specific config

### 🛠️ Architecture:

1. Tag EC2s during provisioning: `Environment=prod`, `Tenant=beta`
2. Create document: `Agent-Bootstrap`
3. Associate with:

   ```json
   "Key": "tag:Environment",
   "Values": ["prod"]
   ```
4. Use parameters like:

   ```
   /prod/logLevel
   /prod/agentUrl
   ```

Now, instances **self-heal and self-configure** upon boot — no manual login or custom AMI baking.

---

## 🔄 7.6: Pattern 5 – Remediation on Non-Compliance

### 📦 Use Case:

If a document fails (e.g., antivirus not installed), trigger:

* An alert
* A second document to fix it

### 🛠️ Architecture:

1. Association with `Install-Antivirus`
2. EventBridge Rule:

   * Trigger when `complianceStatus = NON_COMPLIANT`
3. Rule sends to:

   * SNS alert
   * Lambda that triggers `Fix-Antivirus` document

### ✅ Result:

* Closed-loop healing
* SLA enforcement
* Event-driven ops without manual effort

---

## 🧰 7.7: Bonus Tools & Best Practices

### 📌 Use Parameters for:

* Secrets (`SecureString`)
* Regions / Tenants
* Script input values

### 📌 Use SSM Documents with:

* Schema version 2.2+
* Steps broken out clearly (phases: pre-check, run, cleanup)

### 📌 Use State Manager to:

* Schedule recurring health checks
* Automate onboarding
* Implement patch baselines

### 📌 Tag Everything:

* Tags help target, track, and group
* Use consistent tag keys: `tenant`, `env`, `role`, `service`

---

## 🧠 Chapter Summary

You now know how to:

* Architect **multi-tenant** and **multi-region** automation
* Trigger **automated failover** policies with SSM
* Use **compliance data** to trigger **healing workflows**
* Design **zero-touch infrastructure** with tags, documents, and parameters

---

## 🎓 Final Thoughts: Mastering State Manager

State Manager isn’t just a tool — it’s your **policy engine**, **compliance monitor**, and **automation orchestrator**.

It gives you:

* Infrastructure-as-code execution without agents
* Auditability and compliance tracking
* Dynamic behavior through Parameters
* Enterprise-grade control through tagging, logging, and IAM

