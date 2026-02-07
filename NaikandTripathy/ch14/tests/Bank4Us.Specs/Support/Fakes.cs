
using Bank4Us.Domain.Models;
using Bank4Us.Domain.Services;

namespace Bank4Us.Specs.Support;

public sealed class SpyCoreBankingSystem : ICoreBankingSystem
{
    public int CreateCalls { get; private set; }
    public void CreateAccount(Applicant applicant) => CreateCalls++;
}

public sealed class ConfigurableResidencyVerificationService : IResidencyVerificationService
{
    public bool ThrowOnVerify { get; set; }
    public bool Verify(Applicant applicant)
    {
        if (ThrowOnVerify) throw new TimeoutException("Service unavailable");
        return true;
    }
}
