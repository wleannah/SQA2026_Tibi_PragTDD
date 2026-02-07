
using System.Text.RegularExpressions;
using Bank4Us.Domain.Models;
using Bank4Us.Domain.Services;

namespace Bank4Us.Domain.Workflow;

public sealed class NewAccountWorkflow
{
    private readonly IResidencyVerificationService _residency;
    private readonly ICoreBankingSystem _core;

    public NewAccountWorkflow(IResidencyVerificationService residency, ICoreBankingSystem core)
    {
        _residency = residency;
        _core = core;
    }

    public ApplicationResult Process(Applicant applicant)
    {
        // BR-1/AC-5: Application must be complete (ID required)
        if (string.IsNullOrWhiteSpace(applicant.IdentificationNumber))
            return ApplicationResult.Cancelled("ID is required");

        // FR-05: validate identification number format
        var idError = ValidateIdentificationNumber(applicant);
        if (idError is not null)
            return ApplicationResult.Cancelled(idError);

        // Address completeness (BR-1/AC-5)
        var addressError = ValidateAddress(applicant);
        if (addressError is not null)
            return ApplicationResult.Incomplete(addressError);

        // Citizenship partitions
        var decision = EvaluateCitizenship(applicant);
        if (decision == CitizenshipDecision.Incomplete)
            return ApplicationResult.Incomplete("Citizenship status is required");
        if (decision == CitizenshipDecision.NeedsExtraVerification)
            return ApplicationResult.NeedsExtraVerification("Additional citizenship verification required");

        // BR-5 / FR-11: deposit minimum
        var depositResult = ValidateDeposit(applicant);
        if (depositResult is not null)
            return depositResult;

        // BR-3: Residency verification service unavailable => PendingVerification and do not create account
        try
        {
            if (applicant.HasResidencyDocumentation)
                _residency.Verify(applicant);
        }
        catch
        {
            return ApplicationResult.PendingVerification("Residency verification unavailable");
        }

        _core.CreateAccount(applicant);
        return ApplicationResult.Approved();
    }

    public static string? ValidateIdentificationNumber(Applicant applicant)
    {
        return applicant.IdentifierType switch
        {
            IdentifierType.Ssn => Regex.IsMatch(applicant.IdentificationNumber ?? "", @"^\d{3}-\d{2}-\d{4}$")
                ? null
                : "Invalid SSN format",

            IdentifierType.Itin => Regex.IsMatch(applicant.IdentificationNumber ?? "", @"^\d{2}-\d{7}$")
                ? null
                : "Invalid ITIN format",

            IdentifierType.Passport => Regex.IsMatch(applicant.IdentificationNumber ?? "", @"^[A-Z0-9]{6,9}$", RegexOptions.IgnoreCase)
                ? null
                : "Invalid passport format",

            _ => null
        };
    }

    public static string? ValidateAddress(Applicant applicant)
    {
        if (string.IsNullOrWhiteSpace(applicant.Address.Street)) return "Street is required";
        if (string.IsNullOrWhiteSpace(applicant.Address.City)) return "City is required";
        if (string.IsNullOrWhiteSpace(applicant.Address.State)) return "State is required";
        if (string.IsNullOrWhiteSpace(applicant.Address.PostalCode)) return "PostalCode is required";
        return null;
    }

    public static CitizenshipDecision EvaluateCitizenship(Applicant applicant)
    {
        return applicant.CitizenshipStatus switch
        {
            CitizenshipStatus.Citizen => CitizenshipDecision.Proceed,
            CitizenshipStatus.PermanentResident => CitizenshipDecision.NeedsExtraVerification,
            _ => CitizenshipDecision.Incomplete
        };
    }

    private static ApplicationResult? ValidateDeposit(Applicant applicant)
    {
        if (!applicant.Product.RequiresOpeningDeposit)
            return null;

        if (applicant.OpeningDepositAmount is null)
            return ApplicationResult.Incomplete("Opening deposit is required");

        if (applicant.OpeningDepositAmount.Value < applicant.Product.MinimumDeposit)
            return ApplicationResult.Cancelled("Deposit below minimum");

        return null;
    }
}
