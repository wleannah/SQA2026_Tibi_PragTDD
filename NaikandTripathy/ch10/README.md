# Bank4Us Account Opening Demo

This project demonstrates Test-Driven Development (TDD), finite state machines, and mutation testing for a simplified bank account opening use case.

## Project Structure

- **Bank4Us.Core**: Class library containing the account application logic with finite state machine.
- **Bank4Us.Core.Tests**: XUnit test project demonstrating TDD approach.
- **models/**: Directed graph models (GraphML and JSON) of the FSM, suitable for AltWalker and visualization with Altgraph VS Code extension.

## Features

- **Finite State Machine**: Application progresses through states: Started → PersonalInfoProvided → IdProvided → AddressProvided → DepositProvided → Submitted → [Id/Address/Citizen]Verified → Approved/Rejected
- **Step-by-Step Workflow**: Methods for providing personal info, ID, address, deposit, and verifications
- **Business Rules**: Form completeness, minimum deposit ($200), and verification failures
- **Comprehensive Unit Tests**: 22 tests covering happy path, failures, edge cases, and AltWalker model integration using xUnit v3
- **100% Code Coverage**: All lines and branches covered
- **Mutation Testing**: Stryker.NET used to validate test suite quality

## Running Tests

```bash
dotnet test
```

## Code Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:Bank4Us.Core.Tests/TestResults/*/coverage.cobertura.xml -targetdir:coverage-report
# Open coverage-report/index.html
```

## AltWalker (Model-Based Testing Framework)

AltWalker is an open‑source Model‑Based Testing (MBT) framework that allows you to write automated tests in .NET/C# or Python, and execute those tests based on paths generated from a behavioral model. It is designed specifically to support MBT workflows where models are represented as directed graphs (states + transitions).

**This project demonstrates active AltWalker integration**: The JSON model is loaded at runtime and used to generate test sequences that validate the FSM implementation.

### Key AltWalker Components Demonstrated

- **Model**: JSON file with vertices (states) and edges (transitions)
- **Generator**: "random" strategy for path generation
- **Planner**: Uses model + generator to create test sequences
- **Executor**: Maps model transitions to `AccountApplication` method calls
- **Reporter**: Test results and coverage analysis

### FSM Model Files

The FSM is defined in graphs in the `models/` directory:
- **`models/bank4us-account-fsm.graphml`**: GraphML format; open with [AltWalker Model Visualizer](https://marketplace.visualstudio.com/items?itemName=altom.altwalker-model-visualizer) extension.
- **`models/bank4us-account-fsm-altwalker.json`**: AltWalker-specific JSON format; actively used by the test suite for model-based testing.

The JSON model is loaded by `AltWalkerAdapter` and used to generate test sequences that validate the `AccountApplication` implementation against the formal FSM specification.

## Mutation Testing

Mutation testing is set up using Stryker.NET to validate test suite quality.

**Latest Results (February 21, 2026)**:
- **Mutation Score**: 84.48% (98 killed, 9 survived, 0 timeout)
- **Total Mutants**: 128 created, 107 tested
- **Test Coverage**: 100% line/branch coverage maintained

To run mutation tests:

```bash
cd Bank4Us.Core.Tests
dotnet stryker
```

The mutation score indicates strong test quality, with most code changes being detected by the test suite. The surviving mutants represent areas where additional test cases could provide even stronger validation.

## FSM States and Transitions

- **Started**: Initial state
- **PersonalInfoProvided**: After providing personal details
- **IdProvided**: After providing ID and photo
- **AddressProvided**: After providing proof of address
- **DepositProvided**: After providing deposit >= $200
- **Submitted**: After submitting complete application
- **IdVerified/AddressVerified/CitizenVerified**: After successful verifications
- **Approved**: All verifications pass
- **Rejected**: Any verification fails

## Key Items When Using FSMs as a Test Strategy

- **FSM testing verifies conformance to a behavioral model**: Treat the FSM as the specification and verify the implementation follows it.
- **Generate sequences of transitions from the FSM**: Create test sequences that exercise valid and invalid paths, covering both success and alternate flows.
- **Provide inputs to drive the system**: Use concrete input values that trigger transitions (e.g., valid/invalid IDs, deposit amounts).
- **Execute transitions on the implementation**: Apply the generated sequences to the system under test and advance its state.
- **Check expected outputs against actual outputs**: After each transition, assert the expected next state and any output or side effect.
- **Handle unexpected outputs gracefully**: Tests should assert failures clearly and avoid cascading errors; use meaningful assertions and cleanup.
- **Use timers/timeouts to avoid deadlock**: When tests interact with async or external systems, enforce timeouts to prevent hung tests.

## Test Categories

The test suite includes multiple categories of tests:

### Unit Tests (18 tests)
- **Happy Path**: Complete successful application flow
- **Failure Cases**: Individual verification failures
- **Edge Cases**: Invalid operations, state transitions
- **Business Rules**: Deposit requirements, form completeness

### Model-Based Tests (4 tests)
- **AltWalker Integration**: JSON model loading and validation
- **Sequence Generation**: Path generation from FSM model
- **State Coverage**: Verification of terminal state reachability
- **Model-Implementation Alignment**: Consistency between model and code

### Key Test: `GeneratedSequences_CoverSubmittedAndVerificationCombos`
This test demonstrates AltWalker's core capability: generating test sequences from the model and validating all verification combinations after application submission.

## Requirements Coverage

- AC-1: Happy path - all verifications pass, application approved
- AC-2: ID verification failure - application rejected
- AC-3: Address verification failure - application rejected
- AC-4: Citizen verification failure - application rejected
- AC-5: Incomplete application cannot proceed

Alternate flows for missing/invalid documents are handled by the verification failures.