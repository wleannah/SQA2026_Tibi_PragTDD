
using Bank4Us.Domain.Models;
using Bank4Us.Domain.Workflow;
using Bank4Us.Specs.Support;
using Xunit;
using Reqnroll;

namespace Bank4Us.Specs.StepDefinitions;

[Binding]
public sealed class NewAccountSteps
{
    private Applicant _applicant = null!;
    private ApplicationResult? _result;
    private string? _validationError;

    private SpyCoreBankingSystem _core = null!;
    private ConfigurableResidencyVerificationService _residency = null!;

    [Given("a valid applicant for a product that requires a minimum opening deposit of {int}")]
    public void GivenApplicantWithDepositMinimum(int minDeposit)
    {
        _core = new SpyCoreBankingSystem();
        _residency = new ConfigurableResidencyVerificationService();

        _applicant = new Applicant
        {
            IdentificationNumber = "123-45-6789",
            IdentifierType = IdentifierType.Ssn,
            Product = new AccountProduct("Everyday Checking", true, minDeposit)
        };
    }

    [Given("a valid applicant with identifier type SSN")]
    public void GivenValidApplicantWithSsn()
    {
        _core = new SpyCoreBankingSystem();
        _residency = new ConfigurableResidencyVerificationService();

        _applicant = new Applicant
        {
            IdentifierType = IdentifierType.Ssn,
            IdentificationNumber = "123-45-6789"
        };
    }

    [Given("a valid applicant with residency documentation")]
    public void GivenValidApplicantWithResidencyDoc()
    {
        _core = new SpyCoreBankingSystem();
        _residency = new ConfigurableResidencyVerificationService();

        _applicant = new Applicant
        {
            IdentifierType = IdentifierType.Ssn,
            IdentificationNumber = "123-45-6789",
            HasResidencyDocumentation = true,
            OpeningDepositAmount = 200m
        };
    }

    [Given(@"the applicant enters an opening deposit of {int}")]
    public void GivenDeposit(int deposit) => _applicant.OpeningDepositAmount = deposit;

    [Given("the applicant does not provide an opening deposit")]
    public void GivenNoDeposit() => _applicant.OpeningDepositAmount = null;

    [Given(@"the applicant provides identification number ""(.*)""")]
    public void GivenIdentificationNumber(string idNumber) => _applicant.IdentificationNumber = idNumber;

    [Given("the applicant omits the identification number")]
    public void GivenOmitId() => _applicant.IdentificationNumber = null;

    [Given("the applicant provides an empty identification number")]
    public void GivenEmptyId() => _applicant.IdentificationNumber = string.Empty;

    [Given("the residency verification service is unavailable")]
    public void GivenResidencyUnavailable() => _residency.ThrowOnVerify = true;

    [When("the application is processed")]
    public void WhenProcess()
    {
        var wf = new NewAccountWorkflow(_residency, _core);
        _result = wf.Process(_applicant);
    }

    [When("the identification number is validated")]
    public void WhenValidateId()
    {
        _validationError = NewAccountWorkflow.ValidateIdentificationNumber(_applicant);
    }

    [When("the address is validated")]
    public void WhenValidateAddress()
    {
        // Use the same validation method the workflow uses
        var err = NewAccountWorkflow.ValidateAddress(_applicant);
        _result = err is null ? ApplicationResult.Approved() : ApplicationResult.Incomplete(err);
    }

    [When("citizenship is evaluated")]
    public void WhenEvaluateCitizenship()
    {
        var decision = NewAccountWorkflow.EvaluateCitizenship(_applicant);
        _validationError = decision.ToString();
    }

    [Then(@"the application status should be (.*)")]
    public void ThenStatus(string status)
    {
        Assert.NotNull(_result);
        Assert.Equal(Enum.Parse<ApplicationStatus>(status), _result!.Status);
    }

    [Then(@"the error message should be (.*)")]
    public void ThenErrorMessage(string expected)
    {
        Assert.NotNull(_result);
        if (expected == "(none)")
        {
            Assert.Empty(_result!.Errors);
            return;
        }

        Assert.NotEmpty(_result!.Errors);
        Assert.Equal(expected, _result!.Errors[0]);
    }


    [Then(@"the error message should contain ""(.*)""")]
    public void ThenErrorContains(string fragment)
    {
        Assert.NotNull(_result);
        var combined = string.Join(" | ", _result!.Errors);
        Assert.Contains(fragment, combined);
    }

    
[Then("no core banking account should be created")]
public void ThenNoAccountCreated() => Assert.Equal(0, _core.CreateCalls);


    [Given("a valid applicant with a complete address")]
    public void GivenValidApplicantWithAddress()
    {
        _core = new SpyCoreBankingSystem();
        _residency = new ConfigurableResidencyVerificationService();

        _applicant = new Applicant
        {
            IdentifierType = IdentifierType.Ssn,
            IdentificationNumber = "123-45-6789",
            Address = new Address("1 Main", "Milwaukee", "WI", "53202"),
            OpeningDepositAmount = 200m
        };
    }

    [Given(@"the applicant address is missing ""(.*)""")]
    public void GivenAddressMissing(string field)
    {
        _applicant.Address = field switch
        {
            "Street" => _applicant.Address with { Street = null },
            "City" => _applicant.Address with { City = null },
            "State" => _applicant.Address with { State = null },
            "PostalCode" => _applicant.Address with { PostalCode = null },
            _ => _applicant.Address
        };
    }

    [Given(@"a valid applicant with citizenship status ""(.*)""")]
    public void GivenCitizenship(string citizenship)
    {
        _core = new SpyCoreBankingSystem();
        _residency = new ConfigurableResidencyVerificationService();

        _applicant = new Applicant
        {
            IdentifierType = IdentifierType.Ssn,
            IdentificationNumber = "123-45-6789",
            CitizenshipStatus = Enum.Parse<CitizenshipStatus>(citizenship)
        };
    }

    [Then(@"the citizenship decision should be ""(.*)""")]
    public void ThenCitizenshipDecision(string expected)
    {
        Assert.Equal(expected, _validationError);
    }

    [Then(@"the validation error should be ""(.*)""")]
    public void ThenValidationError(string expected)
    {
        Assert.Equal(expected, _validationError);
    }

    [Then("there should be no validation error")]
    public void ThenNoValidationError() => Assert.Null(_validationError);
}
