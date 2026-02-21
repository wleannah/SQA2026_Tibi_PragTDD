namespace Bank4Us.Core;

public enum ApplicationStatus
{
    Started,
    PersonalInfoProvided,
    IdProvided,
    AddressProvided,
    DepositProvided,
    Submitted,
    IdVerified,
    AddressVerified,
    CitizenVerified,
    Approved,
    Rejected
}

public class AccountApplication
{
    public string? PersonalDetails { get; private set; }
    public string? IdNumber { get; private set; }
    public string? PhotoId { get; private set; }
    public string? ProofOfAddress { get; private set; }
    public string? ImmigrationDocs { get; private set; }
    public decimal OpeningDeposit { get; private set; }
    public ApplicationStatus Status { get; private set; } = ApplicationStatus.Started;

    public bool IsFormComplete => !string.IsNullOrEmpty(PersonalDetails) &&
                                  !string.IsNullOrEmpty(IdNumber) &&
                                  !string.IsNullOrEmpty(PhotoId) &&
                                  !string.IsNullOrEmpty(ProofOfAddress) &&
                                  OpeningDeposit >= 200;

    public bool IsIdVerified { get; private set; }
    public bool IsAddressVerified { get; private set; }
    public bool IsCitizenVerified { get; private set; }

    public void ProvidePersonalDetails(string details)
    {
        if (Status != ApplicationStatus.Started)
            throw new InvalidOperationException("Invalid state for providing personal details");

        if (string.IsNullOrWhiteSpace(details))
            throw new ArgumentException("Personal details cannot be empty");

        PersonalDetails = details;
        Status = ApplicationStatus.PersonalInfoProvided;
    }

    public void ProvideId(string idNumber, string photoId)
    {
        if (Status != ApplicationStatus.PersonalInfoProvided && Status != ApplicationStatus.IdProvided)
            throw new InvalidOperationException("Invalid state for providing ID");

        if (string.IsNullOrWhiteSpace(idNumber) || string.IsNullOrWhiteSpace(photoId))
            throw new ArgumentException("ID details cannot be empty");

        IdNumber = idNumber;
        PhotoId = photoId;
        Status = ApplicationStatus.IdProvided;
    }

    public void ProvideAddress(string proofOfAddress)
    {
        if (Status != ApplicationStatus.IdProvided && Status != ApplicationStatus.AddressProvided)
            throw new InvalidOperationException("Invalid state for providing address");

        if (string.IsNullOrWhiteSpace(proofOfAddress))
            throw new ArgumentException("Address proof cannot be empty");

        ProofOfAddress = proofOfAddress;
        Status = ApplicationStatus.AddressProvided;
    }

    public void ProvideDeposit(decimal amount)
    {
        if (Status != ApplicationStatus.AddressProvided && Status != ApplicationStatus.DepositProvided)
            throw new InvalidOperationException("Invalid state for providing deposit");

        if (amount < 200)
            throw new ArgumentException("Deposit must be at least $200");

        OpeningDeposit = amount;
        Status = ApplicationStatus.DepositProvided;
    }

    public void ProvideImmigrationDocs(string docs)
    {
        // Optional, for non-citizens
        ImmigrationDocs = docs;
    }

    public void SubmitApplication()
    {
        if (Status != ApplicationStatus.DepositProvided)
            throw new InvalidOperationException("Application not ready to submit");

        if (!IsFormComplete)
            throw new InvalidOperationException("Form is not complete");

        Status = ApplicationStatus.Submitted;
    }

    public void VerifyId(bool isValid)
    {
        if (Status != ApplicationStatus.Submitted && Status != ApplicationStatus.IdVerified && Status != ApplicationStatus.AddressVerified && Status != ApplicationStatus.CitizenVerified)
            throw new InvalidOperationException("Invalid state for ID verification");

        IsIdVerified = isValid;
        if (!isValid)
        {
            Status = ApplicationStatus.Rejected;
        }
        else
        {
            Status = ApplicationStatus.IdVerified;
            CheckAllVerifications();
        }
    }

    public void VerifyAddress(bool isValid)
    {
        if (Status != ApplicationStatus.Submitted && Status != ApplicationStatus.IdVerified && Status != ApplicationStatus.AddressVerified && Status != ApplicationStatus.CitizenVerified)
            throw new InvalidOperationException("Invalid state for address verification");

        IsAddressVerified = isValid;
        if (!isValid)
        {
            Status = ApplicationStatus.Rejected;
        }
        else
        {
            Status = ApplicationStatus.AddressVerified;
            CheckAllVerifications();
        }
    }

    public void VerifyCitizen(bool isValid)
    {
        if (Status != ApplicationStatus.Submitted && Status != ApplicationStatus.IdVerified && Status != ApplicationStatus.AddressVerified && Status != ApplicationStatus.CitizenVerified)
            throw new InvalidOperationException("Invalid state for citizen verification");

        IsCitizenVerified = isValid;
        if (!isValid)
        {
            Status = ApplicationStatus.Rejected;
        }
        else
        {
            Status = ApplicationStatus.CitizenVerified;
            CheckAllVerifications();
        }
    }

    private void CheckAllVerifications()
    {
        if (IsIdVerified && IsAddressVerified && IsCitizenVerified)
        {
            Status = ApplicationStatus.Approved;
        }
    }
}
