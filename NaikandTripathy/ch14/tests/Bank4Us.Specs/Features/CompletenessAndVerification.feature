
Feature: Application completeness and verification

  Rule: Application must be complete (ID required)
  Scenario: Missing identification number cancels the application
    Given a valid applicant with identifier type SSN
    And the applicant omits the identification number
    When the application is processed
    Then the application status should be Cancelled
    And the error message should contain "ID"

  Scenario: Empty identification number cancels the application
    Given a valid applicant with identifier type SSN
    And the applicant provides an empty identification number
    When the application is processed
    Then the application status should be Cancelled
    And the error message should contain "ID"

  Rule: Residency verification service unavailable -> PendingVerification
  Scenario: Residency service timeout should not create an account
    Given a valid applicant with residency documentation
    And the residency verification service is unavailable
    When the application is processed
    Then the application status should be PendingVerification
    And no core banking account should be created

  Rule: Address completeness
  Scenario Outline: Missing required address field makes the application incomplete
    Given a valid applicant with a complete address
    And the applicant address is missing "<field>"
    When the address is validated
    Then the application status should be Incomplete
    And the error message should contain "<field>"

    Examples:
      | field      |
      | Street     |
      | City       |
      | State      |
      | PostalCode |

  Rule: Citizenship partitions
  Scenario Outline: Citizenship status drives the next state
    Given a valid applicant with citizenship status "<citizenship>"
    When citizenship is evaluated
    Then the citizenship decision should be "<decision>"

    Examples:
      | citizenship        | decision              |
      | Citizen            | Proceed               |
      | PermanentResident  | NeedsExtraVerification|
      | Unknown            | Incomplete            |
