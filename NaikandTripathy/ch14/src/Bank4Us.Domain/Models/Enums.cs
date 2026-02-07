
namespace Bank4Us.Domain.Models;

public enum IdentifierType
{
    Ssn,
    Itin,
    Passport,
    AlienRegistrationNumber,
    Other
}

public enum CitizenshipStatus
{
    Citizen,
    PermanentResident,
    Unknown
}

public enum ApplicationStatus
{
    Approved,
    Cancelled,
    Incomplete,
    PendingVerification,
    NeedsExtraVerification
}

public enum CitizenshipDecision
{
    Proceed,
    NeedsExtraVerification,
    Incomplete
}
