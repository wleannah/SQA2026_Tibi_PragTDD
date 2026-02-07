
Feature: Opening deposit rules
  Rule: Minimum deposit must meet product rules

  Scenario Outline: Deposit amount is evaluated against the minimum
    Given a valid applicant for a product that requires a minimum opening deposit of 200
    And the applicant enters an opening deposit of <deposit>
    When the application is processed
    Then the application status should be <status>
    And the error message should be <error>

    Examples:
      | deposit | status    | error                  |
      | 199     | Cancelled | Deposit below minimum  |
      | 200     | Approved  | (none)                 |
      | 201     | Approved  | (none)                 |

  Scenario: Missing deposit should be incomplete
    Given a valid applicant for a product that requires a minimum opening deposit of 200
    And the applicant does not provide an opening deposit
    When the application is processed
    Then the application status should be Incomplete
    And the error message should be Opening deposit is required
