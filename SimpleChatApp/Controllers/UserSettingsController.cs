using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimpleChatApp.Data.Services;
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
        public async Task<UserProfileDto?> GetProfile()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var profile = await _userDataService.GetUserProfileAsync(userId);
            return profile!;
        }

        [HttpPost]
        [Authorize]
        [Route("UpdateProfile")]
        public async Task<Results<Ok<UserProfileDto>, BadRequest>> UpdateProfile([FromBody] UserProfileDto profile)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var pr = await _userDataService.UpdateUserProfileAsync(userId, profile);
            if (pr == null)
                return TypedResults.BadRequest();

            return TypedResults.Ok(pr);
        }
    }
}
