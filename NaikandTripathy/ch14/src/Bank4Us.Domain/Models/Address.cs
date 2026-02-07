
namespace Bank4Us.Domain.Models;

public sealed record Address(
    string? Street,
    string? City,
    string? State,
    string? PostalCode,
    string? Country = "US");
