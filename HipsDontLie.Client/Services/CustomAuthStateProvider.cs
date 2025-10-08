using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace HipsDontLie.Client.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity()); //non-authenticated user

            return Task.FromResult(new AuthenticationState(user));
        }
    }
}
