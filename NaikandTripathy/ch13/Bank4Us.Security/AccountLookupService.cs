using Microsoft.Data.SqlClient;

namespace Bank4Us.Security;

/// <summary>
/// Bank4Us Account Lookup Service
///
/// IMPORTANT — EDUCATIONAL DEMO:
/// This file contains a DELIBERATE vulnerability for CodeQL Week 11 demonstration.
/// CWE-89: Improper Neutralization of Special Elements used in an SQL Command (SQL Injection).
///
/// The method <see cref="GetAccountsByOwner"/> concatenates user-supplied input directly
/// into a SQL query string.  CodeQL's taint-tracking query will flag this as a
/// high-severity finding (OWASP A05:2025 — Injection).
///
/// The companion method <see cref="GetAccountsByOwnerSafe"/> shows the correct fix:
/// a parameterized query that CodeQL will NOT flag.
///
/// See: README.md for the full CodeQL demo walkthrough.
/// </summary>
public sealed class AccountLookupService
{
    private readonly string _connectionString;

    public AccountLookupService(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // VULNERABLE METHOD — CWE-89 SQL Injection
    //
    // The 'ownerName' parameter flows from an external caller (e.g., an HTTP request
    // parameter) directly into the SQL string via string concatenation.
    //
    // Attack example:  ownerName = "' OR '1'='1"
    //   Resulting SQL: SELECT … WHERE OwnerName = '' OR '1'='1'
    //   Effect:        Returns ALL account rows regardless of ownership.
    //
    // CodeQL taint-tracking path:
    //   SOURCE:      ownerName parameter (user-controlled)
    //   PROPAGATION: string concatenation into 'sql'
    //   SINK:        SqlCommand constructor — unsafe SQL reaches the database
    // ─────────────────────────────────────────────────────────────────────────────
    public List<AccountRecord> GetAccountsByOwner(string ownerName)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        // VULNERABILITY: string concatenation — CodeQL flags this as CWE-89
        var sql = "SELECT AccountId, OwnerName, Balance FROM Accounts WHERE OwnerName = '"
                  + ownerName + "'";

        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        var results = new List<AccountRecord>();
        while (reader.Read())
            results.Add(new AccountRecord(reader.GetString(0), reader.GetString(1), reader.GetDecimal(2)));

        return results;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // SAFE METHOD — Parameterized Query (CodeQL will NOT flag this)
    //
    // @ownerName is a named parameter — the database driver handles escaping.
    // User input never becomes part of the SQL command string.
    // ─────────────────────────────────────────────────────────────────────────────
    public List<AccountRecord> GetAccountsByOwnerSafe(string ownerName)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        // SAFE: parameterized query — user input is passed as a parameter, not concatenated
        const string sql = "SELECT AccountId, OwnerName, Balance FROM Accounts WHERE OwnerName = @ownerName";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ownerName", ownerName);

        using var reader = cmd.ExecuteReader();

        var results = new List<AccountRecord>();
        while (reader.Read())
            results.Add(new AccountRecord(reader.GetString(0), reader.GetString(1), reader.GetDecimal(2)));

        return results;
    }
}

/// <summary>
/// Lightweight projection of an Account row returned by lookup queries.
/// </summary>
public sealed record AccountRecord(string AccountId, string OwnerName, decimal Balance);
