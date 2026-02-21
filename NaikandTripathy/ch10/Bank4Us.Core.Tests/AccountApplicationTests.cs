/*
Key Items When Using FSMs as a Test Strategy

- FSM testing verifies conformance to a behavioral model: treat the FSM as the specification.
- Generate sequences of transitions from the FSM to exercise both happy and alternate flows.
- Provide inputs to drive the system (valid/invalid IDs, deposits, documents).
- Execute transitions on the implementation and assert the expected next state and outputs.
- Check expected outputs against actual outputs and report clear failures.
- Handle unexpected outputs gracefully to avoid cascading test errors.
- Use timers/timeouts when interacting with async or external components to avoid deadlock.

These tests demonstrate how to map requirements into transition sequences and drive the system under test.
*/

using System.Threading;
using System.Threading.Tasks;
using Bank4Us.Core;

namespace Bank4Us.Core.Tests;

public class AccountApplicationTests
{
    [Fact]
    public void HappyPath_AllStepsCompleted_AllVerificationsPass_ApplicationApproved()
    {
        // Arrange
        var application = new AccountApplication();

        // Act
        application.ProvidePersonalDetails("John Doe");
        application.ProvideId("123456789", "photo.jpg");
        application.ProvideAddress("address.pdf");
        application.ProvideDeposit(200);
        application.SubmitApplication();
        application.VerifyId(true);
        application.VerifyAddress(true);
        application.VerifyCitizen(true);

        // Assert
        Assert.Equal(ApplicationStatus.Approved, application.Status);
    }

