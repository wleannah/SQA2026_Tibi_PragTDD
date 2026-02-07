
using Bank4Us.Domain.Models;

namespace Bank4Us.Domain.Services;

public sealed class AlwaysPassResidencyVerificationService : IResidencyVerificationService
{
    public bool Verify(Applicant applicant) => true;
}

public sealed class InMemoryCoreBankingSystem : ICoreBankingSystem
{
    public int CreatedAccountsCount { get; private set; }
    public void CreateAccount(Applicant applicant) => CreatedAccountsCount++;
}
