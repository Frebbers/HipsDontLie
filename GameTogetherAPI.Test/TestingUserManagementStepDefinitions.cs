using GameTogetherAPI.Test.Drivers;
using GameTogetherAPI.Test.Fixtures;

namespace GameTogetherAPI.Test;
using FluentAssertions;

[Binding]
public class TestingUserManagementStepDefinitions
{
    private readonly ScenarioContext scenarioContext;
    public APITestContext Context { get; }
    
    public TestingUserManagementStepDefinitions(APITestContext context, ScenarioContext scenarioContext)
    {
        this.Context = context;
        this.scenarioContext = scenarioContext;
    }


    [Given(@"I send a create account request")]
    public async Task GivenISendACreateAccountRequest()
    {
        APIDriver driver = new APIDriver(Context.Client);
        var response = await driver.SendRequest($"/api/auth/register", HttpMethod.Post, new { email = "user@example.com", Password = "Password123" });
        scenarioContext.Add("response", response);
    }

    [Then(@"I assert that the account is created")]
    public void ThenIAssertThatTheAccountIsCreated()
    {
        var response = scenarioContext.Get<HttpResponseMessage>("response");
        var responseCode = response.StatusCode.ToString();
        responseCode.Should().BeEquivalentTo("OK");
    }
}