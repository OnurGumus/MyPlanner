Feature: Show tasks

As a I user I would like to list tasks.

Scenario: Visit tasks
Given N is a positive integer
And there are N tasks in the system
When I visit url /tasks
Then I should see N tasks listed