
Feature: Identification number validation
  Rule: SSN format must be valid

  Scenario Outline: Invalid SSN formats are rejected with a specific error
    Given a valid applicant with identifier type SSN
    And the applicant provides identification number "<idNumber>"
    When the identification number is validated
    Then the validation error should be "Invalid SSN format"

    Examples:
      | idNumber     |
      | 123          |
      | 123-45-678   |
      | ABC-DE-FGHI  |

  Scenario: Valid SSN format yields no validation error
    Given a valid applicant with identifier type SSN
    And the applicant provides identification number "123-45-6789"
    When the identification number is validated
    Then there should be no validation error
