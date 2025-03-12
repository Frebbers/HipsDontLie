using GameTogetherAPI.Test.Drivers;
using GameTogetherAPI.Test.Fixtures;
using GameTogetherAPI.Test.Hooks;

namespace GameTogetherAPI.Test.StepDefinitions;
using FluentAssertions;

[Binding]
public class TestingUserManagementStepDefinitions(ScenarioContext scenarioContext)
{
    private readonly ScenarioContext scenarioContext = scenarioContext;

    [Given(@"I send a create account request")]
    public async Task GivenISendACreateAccountRequest()
    {
        APIDriver driver = new APIDriver(TestHooks.Context.Client);
        var response = await driver.SendRequest($"/api/auth/register", HttpMethod.Post, new { Util.APIConstants.TestEmail,Util.APIConstants.TestPassword});
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