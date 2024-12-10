using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimpleChatApp.Data.Services;
using SimpleChatApp.ErrorHandling.ResultPattern;
using SimpleChatApp.Models;
using SimpleChatApp.Models.DTO;
using System.Security.Claims;

namespace SimpleChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserSettingsController : ControllerBase
    {
        IUserDataService _userDataService;
        public UserSettingsController(IUserDataService userDataService)
        {
            _userDataService = userDataService;
        }

        [HttpGet]
        [Authorize]
        [Route("GetProfile")]
        public async Task<IResult> GetProfile()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var profileResult = await _userDataService.GetUserProfileAsync(userId);

            if (profileResult.IsFailure)
                return profileResult.ToProblemDetails();

            return TypedResults.Ok(profileResult.Value);
        }

        [HttpPost]
        [Authorize]
        [Route("UpdateProfile")]
        public async Task<IResult> UpdateProfile([FromBody] UserProfileDto profile)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var updateResult = await _userDataService.UpdateUserProfileAsync(userId, profile);
            if (updateResult.IsFailure)
                return updateResult.ToProblemDetails();

            return TypedResults.Ok(updateResult.Value);
        }
    }
}
