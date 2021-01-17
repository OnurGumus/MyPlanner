Feature: Start page

As a I user I would like to visit the start page

Scenario: _Visit start page
Given there is 1 task on the system
When I visit the start page
Then I should be redirect to /tasks
And I should see 1 task(s) listed