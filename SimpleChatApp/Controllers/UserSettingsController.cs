using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimpleChatApp.Data.Services;
using SimpleChatApp.Models;
using SimpleChatApp.Models.DTO;

namespace SimpleChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserSettingsController : ControllerBase
    {
        IDbDataService _dbDataService;
        UserManager<User> _userManager;
        public UserSettingsController(IDbDataService dbDataService,
                                      UserManager<User> userManager)
        {
            _dbDataService = dbDataService;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize]
        [Route("GetProfile")]
        public async Task<UserProfileDto> GetProfile()
        {
            User? user = await _userManager.GetUserAsync(HttpContext.User);
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            var profile = await _dbDataService.GetUserProfileAsync(user.Id);
            return profile!;
        }

        [HttpPost]
        [Authorize]
        [Route("UpdateProfile")]
        public async Task<Results<Ok<UserProfileDto>, BadRequest>> UpdateProfile([FromBody] UserProfileDto profile)
        {
            if (!ModelState.IsValid)
                return TypedResults.BadRequest();
            // TODO: add validation

            User? user = await _userManager.GetUserAsync(HttpContext.User);
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var pr =await _dbDataService.UpdateUserProfileAsync(user, profile);
            return TypedResults.Ok(pr);
        }
    }
}
