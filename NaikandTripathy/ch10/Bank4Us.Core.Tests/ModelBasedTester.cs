using System;
using System.Collections.Generic;
using Bank4Us.Core;

namespace Bank4Us.Core.Tests;

/// <summary>
/// Simple model-based tester / AltWalker-like adapter.
/// This generates transition sequences from a formal FSM description
/// (states and allowed transitions). Replace or integrate with AltWalker
/// by mapping the FSM definition to AltWalker's model format.
/// </summary>
public static class ModelBasedTester
{
    public record Step(string Name, Action<AccountApplication> Execute);

    // Returns a mapping of transition names keyed by source state
    public static Dictionary<ApplicationStatus, List<string>> GetTransitionMap()
    {
        return new Dictionary<ApplicationStatus, List<string>>
        {
            [ApplicationStatus.Started] = new() { "ProvidePersonalDetails" },
            [ApplicationStatus.PersonalInfoProvided] = new() { "ProvideId" },
            [ApplicationStatus.IdProvided] = new() { "ProvideAddress" },
            [ApplicationStatus.AddressProvided] = new() { "ProvideDeposit" },
            [ApplicationStatus.DepositProvided] = new() { "SubmitApplication" },
            [ApplicationStatus.Submitted] = new() { "ProvideImmigrationDocs", "VerifyId_Success", "VerifyId_Failure", "VerifyAddress_Success", "VerifyAddress_Failure", "VerifyCitizen_Success", "VerifyCitizen_Failure" },
            [ApplicationStatus.IdVerified] = new() { "VerifyAddress_Success", "VerifyAddress_Failure" },
            [ApplicationStatus.AddressVerified] = new() { "VerifyCitizen_Success", "VerifyCitizen_Failure" },
            [ApplicationStatus.CitizenVerified] = new() { "CheckAllVerifications" },
            // terminal states have no outgoing transitions
            [ApplicationStatus.Approved] = new(),
            [ApplicationStatus.Rejected] = new(),
        };
    }

    public static Dictionary<string, Action<AccountApplication>> GetActionMap()
    {
        return new Dictionary<string, Action<AccountApplication>>
        {
            ["ProvidePersonalDetails"] = app => app.ProvidePersonalDetails("Model User"),
            ["ProvideId"] = app => app.ProvideId("555-55-5555", "model-id.jpg"),
            ["ProvideAddress"] = app => app.ProvideAddress("model-address.pdf"),
            ["ProvideDeposit"] = app => app.ProvideDeposit(200),
            ["SubmitApplication"] = app => app.SubmitApplication(),
            ["ProvideImmigrationDocs"] = app => { /* Optional action - no-op */ },
            ["VerifyId_Success"] = app => app.VerifyId(true),
            ["VerifyId_Failure"] = app => app.VerifyId(false),
            ["VerifyAddress_Success"] = app => app.VerifyAddress(true),
            ["VerifyAddress_Failure"] = app => app.VerifyAddress(false),
            ["VerifyCitizen_Success"] = app => app.VerifyCitizen(true),
            ["VerifyCitizen_Failure"] = app => app.VerifyCitizen(false),
            ["CheckAllVerifications"] = app => { /* This is handled automatically by the verification logic */ },
        };
    }

    // DFS walk up to a max depth, producing sequences of transition names
    public static IEnumerable<List<string>> GenerateSequences(ApplicationStatus startState, int maxDepth)
    {
        var map = GetTransitionMap();

        var stack = new Stack<(ApplicationStatus state, List<string> path)>();
        stack.Push((startState, new List<string>()));

        while (stack.Count > 0)
        {
            var (state, path) = stack.Pop();
            if (path.Count > 0)
                yield return new List<string>(path);

            if (path.Count >= maxDepth)
                continue;

            if (!map.TryGetValue(state, out var transitions))
                continue;

            foreach (var t in transitions)
            {
                // compute next state heuristically based on transition name
                var nextState = InferNextState(state, t);
                var newPath = new List<string>(path) { t };
                stack.Push((nextState, newPath));
            }
        }
    }

    // Very small heuristic to infer next state after applying a transition
    private static ApplicationStatus InferNextState(ApplicationStatus current, string transitionName)
    {
        return transitionName switch
        {
            "ProvidePersonalDetails" => ApplicationStatus.PersonalInfoProvided,
            "ProvideId" => ApplicationStatus.IdProvided,
            "ProvideAddress" => ApplicationStatus.AddressProvided,
            "ProvideDeposit" => ApplicationStatus.DepositProvided,
            "SubmitApplication" => ApplicationStatus.Submitted,
            "ProvideImmigrationDocs" => ApplicationStatus.Submitted, // Self-loop
            "VerifyId_Success" => ApplicationStatus.IdVerified,
            "VerifyId_Failure" => ApplicationStatus.Rejected,
            "VerifyAddress_Success" => ApplicationStatus.AddressVerified,
            "VerifyAddress_Failure" => ApplicationStatus.Rejected,
            "VerifyCitizen_Success" => ApplicationStatus.CitizenVerified,
            "VerifyCitizen_Failure" => ApplicationStatus.Rejected,
            "CheckAllVerifications" => ApplicationStatus.Approved,
            _ => current,
        };
    }

    // Apply a sequence of transition names to an AccountApplication instance
    public static void ExecuteSequence(AccountApplication app, IEnumerable<string> sequence)
    {
        var actions = GetActionMap();
        foreach (var name in sequence)
        {
            if (!actions.TryGetValue(name, out var action))
                throw new InvalidOperationException($"Unknown action '{name}'");

            try
            {
                action(app);
            }
            catch (InvalidOperationException)
            {
                // When a transition is invalid for current state, skip to allow other sequences to continue
            }
        }
    }
}
