Feature: Create tasks

As a I user I would like to create tasks.

Scenario: Visit tasks
Given there are no tasks in the system
When I create a task
Then the task should be created successfully
When I visit url /tasks
Then I should see 1 task(s) listed