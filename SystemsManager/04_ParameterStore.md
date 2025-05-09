# 📘 Chapter 5: Parameter Store and Its Role in State Manager & SSM Documents

---

## 🧭 5.1: What Is AWS Systems Manager Parameter Store?

Parameter Store is a **centralized configuration management** and **secret storage** service built into AWS Systems Manager.

Think of it as:

> 🧠 “An encrypted database of configuration values and secrets, where each value is retrieved securely during automation.”

---

## 🔑 5.2: Why Use Parameter Store?

| Use Case                                                  | Benefit                                            |
| --------------------------------------------------------- | -------------------------------------------------- |
| Storing config values (e.g., DB endpoints, feature flags) | Easy to reference from SSM Documents               |
| Managing secrets (e.g., passwords, tokens)                | Supports encryption via KMS                        |
| Sharing parameters across regions or accounts             | Centralized and secure                             |
| Automating environments (dev/stage/prod)                  | Use different values for the same automation logic |

---

## 📂 5.3: Types of Parameters

There are **three main types**:

| Type           | Example Value               | Notes                         |
| -------------- | --------------------------- | ----------------------------- |
| `String`       | `"us-west-2"`               | Plain text                    |
| `StringList`   | `"nginx,apache,httpd"`      | Comma-separated string values |
| `SecureString` | `"password123"` (encrypted) | Encrypted using AWS KMS       |

> You can choose an AWS KMS Key for encryption. If none is provided, AWS uses the default SSM key.

---

## 🛠️ 5.4: Creating Parameters

### AWS Console:

* Go to **Systems Manager > Parameter Store**
* Click **Create parameter**
* Choose type, name, and value
* Optionally, enable **tier** (Standard or Advanced)

### AWS CLI:

```bash
aws ssm put-parameter \
  --name "/webapp/dbPassword" \
  --value "SuperSecret123" \
  --type SecureString \
  --key-id alias/aws/ssm
```

---

## 🏷️ 4.5: Naming Convention & Hierarchies

Parameter names can be flat or hierarchical:

| Name                    | Hierarchical? | Use Case                      |
| ----------------------- | ------------- | ----------------------------- |
| `/myapp/dev/dbPassword` | ✅ Yes         | Dev environment config        |
| `envSetting`            | ❌ No          | Simple config or ad hoc usage |

Hierarchies help with:

* Access control via IAM
* Environment separation (dev, stage, prod)
* Easier organization

---

## ⚙️ 4.6: Retrieving Parameters

### AWS CLI:

```bash
aws ssm get-parameter \
  --name "/webapp/dbPassword" \
  --with-decryption
```

### EC2/SSM Documents:

Inside a document or script, you can retrieve like:

```bash
aws ssm get-parameters \
  --names "/myapp/dev/dbEndpoint" \
  --with-decryption
```

### Using `ssm:` dynamic references (Recommended!):

Inside EC2 user-data, SSM Document, or other AWS services:

```bash
DB_PASSWORD="{{ssm:/myapp/dev/dbPassword}}"
```

With SecureString, you can even use:

```bash
{{ssm-secure:/myapp/dev/dbPassword}}
```

---

## 🧠 4.7: Parameter Store in SSM Documents

Inside a document, **parameters** can reference values directly from Parameter Store.

### Example:

```yaml
parameters:
  dbPassword:
    type: String
    default: "{{ssm-secure:/myapp/dev/dbPassword}}"
```

Or use inside `runCommand`:

```yaml
runCommand:
  - echo "DB Password is {{ dbPassword }}"
```

Now the value is securely injected at runtime — no hardcoding!

---

## 🔄 4.8: Parameter Store with State Manager

When associating a **State Manager document**, you can inject values from Parameter Store into the association:

### Example:

```bash
aws ssm create-association \
  --name "InstallMyApp" \
  --targets Key=tag:App,Values=web \
  --parameters '{"dbPassword":["{{ssm-secure:/myapp/prod/dbPassword}}"]}'
```

This lets you:

* Create **generic** SSM Documents
* Use **dynamic values** from Parameter Store per environment
* Apply the same automation to many servers with different config

---

## 🧪 4.9: Advanced Features

### ❗ Expiration:

Parameters can have expiration policies (Advanced Tier).

### 📜 Policies:

You can attach policies to parameters like:

* Require rotation
* Expire after X days
* Notify on access

### 📊 History & Auditing:

* Up to 100 versions of each parameter are retained
* Track changes and roll back
* Use AWS CloudTrail to log access and updates

---

## 🔐 4.10: IAM Permissions

Access to Parameter Store is **strictly controlled** via IAM.

### Examples:

```json
{
  "Effect": "Allow",
  "Action": [
    "ssm:GetParameter",
    "ssm:GetParameters",
    "ssm:GetParameterHistory"
  ],
  "Resource": "arn:aws:ssm:*:*:parameter/myapp/*"
}
```

If using **SecureString**, you must also grant permission to use the KMS key:

```json
{
  "Effect": "Allow",
  "Action": ["kms:Decrypt"],
  "Resource": "arn:aws:kms:region:account-id:key/key-id"
}
```

---

## ✅ Chapter Summary

* Parameter Store provides **secure**, centralized storage for configurations and secrets.
* It works seamlessly with **SSM Documents** and **State Manager**, allowing you to write **one document for many environments** by referencing different parameters.
* SecureString parameters are encrypted with KMS and ideal for secrets.
* Parameters can be hierarchical, versioned, access-controlled, and rotated.
