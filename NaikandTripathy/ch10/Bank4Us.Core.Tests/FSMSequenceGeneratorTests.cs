using System;
using System.Collections.Generic;
using Bank4Us.Core;

namespace Bank4Us.Core.Tests;

/*
 Programmatic FSM sequence generator tests

 This file demonstrates how to generate transition sequences from the FSM
 model and drive the implementation programmatically. It produces sequences
 that reach the Submitted state and then runs all combinations of verification
 outcomes (Id/Address/Citizen) to validate the final state (Approved or Rejected).
*/

public class FSMSequenceGeneratorTests
{
    private record Step(string Name, Action<AccountApplication> Execute);

    private static IEnumerable<Step> GetBaseSequenceSteps()
    {
        yield return new Step("ProvidePersonalDetails", app => app.ProvidePersonalDetails("Auto User"));
        yield return new Step("ProvideId", app => app.ProvideId("999-99-9999", "auto-id.jpg"));
        yield return new Step("ProvideAddress", app => app.ProvideAddress("auto-address.pdf"));
        yield return new Step("ProvideDeposit", app => app.ProvideDeposit(200));
        yield return new Step("SubmitApplication", app => app.SubmitApplication());
    }

    private static IEnumerable<bool[]> GetVerificationCombinations()
    {
        for (int i = 0; i < 8; i++)
        {
            yield return new[] { (i & 1) != 0, (i & 2) != 0, (i & 4) != 0 };
        }
    }

    [Fact]
    public void Generator_ProducesSequences_AndFinalStatesMatchExpectations()
    {
        var baseSteps = new List<Step>(GetBaseSequenceSteps());
        var combos = GetVerificationCombinations();
        var executed = 0;

        foreach (var combo in combos)
        {
            var app = new AccountApplication();

            // Execute base steps to reach Submitted
            foreach (var s in baseSteps)
                s.Execute(app);

            // Now execute verifications in order: Id, Address, Citizen
            var idOk = combo[0];
            var addrOk = combo[1];
            var citOk = combo[2];

            // Execute and catch any invalid state exceptions (some sequences will short-circuit to Rejected)
            try
            {
                app.VerifyId(idOk);
            }
            catch (InvalidOperationException) { }

            try
            {
                app.VerifyAddress(addrOk);
            }
            catch (InvalidOperationException) { }

            try
            {
                app.VerifyCitizen(citOk);
            }
            catch (InvalidOperationException) { }

            executed++;

            // Determine expected final state: if all three are true -> Approved, otherwise Rejected
            var expected = (idOk && addrOk && citOk) ? ApplicationStatus.Approved : ApplicationStatus.Rejected;

            Assert.Equal(expected, app.Status);
        }

        Assert.True(executed > 0, "No sequences executed");
    }

    [Fact]
    public void Generator_SequenceCount_IsAsExpected()
    {
        var count = 0;
        foreach (var _ in GetVerificationCombinations()) count++;
        Assert.Equal(8, count);
    }
}
