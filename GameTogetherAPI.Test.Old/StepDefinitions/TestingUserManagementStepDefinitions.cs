using HipsDontLie.DTO;
using HipsDontLie.Models;
using HipsDontLie.Test.Old.Fixtures;
using HipsDontLie.Test.Old.Drivers;
using HipsDontLie.Test.Old.Hooks;
using HipsDontLie.Test.Old.Models;
using HipsDontLie.Test.Old.Util;

namespace GameTogetherAPI.Test.Old.StepDefinitions;
using FluentAssertions;
using Old.Models;
using SpecFlow.Internal.Json;

[Binding]
public class TestingUserManagementStepDefinitions(ScenarioContext scenarioContext)
{

    [Given(@"I send a create account request")]
    public async Task GivenISendACreateAccountRequest()
    {
        var driver = new APIDriver(TestHooks.Context.Client);
        var registerModel = new RegisterModel(
            APIConstants.TestEmail, APIConstants.TestPassword);
        var response = await driver.SendRequest(HttpMethod.Post,"/api/auth/register" ,false,null,new object[]{registerModel} );
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
        var response = await driver.SendRequest(HttpMethod.Post, "/api/auth/login", false, null,new object[]{loginModel});
        APIResponse responseModel = JSONParser.FromJson<APIResponse>(response.Content.ReadAsStringAsync().Result);
        //string responseToken = responseString.Split("")[1]; //remove "Token: " from the response
        var statusCode = response.StatusCode.ToString();
        try
        {
            scenarioContext.Add("StatusCode", statusCode);
            scenarioContext.Add("token", responseModel.token);
        }
        catch (Exception e)
        {
            Console.WriteLine("No token exists");
        }
        
    }

    [Then("I assert that the account is logged in")]
    [When(@"I am logged in")]
    [Given("I am logged in")]
    public async Task ThenIAssertThatTheAccountIsLoggedIn()
    {
        var responseCode = scenarioContext.Get<string>("StatusCode");
        responseCode.Should().BeEquivalentTo("OK");
        var driver = new APIDriver(TestHooks.Context.Client);
        string responseToken = scenarioContext.Get<string>("token").ToString();
        driver.SetAuthToken(responseToken);
        HttpResponseMessage response = await driver.SendRequest(HttpMethod.Get, "/api/auth/me", true);
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
            string token = scenarioContext.Get<string>("token");
            if (token != null) //if there is a token, delete the user
            { 
                var responseToken = scenarioContext.Get<string>("token");
                driver.SetAuthToken(responseToken);
                var deleteResponse = await driver.SendRequest(HttpMethod.Delete, "/api/auth/remove-user", true);
                deleteResponse.StatusCode.ToString().Should().BeEquivalentTo("OK");
            }
            else
            {
                Console.WriteLine("User failed to log in");
            }
    }

        [Then(@"I am no longer logged in")]
        public async Task ThenIAmNoLongerLoggedIn()
        {
            var driver = new APIDriver(TestHooks.Context.Client);
            driver.SetAuthToken("");
            var logOutResponse = await driver.SendRequest(HttpMethod.Get, "api/auth/me", true);
            logOutResponse.StatusCode.ToString().Should().BeEquivalentTo("Unauthorized");



        }

        [When(@"I send a join request")]
        public async Task WhenISendAJoinRequest()
        {
            APIDriver driver = new APIDriver(TestHooks.Context.Client);
            var responseToken =scenarioContext.Get<string>("token").ToString();
            var sessionCreationResponse = await driver.SendRequest(HttpMethod.Post, "/api/Sessions/1/join", true);
            sessionCreationResponse.StatusCode.ToString().Should().BeEquivalentTo("OK");
        }

        [Given(@"a group has been created")]
        public async Task GivenAGroupHasBeenCreated()
        {
            var driver = new APIDriver(TestHooks.Context.Client);
            var responseToken1 =scenarioContext.Get<string>("tokenI").ToString();
            var authorization = new Dictionary<string, string>
                { { "Authorization", "Bearer " + responseToken1 } };
            var createGroupRequestDto = new CreateGroupRequestDTO()
            {
                Title = "Testgroup",
                AgeRange = "18+",
                Description = "Testgroup description",
                IsVisible = true,
                MaxMembers = 10,
                Tags =
                ["tag1","tag2","tag3","tag4","tag5","tag6","tag7",
                ],
            };
            var groupResponse = await driver.SendRequest(HttpMethod.Post, "/api/Groups/create", true, null,new object[]{createGroupRequestDto});
            groupResponse.StatusCode.ToString().Should().BeEquivalentTo("ok");
        }

        [Given(@"i create a second user")]
        public async Task ThenICreateASecondUser()
        {
            var driver = new APIDriver(TestHooks.Context.Client);
            var registerModel = new RegisterModel(
                APIConstants.TestEmail1, APIConstants.TestPassword1);
            await driver.SendRequest(HttpMethod.Post,"/api/auth/register", false,null, new object[]{registerModel});
            LoginModel loginModel = new LoginModel(APIConstants.TestEmail1, APIConstants.TestPassword1);
            var loginResponse = await driver.SendRequest(HttpMethod.Post,$"/api/auth/login", false,null, new object[]{loginModel});
            APIResponse responseModel1 = JSONParser.FromJson<APIResponse>(loginResponse.Content.ReadAsStringAsync().Result);
            var statusCode1 = loginResponse.StatusCode.ToString();
            try
            {
                scenarioContext.Add("StatusCode1", statusCode1);
                scenarioContext.Add("tokenI", responseModel1.token);
            }
            catch (Exception e)
            {
                Console.WriteLine("No token exists");
            }
        }

        [Then(@"I join a group")]
        public async Task ThenIJoinAGroup()
        {
            
            var driver = new APIDriver(TestHooks.Context.Client);
            var responseToken1 = scenarioContext.Get<string>("tokenI");
            driver.SetAuthToken(responseToken1);
            var joinResponse = await driver.SendRequest(HttpMethod.Get, "/api/Groups/user", true);
            joinResponse.StatusCode.ToString().Should().BeEquivalentTo("OK");
        }
}