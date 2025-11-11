using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;

    public CustomAuthStateProvider(IJSRuntime jsRuntime) => _jsRuntime = jsRuntime;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "jwt");
        if (string.IsNullOrWhiteSpace(token)) return Anonymous();

        if (!TryBuildPrincipal(token, out var user))
        {
            await LogoutAsync();
            return Anonymous();
        }
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
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task LogoutAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "jwt");
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

    public Task<string?> GetAccessTokenAsync()
         => _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "jwt").AsTask();
}
