using GameTogetherAPI.Models;
using GameTogetherAPI.Test.Drivers;
using GameTogetherAPI.Test.Fixtures;
using GameTogetherAPI.Test.Hooks;
using GameTogetherAPI.Test.Util;

namespace GameTogetherAPI.Test.StepDefinitions;
using FluentAssertions;
using GameTogetherAPI.Test.Models;
using SpecFlow.Internal.Json;

[Binding]
public class TestingUserManagementStepDefinitions(ScenarioContext scenarioContext)
{

    [Given(@"I send a create account request")]
    public async Task GivenISendACreateAccountRequest()
    {
        var driver = new APIDriver(TestHooks.Context.Client);
        var response = await driver.SendPostRequest($"/api/auth/register", new RegisterModel(
            APIConstants.TestEmail, APIConstants.TestPassword)
        );
        scenarioContext.Add("response", response);
    }
    
    [Then(@"I assert that the account is created")]
    public void ThenIAssertThatTheAccountIsCreated()
    {
        var response = scenarioContext.Get<HttpResponseMessage>("response");
        var responseCode = response.StatusCode.ToString();
        responseCode.Should().BeEquivalentTo("OK");
    }

    [When("I send a log in request")]
    
    public async Task  GivenISendALogInRequest()
    {
        var driver = new APIDriver(TestHooks.Context.Client);
        LoginModel loginModel = new LoginModel(APIConstants.TestEmail, APIConstants.TestPassword);
        var response = await driver.SendPostRequest("/api/auth/login", loginModel);
        APIResponse responseModel = JSONParser.FromJson<APIResponse>(response.Content.ReadAsStringAsync().Result);
        //string responseToken = responseString.Split("")[1]; //remove "Token: " from the response
        var responseCode = response.StatusCode.ToString();
        try
        {
           scenarioContext.Add("token", responseModel.token);
        }
        catch (Exception e)
        {
            Console.WriteLine("No token exists");
        }
        
    }

    [Then("I assert that the account is logged in")]
    [When(@"I am logged in")]
    public async Task ThenIAssertThatTheAccountIsLoggedIn()
    {   
        var driver = new APIDriver(TestHooks.Context.Client);
        string responseToken = scenarioContext.Get<string>("token").ToString();
        HttpResponseMessage response = await driver.SendGetRequest("/api/auth/me", new Dictionary<string, string> { { "Authorization", "Bearer " + responseToken } }, null);
        var responseModel = JSONParser.FromJson<APIResponse>(response.Content.ReadAsStringAsync().Result);
        responseModel.email.Should().BeEquivalentTo(APIConstants.TestEmail);
    }

        [Given(@"I click the log off button")]
        public void GivenIClickTheLogOffButton()
        {
            ScenarioContext.StepIsPending();
        }
        
        [Given(@"TestUser is reset")]
        public async Task GivenTestUserIsReset()
        {
            var driver = new APIDriver(TestHooks.Context.Client);
            await GivenISendALogInRequest();
            if (scenarioContext.ContainsKey("token")) //if there is a token, delete the user
            { 
                var responseToken = scenarioContext.Get<string>("token").ToString();
                var deleteResponse = await driver.SendDeleteRequest("/api/auth/remove-user", new Dictionary<string, string> { { "Authorization", "Bearer " + responseToken } });
                deleteResponse.StatusCode.ToString().Should().BeEquivalentTo("OK");
            }
            else
            {
                Console.WriteLine("User failed to log in");
            }
    }
        
}