
namespace Bank4Us.Domain.Models;

public sealed record AccountProduct(
    string Name,
    bool RequiresOpeningDeposit,
    decimal MinimumDeposit);