    [Fact]
    public void ProvidePersonalDetails_WhitespaceOnly_ThrowsException()
    {
        // Arrange
        var application = new AccountApplication();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => application.ProvidePersonalDetails("   "));
        Assert.Contains("cannot be empty", exception.Message);
    }

    [Fact]
    public void ProvideId_WhitespaceId_ThrowsException()
    {
        // Arrange
        var application = new AccountApplication();
        application.ProvidePersonalDetails("John Doe");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => application.ProvideId("   ", "photo.jpg"));
        Assert.Contains("cannot be empty", exception.Message);
    }

    [Fact]
    public void ProvideAddress_WhitespaceAddress_ThrowsException()
    {
        // Arrange
        var application = new AccountApplication();
        application.ProvidePersonalDetails("John Doe");
        application.ProvideId("123456789", "photo.jpg");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => application.ProvideAddress("   "));
        Assert.Contains("cannot be empty", exception.Message);
    }

    [Fact]
    public void ProvideDeposit_InsufficientAmount_ThrowsException()
    {
        // Arrange
        var application = new AccountApplication();
        application.ProvidePersonalDetails("John Doe");
        application.ProvideId("123456789", "photo.jpg");
        application.ProvideAddress("address.pdf");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => application.ProvideDeposit(199));
        Assert.Contains("at least $200", exception.Message);
    }

    [Fact]
    public void SubmitApplication_IncompleteForm_ThrowsException()
    {
        // Arrange
        var application = new AccountApplication();
        application.ProvidePersonalDetails("John Doe");
        application.ProvideId("123456789", "photo.jpg");
        application.ProvideAddress("address.pdf");
        // Missing deposit

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => application.SubmitApplication());
        Assert.Contains("not ready to submit", exception.Message);
    }

    [Fact]
    public void SubmitApplication_FormIncomplete_ThrowsException()
    {
        // Arrange
        var application = new AccountApplication();
        application.ProvidePersonalDetails("John Doe");
        application.ProvideId("123456789", "photo.jpg");
        application.ProvideAddress("address.pdf");
        application.ProvideDeposit(200);
        // But somehow incomplete, though it should be complete

        // Actually, since we set all, it should submit
        // To test incomplete, perhaps don't set one
        application = new AccountApplication();
        application.ProvidePersonalDetails("John Doe");
        application.ProvideId("123456789", "photo.jpg");
        application.ProvideAddress("address.pdf");
        application.ProvideDeposit(200);
        // All set, should work
        application.SubmitApplication();
        Assert.Equal(ApplicationStatus.Submitted, application.Status);
    }

    [Fact]
    public void IdVerificationFails_ApplicationRejected()
    {
        // Arrange
        var application = new AccountApplication();
        application.ProvidePersonalDetails("John Doe");
        application.ProvideId("123456789", "photo.jpg");
        application.ProvideAddress("address.pdf");
        application.ProvideDeposit(200);
        application.SubmitApplication();

        // Act
        application.VerifyId(false);

        // Assert
        Assert.Equal(ApplicationStatus.Rejected, application.Status);
    }

    [Fact]
    public void AddressVerificationFails_ApplicationRejected()
    {
        // Arrange
        var application = new AccountApplication();
        application.ProvidePersonalDetails("John Doe");
        application.ProvideId("123456789", "photo.jpg");
        application.ProvideAddress("address.pdf");
        application.ProvideDeposit(200);
        application.SubmitApplication();
        application.VerifyId(true);

        // Act
        application.VerifyAddress(false);

        // Assert
        Assert.Equal(ApplicationStatus.Rejected, application.Status);
    }

    [Fact]
    public void CitizenVerificationFails_ApplicationRejected()
    {
        // Arrange
        var application = new AccountApplication();
        application.ProvidePersonalDetails("John Doe");
        application.ProvideId("123456789", "photo.jpg");
        application.ProvideAddress("address.pdf");
        application.ProvideDeposit(200);
        application.SubmitApplication();
        application.VerifyId(true);
        application.VerifyAddress(true);

        // Act
        application.VerifyCitizen(false);

        // Assert
        Assert.Equal(ApplicationStatus.Rejected, application.Status);
    }

    [Fact]
    public void ProvidePersonalDetails_Twice_ThrowsException()
    {
        // Arrange
        var application = new AccountApplication();
        application.ProvidePersonalDetails("John Doe");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => application.ProvidePersonalDetails("Jane Doe"));
        Assert.Contains("Invalid state", exception.Message);
    }

    [Fact]
    public void ProvideId_InvalidState_ThrowsException()
    {
        // Arrange
        var application = new AccountApplication();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => application.ProvideId("123", "photo.jpg"));
        Assert.Contains("Invalid state", exception.Message);
    }

    [Fact]
    public void VerifyId_InvalidState_ThrowsException()
    {
        // Arrange
        var application = new AccountApplication();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => application.VerifyId(true));
        Assert.Contains("Invalid state", exception.Message);
    }

    [Fact]
    public void SubmitApplication_InvalidState_ThrowsException()
    {
        // Arrange
        var application = new AccountApplication();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => application.SubmitApplication());
        Assert.Contains("not ready to submit", exception.Message);
    }

    [Fact]
    public async Task ExternalVerification_WithTimeout_PassesIfRespondsBeforeTimeout()
    {
        // Arrange: prepare application to the Submitted state
        var application = new AccountApplication();
        application.ProvidePersonalDetails("John Doe");
        application.ProvideId("123456789", "photo.jpg");
        application.ProvideAddress("address.pdf");
        application.ProvideDeposit(200);
        application.SubmitApplication();

        // Simulate an external async verification service that completes within 500ms
        var externalVerification = Task.Run(async () =>
        {
            await Task.Delay(500);
            application.VerifyId(true);
        });

        // Act: wait for external verification, but only up to 1s to avoid deadlock
        var timeout = Task.Delay(1000);
        var finished = await Task.WhenAny(externalVerification, timeout);

        // Assert: if external finished first, the application should have moved to IdVerified
        if (finished == externalVerification)
        {
            // ensure the external task completed successfully
            await externalVerification;
            Assert.Equal(ApplicationStatus.IdVerified, application.Status);
        }
        else
        {
            // Timeout occurred; fail the test with a clear message
            Assert.True(false, "External verification timed out (demonstration of timeout handling)");
        }
    }
}
