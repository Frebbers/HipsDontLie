using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private readonly HttpClient _httpClient;

    public CustomAuthStateProvider(IJSRuntime jsRuntime, HttpClient httpClient)
    {
        _jsRuntime = jsRuntime;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "jwt");

        if (string.IsNullOrWhiteSpace(token))
            return Anonymous();

        if (!TryBuildPrincipal(token, out var user))
        {
            await LogoutAsync();
            return Anonymous();
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return new AuthenticationState(user);
    }

    public async Task MarkUserAsAuthenticated(string token)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "jwt", token);

        if (!TryBuildPrincipal(token, out var user))
        {
            await LogoutAsync();
            return;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task LogoutAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "jwt");
        _httpClient.DefaultRequestHeaders.Authorization = null;
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous()));
    }

    //Reset auth
    private static AuthenticationState Anonymous() => new(new ClaimsPrincipal(new ClaimsIdentity()));

    private static bool TryBuildPrincipal(string token, out ClaimsPrincipal user)
    {
        user = new ClaimsPrincipal(new ClaimsIdentity());

        JwtSecurityToken? jwt;
        try
        {
            jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        }
        catch
        {
            return false;
        }

        //check if token is expired
        if (jwt.ValidTo <= DateTime.UtcNow)
            return false;

        var mappedClaims = jwt.Claims.Select(c =>
        {
            return c.Type switch
            {
                "unique_name" => new Claim(ClaimTypes.Name, c.Value),
                "nameid" => new Claim(ClaimTypes.NameIdentifier, c.Value),
                "role" => new Claim(ClaimTypes.Role, c.Value),
                _ => c
            };
        });

        var identity = new ClaimsIdentity(
            mappedClaims,
            authenticationType: "jwt",
            nameType: ClaimTypes.Name,
            roleType: ClaimTypes.Role
        );

        user = new ClaimsPrincipal(identity);
        return true;
    }
}
