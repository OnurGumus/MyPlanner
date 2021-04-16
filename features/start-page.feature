Feature: Start page

As a I user I would like to visit the start page

Scenario: Not logged in
Given I am not logged in
When I visit the start page
Then I should be redirect to signin page


# Scenario: Click to Signup
# Given I am not logged in
# When I visit the login page
# And I click to signup link
# Then I should be at signup page

