using SimpleChatApp.Models.Requests.Auth;
using SimpleChatApp.Models;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace SimpleChatApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        UserManager<User> _userManager;
        IUserStore<User> _userStore;
        SignInManager<User> _signInManager;
        IOptionsMonitor<BearerTokenOptions> _bearerTokenOptions;
        TimeProvider _timeProvider;

        public AccountController(UserManager<User> userManager,
                    IUserStore<User> userStore,
                    SignInManager<User> signInManager,
                    IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
                    TimeProvider timeProvider)
        {
            _userManager = userManager;
            _userStore = userStore;
            _signInManager = signInManager;
            _bearerTokenOptions = bearerTokenOptions;
            _timeProvider = timeProvider;
        }

        [HttpPost]
        [Route("register")]
        public async Task<Results<Ok, ValidationProblem, BadRequest>> Register([FromBody] RegisterRequestModel regRequest)
        {
            var email = regRequest.Email;
            var userName = regRequest.Username;

            var user = new User();
            await _userStore.SetUserNameAsync(user, userName, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, regRequest.Password);

            if (!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }

            return TypedResults.Ok();
        }
        [HttpPost]
        [Route("login")]
        public async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> Login
            ([FromBody] LoginRequestModel loginRequest, [FromQuery] bool? useCookies, [FromQuery] bool? useSessionCookies)
        {
            var useCookieScheme = (useCookies == true) || (useSessionCookies == true);
            var isPersistent = (useCookies == true) && (useSessionCookies != true);
            _signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;
            var result = await _signInManager.PasswordSignInAsync(loginRequest.Username,
                loginRequest.Password, isPersistent, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                return TypedResults.Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
            }

            // The signInManager already produced the needed response in the form of a cookie or bearer token.
            return TypedResults.Empty;
        }

        [HttpPost]
        [Route("logout")]
        [Authorize]
        public async Task<Results<Ok, UnauthorizedHttpResult>> Logout([FromBody] object empty)
        {
            if (empty != null)
            {   
                await _signInManager.SignOutAsync();
                return TypedResults.Ok();
            }
            return TypedResults.Unauthorized();
        }

        [HttpPost]
        [Route("refresh")]
        [Authorize]
        public async Task<Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult, SignInHttpResult, ChallengeHttpResult>>Register
            ([FromBody] RefreshRequest refreshRequest)
        {
            var refreshTokenProtector = _bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
            var refreshTicket = refreshTokenProtector.Unprotect(refreshRequest.RefreshToken);

            // Reject the /refresh attempt with a 401 if the token expired or the security stamp validation fails
            if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc ||
                _timeProvider.GetUtcNow() >= expiresUtc ||
                await _signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not User user)
            {
                return TypedResults.Challenge();
            }

            var newPrincipal = await _signInManager.CreateUserPrincipalAsync(user);
            return TypedResults.SignIn(newPrincipal, authenticationScheme: IdentityConstants.BearerScheme);
        }

        private static ValidationProblem CreateValidationProblem(IdentityResult result)
        {
            // We expect a single error code and description in the normal case.
            // This could be golfed with GroupBy and ToDictionary, but perf! :P
            Debug.Assert(!result.Succeeded);
            var errorDictionary = new Dictionary<string, string[]>(1);

            foreach (var error in result.Errors)
            {
                string[] newDescriptions;

                if (errorDictionary.TryGetValue(error.Code, out var descriptions))
                {
                    newDescriptions = new string[descriptions.Length + 1];
                    Array.Copy(descriptions, newDescriptions, descriptions.Length);
                    newDescriptions[descriptions.Length] = error.Description;
                }
                else
                {
                    newDescriptions = [error.Description];
                }

                errorDictionary[error.Code] = newDescriptions;
            }

            return TypedResults.ValidationProblem(errorDictionary);
        }
    }
}
