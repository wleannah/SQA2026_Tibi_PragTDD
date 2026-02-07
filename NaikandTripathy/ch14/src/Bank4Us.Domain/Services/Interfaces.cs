
using Bank4Us.Domain.Models;

namespace Bank4Us.Domain.Services;

public interface IResidencyVerificationService
{
    bool Verify(Applicant applicant);
}

public interface ICoreBankingSystem
{
    void CreateAccount(Applicant applicant);
}
