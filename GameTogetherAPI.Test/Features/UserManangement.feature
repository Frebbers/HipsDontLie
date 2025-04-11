Feature: UserManagement
This feature file describes the process of logging in and managing one's user account.

    Background:
#Given the API is initialized
        Given TestUser is reset
        And I send a create account request
        Then I assert that the account is created
    

   # Scenario: Create account and log in

    Scenario: log in
        When I send a log in request
		Then I assert that the account is logged in

    Scenario: Log off
        Given I am logged in
        Then I am no longer logged in

    Scenario: Group request accepted
        Given I am logged in
        And i create a second user  
        And a group has been created
        When I send a join request
        Then I join a group
    
#    Scenario: Group request declined
#        Given I am logged in
#        When I send a join request
#        And the join request is rejected
#        Then The group stays the same

#     Scenario: Leaving a group
#        Given I am logged in
#        And part of a group        
        
#    Scenario: Tag assignment for my account
#        Given I am logged in
#        And I assign a tag {string} to my account
#        Then the tag {string} should be attatched to my account
#
#    Scenario: Tag assignment for my group
#        Given I am logged in 
#        And I assign a tag {string} to my group
#        Then the tag {string} is found in the group
