// Bank4Us.Security — CodeQL Week 11 Demo
// Minimal API that exposes the vulnerable and safe account lookup endpoints.
//
// CodeQL taint-tracking needs an HTTP SOURCE to flag SQL injection.
// Without a web endpoint, 'ownerName' is just a plain parameter and CodeQL
// has no evidence it comes from untrusted external input.
//
// With these endpoints, CodeQL traces:
//   SOURCE      → req.Query["ownerName"]   (HTTP query string — user-controlled)
//   PROPAGATION → passed into GetAccountsByOwner(ownerName)
//   SINK        → SqlCommand(sql, conn)     (SQL injection)
//
// The /accounts/safe endpoint uses GetAccountsByOwnerSafe — CodeQL will NOT flag it.

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var connectionString = builder.Configuration.GetConnectionString("Bank4Us")
    ?? "Server=localhost;Database=Bank4Us;Integrated Security=true;TrustServerCertificate=true;";

var svc = new AccountLookupService(connectionString);

// VULNERABLE — ownerName from HTTP query string flows into raw SQL
// CodeQL alert: cs/sql-injection (CWE-89, High severity)
app.MapGet("/accounts", (HttpRequest req) =>
{
    var ownerName = req.Query["ownerName"].ToString();
    var accounts = svc.GetAccountsByOwner(ownerName);
    return Results.Ok(accounts);
});

// SAFE — same HTTP source, but routed through a parameterized query
// CodeQL will NOT flag this — no taint path reaches the SQL sink unsanitized
app.MapGet("/accounts/safe", (HttpRequest req) =>
{
    var ownerName = req.Query["ownerName"].ToString();
    var accounts = svc.GetAccountsByOwnerSafe(ownerName);
    return Results.Ok(accounts);
});

app.Run();
