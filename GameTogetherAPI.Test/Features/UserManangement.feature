
Feature UserManagement
This feature file describes the process of logging in and managing one's user account.

Background: 
    #Given the API is initialized
     
Scenario: Create account
    Given I send a create account request
    Then I assert that the account is created
    

#Scenario: Group assignment
#
#Scenario: Tag assignment 
#
#Scenario: Join request
#
#Scenario: Log off
#
#Scenario: Joining a Group
#
#Scenario: 
#    
#Scenario: 
#
#Scenario: 