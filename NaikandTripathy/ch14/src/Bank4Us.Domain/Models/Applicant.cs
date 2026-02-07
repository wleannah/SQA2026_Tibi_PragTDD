
namespace Bank4Us.Domain.Models;

public sealed class Applicant
{
    public IdentifierType IdentifierType { get; set; } = IdentifierType.Ssn;
    public string? IdentificationNumber { get; set; }

    public Address Address { get; set; } = new("1 Main St", "Milwaukee", "WI", "53202");

    public CitizenshipStatus CitizenshipStatus { get; set; } = CitizenshipStatus.Citizen;

    public bool HasResidencyDocumentation { get; set; } = true;

    public decimal? OpeningDepositAmount { get; set; }

    public AccountProduct Product { get; set; } = new("Everyday Checking", true, 200m);
}
