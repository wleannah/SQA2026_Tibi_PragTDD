# Ch13 — CodeQL Security Demo: Bank4Us Account Lookup

## Purpose

This chapter demonstrates CodeQL's **taint-tracking** capability using a deliberately
vulnerable Bank4Us service.  It is the live coding example for **COSC 6055 Week 11 —
Security Testing & Static Analysis**.

The goal is to show students that:
1. Unit tests alone **cannot** detect SQL injection (the code compiles and runs fine with valid input)
2. CodeQL **can** detect it statically — before any code reaches production
3. The fix is a single pattern change: parameterized queries

---

## Project Structure

```
ch13/
└── Bank4Us.Security/
    ├── AccountLookupService.cs   ← deliberate CWE-89 vulnerability + safe version
    └── Bank4Us.Security.csproj
```

---

## The Vulnerability

**File:** `Bank4Us.Security/AccountLookupService.cs`
**Method:** `GetAccountsByOwner(string ownerName)`
**CWE:** [CWE-89 — SQL Injection](https://cwe.mitre.org/data/definitions/89.html)
**OWASP:** [A05:2025 — Injection](https://owasp.org/Top10/A05_2021-Injection/)

### Vulnerable Code

```csharp
// VULNERABLE — ownerName is concatenated into SQL
var sql = "SELECT AccountId, OwnerName, Balance FROM Accounts WHERE OwnerName = '"
          + ownerName + "'";
using var cmd = new SqlCommand(sql, conn);
```

### Attack Scenario

An attacker supplies:  `ownerName = "' OR '1'='1"`

Resulting SQL:
```sql
SELECT AccountId, OwnerName, Balance FROM Accounts WHERE OwnerName = '' OR '1'='1'
```
This returns **all account rows** regardless of ownership — a complete authorization bypass.

---

## The Fix

**Method:** `GetAccountsByOwnerSafe(string ownerName)`

```csharp
// SAFE — parameterized query, user input never touches the SQL string
const string sql = "SELECT AccountId, OwnerName, Balance FROM Accounts WHERE OwnerName = @ownerName";
using var cmd = new SqlCommand(sql, conn);
cmd.Parameters.AddWithValue("@ownerName", ownerName);
```

CodeQL will **not** flag the parameterized version because the user input cannot reach the `SqlCommand` constructor as a SQL string.

---

## CodeQL Taint-Tracking Path

```
SOURCE      → ownerName parameter (user-controlled input)
PROPAGATION → string concatenation into 'sql' variable
SINK        → new SqlCommand(sql, conn)  ← unsafe SQL reaches the database engine
```

CodeQL models `SqlCommand` as a known SQL-injection sink for C# and will emit:

> **Rule:** `cs/sql-injection`
> **Severity:** High
> **Message:** "This query depends on a user-provided value."
> **Location:** `AccountLookupService.cs` line 48

---

## Running CodeQL Locally

Prerequisites: [CodeQL CLI](https://github.com/github/codeql-cli-binaries/releases)

```bash
# 1. Create the database (intercepts the dotnet build)
codeql database create bank4us-db \
  --language=csharp \
  --command="dotnet build Bank4Us.Security/Bank4Us.Security.csproj"

# 2. Run the security-extended query suite
codeql database analyze bank4us-db \
  csharp-security-extended \
  --format=sarif-latest \
  --output=results.sarif

# 3. Inspect results
cat results.sarif | jq '.runs[0].results[] | {rule: .ruleId, msg: .message.text, file: .locations[0].physicalLocation.artifactLocation.uri}'
```

---

## GitHub Actions (CI)

The workflow at `.github/workflows/codeql.yml` (repo root) runs this analysis on every push
to `main` and on all pull requests.  Results appear in the **Security → Code scanning** tab.

---

## Variant Analysis

Once CodeQL finds `cs/sql-injection` in `GetAccountsByOwner`, run variant analysis to
scan the **entire codebase** for the same pattern:

```ql
import csharp
import semmle.code.csharp.security.dataflow.SqlInjectionQuery

from SqlInjectionConfiguration config, DataFlow::PathNode source, DataFlow::PathNode sink
where config.hasFlowPath(source, sink)
select sink, source, sink, "SQL injection: $@.", source, "user-controlled value"
```

This systematically eliminates all variants — not just the one you spotted manually.
