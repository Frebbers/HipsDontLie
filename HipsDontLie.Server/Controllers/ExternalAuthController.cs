using HipsDontLie.Models;
using HipsDontLie.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HipsDontLie.Server.Controllers
{
    [Route("api/oauth/")]
    [ApiController]
    public class ExternalAuthController : ControllerBase
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IAuthService _authService;
        private readonly IConfiguration _config;

        public ExternalAuthController(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IAuthService authService,
            IConfiguration config)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _authService = authService;
            _config = config;
        }

        // GET /api/auth/external/{provider}?returnUrl=/signin-external
        [HttpGet("external/{provider}")]
        [AllowAnonymous]
        public IActionResult ChallengeExternal([FromRoute] string provider, [FromQuery] string returnUrl = "/signin-external")
        {
            var props = _signInManager.ConfigureExternalAuthenticationProperties(provider,
                redirectUrl: Url.Action(nameof(ExternalCallback), "ExternalAuth",
                    new { provider, returnUrl }, Request.Scheme));
            return Challenge(props, provider);
        }

        // GET /api/auth/external-callback/{provider}
        [HttpGet("external-callback/{provider}")]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalCallback([FromRoute] string provider, [FromQuery] string returnUrl = "/signin-external")
        {
            // Get info from external provider (stored in the "External" cookie)
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return Redirect($"{Frontend()}{returnUrl}?status=failed");

            // Try sign in existing external login
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            User user;

            if (signInResult.Succeeded)
            {
                user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            }
            else
            {
                // Get email
                var email = info.Principal.FindFirstValue(ClaimTypes.Email)
                            ?? info.Principal.FindFirstValue("email");
                if (string.IsNullOrWhiteSpace(email))
                    return Redirect($"{Frontend()}{returnUrl}?status=failed&reason=no-email");

                user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Create a new local user
                    user = new User
                    {
                        UserName = email, // or a nicer username if we have it
                        Email = email,
                        EmailConfirmed = true // external providers generally give verified emails
                    };
                    var createRes = await _userManager.CreateAsync(user);
                    if (!createRes.Succeeded)
                        return Redirect($"{Frontend()}{returnUrl}?status=failed&reason=create");

                    // Optional: assign default role
                    await _userManager.AddToRoleAsync(user, "Participant");
                }

                // Link external login to local user (if not linked)
                var addLogin = await _userManager.AddLoginAsync(user, info);
                if (!addLogin.Succeeded)
                    return Redirect($"{Frontend()}{returnUrl}?status=failed&reason=link");
            }

            // At this point, we have a local Identity user. Mint YOUR JWT.
            // If your IAuthService has a helper to mint token by User instance, call it:
            var jwt = await GenerateJwtFor(user);

            // Clear the temp external cookie
            await HttpContext.SignOutAsync("External");

            // Redirect back to SPA with token (fragment so it’s not sent to your server)
            return Redirect($"{Frontend()}{returnUrl}#token={Uri.EscapeDataString(jwt)}");
        }

        private string Frontend() => _config["FRONTEND_BASE_URL"] ?? "https://localhost:7057";

        private async Task<string> GenerateJwtFor(User user)
        {
            // reuse your existing private method via IAuthService if you have one
            // or implement a small wrapper in IAuthService
            // We know you already generate tokens on login; expose that internally:
            // e.g., return await _authService.GenerateJwtForUserAsync(user);
            // For now, if you only expose 'AuthenticateUserAsync(email,pwd)', add a new method to IAuthService.
            // Placeholder:
            return await (Task<string>)typeof(IAuthService)
                .GetMethod("GenerateJwtForUserAsync")!
                .Invoke(_authService, new object[] { user })!;
        }
    }
}
