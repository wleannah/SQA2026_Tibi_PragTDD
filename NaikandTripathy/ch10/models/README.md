# Bank4Us Account Opening FSM Model

This directory contains directed graph models of the Bank4Us account opening Finite State Machine (FSM), suitable for Model-Based Testing (MBT) with AltWalker and visualization in VS Code.

## Files

### `bank4us-account-fsm.graphml`
GraphML format model of the FSM. This format is widely supported by graph visualization tools including the AltWalker Model Visualizer extension.

**Usage:**
- Open in VS Code with [AltWalker Model Visualizer](https://marketplace.visualstudio.com/items?itemName=altom.altwalker-model-visualizer) extension.
- Use Command Palette: `Ctrl+Shift+P` → "AltWalker: Open Model" to visualize.
- Use with AltWalker to generate test sequences based on the model.

### `bank4us-account-fsm-altwalker.json`
AltWalker-specific JSON format model. This is the correct format for AltWalker's model visualizer and executor.

**Usage:**
- Open in VS Code with [AltWalker Model Visualizer](https://marketplace.visualstudio.com/items?itemName=altom.altwalker-model-visualizer) extension.
- Use Command Palette: `Ctrl+Shift+P` → "AltWalker: Open Model" to visualize.
- Import directly into AltWalker for test generation and execution.
- Contains the required "generator": "random" field for AltWalker compatibility.
- Contains the required "startingElementId": "Started" field to specify the initial state.

## FSM Overview

The model represents the Bank4Us account opening process as a directed graph:

### States

- **Started**: Initial state; process begins.
- **PersonalInfoProvided**: User has entered personal details (name, passport, DOB).
- **IdProvided**: Photo ID document has been provided.
- **AddressProvided**: Proof of address document has been provided.
- **DepositProvided**: Opening deposit >= $200 has been provided.
- **Submitted**: Complete application has been submitted for verification.
- **IdVerified**: ID verification passed successfully.
- **AddressVerified**: Address verification passed successfully.
- **CitizenVerified**: Citizen/immigration verification passed successfully.
- **Approved**: ✓ All verifications passed; account opening approved.
- **Rejected**: ✗ Any verification failed; application rejected.

### Transitions

The model defines transitions such as:
- **ProvidePersonalDetails**: Started → PersonalInfoProvided
- **ProvideId**: PersonalInfoProvided → IdProvided
- **ProvideAddress**: IdProvided → AddressProvided
- **ProvideDeposit**: AddressProvided → DepositProvided
- **SubmitApplication**: DepositProvided → Submitted
- **VerifyId_Success**: Submitted → IdVerified
- **VerifyId_Failure**: Submitted → Rejected
- **VerifyAddress_Success**: IdVerified → AddressVerified
- **VerifyAddress_Failure**: IdVerified → Rejected
- **VerifyCitizen_Success**: AddressVerified → CitizenVerified
- **VerifyCitizen_Failure**: AddressVerified → Rejected
- **CheckAllVerifications**: CitizenVerified → Approved
- **ProvideImmigrationDocs**: Submitted → Submitted (self-loop for optional docs)

## Using with AltWalker

1. **Generate Test Sequences**: Use AltWalker with this model to automatically generate test sequences covering various paths through the FSM.
2. **Execute Sequences**: Execute generated sequences against the `AccountApplication` implementation in `Bank4Us.Core`.
3. **Validate Outcomes**: Assert that the application reaches expected states after each sequence.

### Example: Generating Paths

AltWalker can generate paths such as:
- Happy path: Started → PersonalInfoProvided → IdProvided → AddressProvided → DepositProvided → Submitted → IdVerified → AddressVerified → CitizenVerified → Approved
- ID failure: Started → ... → Submitted → Rejected
- Address failure: Started → ... → IdVerified → Rejected
- Citizen failure: Started → ... → AddressVerified → Rejected

## Using with Altgraph (VS Code Extension)

1. Open `bank4us-account-fsm.graphml` in VS Code.
2. Install [Altgraph](https://marketplace.visualstudio.com/items?itemName=tintinweb.graphviz-interactive-preview) extension if not already installed.
3. The graph will render as an interactive diagram showing all states and transitions.
4. Use this visual representation to:
   - Teach students the FSM structure and workflow.
   - Identify untested paths through manual inspection.
   - Verify that generated test sequences exercise the important paths.

## Integration with Bank4Us.Core.Tests

The `ModelBasedTester` and `AltWalkerAdapter` in the test project consume the transition/state definitions from the implementation (`AccountApplication.cs`) and can be aligned with this model to ensure consistency.

### Alignment Checklist

- [ ] All states in the model correspond to values in `ApplicationStatus` enum.
- [ ] All transitions in the model correspond to public methods in `AccountApplication`.
- [ ] Happy path coverage: Does a sequence from Started to Approved exist?
- [ ] Failure paths: Do sequences to Rejected exist for each verification failure?
- [ ] Edge cases: Are optional steps (e.g., ProvideImmigrationDocs) captured?

## Integration with Tests

The AltWalker JSON model is now actively used in the test suite:

- **`AltWalkerAdapter`** loads and parses the JSON model file
- **Test sequences** are generated based on the model's vertices and edges
- **Action mapping** connects model transitions to `AccountApplication` methods
- **Model validation** ensures the JSON structure matches the implementation

### Test Coverage

The test suite includes specific AltWalker integration tests:

- `AltWalkerModel_IsLoadedCorrectly`: Verifies JSON model loading and structure
- `AltWalkerModel_StructureMatchesImplementation`: Ensures model transitions have corresponding actions
- `AltWalkerSequences_ReachExpectedStates`: Validates generated sequences reach terminal states
- `AltWalkerModelBasedTest_Integration`: Full integration test with sequence execution

### Model vs Implementation Alignment

| JSON Model Element | Implementation Mapping |
|-------------------|------------------------|
| `vertices` (states) | `ApplicationStatus` enum |
| `edges` (transitions) | `ModelBasedTester.GetActionMap()` |
| `startElementId` | `ApplicationStatus.Started` |
| `generator: "random"` | Sequence generation strategy |

## References

- [AltWalker Documentation](https://altwalker.github.io/altwalker/)
- [GraphML Format](http://graphml.graphdrawing.org/)
- [Altgraph VS Code Extension](https://marketplace.visualstudio.com/items?itemName=tintinweb.graphviz-interactive-preview)
- Bank4Us Project: [Bank4Us.Core/AccountApplication.cs](../Bank4Us.Core/AccountApplication.cs)
