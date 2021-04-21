Feature: Sign up page

As a I user I would like to sign up

Scenario: Not registered
Given I am not registered
When I sign up with my username and pass
Then I should receive verification code
# When I supply my verification code
# Then I should be registered
