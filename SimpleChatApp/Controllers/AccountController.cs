using SimpleChatApp.Models.Requests.Auth;
using SimpleChatApp.Models;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using SimpleChatApp.Services;

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
        IUserHubContextManager _userHubContextManager;

        public AccountController(UserManager<User> userManager,
                    IUserStore<User> userStore,
                    SignInManager<User> signInManager,
                    IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
                    TimeProvider timeProvider,
                    IUserHubContextManager userHubContextManager)
        {
            _userManager = userManager;
            _userStore = userStore;
            _signInManager = signInManager;
            _bearerTokenOptions = bearerTokenOptions;
            _timeProvider = timeProvider;
            _userHubContextManager = userHubContextManager;
        }

        [HttpPost]
        [Route("register")]
        public async Task<Results<Ok, ValidationProblem>> Register([FromBody] RegisterRequest regRequest)
        {
            var user = new User() { UserName = regRequest.Username, IsAnonimous = false };
            var result = await _userManager.CreateAsync(user, regRequest.Password);

            if (!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }

            return TypedResults.Ok();
            // TODO: make register - login in 1 action?
        }
        [HttpPost]
        [Route("GuestLogin")]
        public async Task<Results<Ok<AccessTokenResponse>, ValidationProblem, EmptyHttpResult>> GuestLogin
            ([FromBody] GuestLoginRequest loginRequest,
            [FromQuery] bool? useCookies)
        {
            var user = new User() { UserName = loginRequest.Username, IsAnonimous = true };
            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }

            var useCookieScheme = useCookies == true;
            var isPersistent = false;
            _signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;
            var signInTask = _signInManager.SignInAsync(user, isPersistent);
            await signInTask;

            // The signInManager already produced the needed response in the form of a cookie or bearer token.
            return TypedResults.Empty;
        }
        [HttpPost]
        [Route("login")]
        public async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> Login
            ([FromBody] LoginRequest loginRequest, [FromQuery] bool? useCookies, [FromQuery] bool? useSessionCookies)
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
        public async Task<Results<Ok, UnauthorizedHttpResult>> Logout()
        {
            User? user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return TypedResults.Unauthorized();
            }

            _userHubContextManager.Disconnect(user.Id);
            await _signInManager.SignOutAsync();
            

            if (user.IsAnonimous)
            {
                var delResult = await _userManager.DeleteAsync(user);
                // ...?
            }

            return TypedResults.Ok();
        }

        [HttpPost]
        [Route("RefreshToken")]
        [Authorize]
        public async Task<Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult, SignInHttpResult, ChallengeHttpResult>> Register
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
