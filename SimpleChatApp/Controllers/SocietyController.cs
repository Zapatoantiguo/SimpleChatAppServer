using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SimpleChatApp_BAL.Services;
using SimpleChatApp_BAL.DTO;
using SimpleChatApp_BAL.ErrorHandling.ResultPattern;
using System.Security.Claims;

namespace SimpleChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SocietyController : ControllerBase
    {
        IUserDataService _userDataService;

        public SocietyController(IUserDataService userDataService)
        {
            _userDataService = userDataService;
        }

        [HttpGet]
        [Route("FindUsers")]
        [Authorize]
        public async Task<Results<Ok<List<UserDto>>, BadRequest>> FindUsers([FromBody] UserSearchDto searchDto)
        {
            List<UserDto> users = await _userDataService.GetUsersViaFilterAsync(searchDto);

            return TypedResults.Ok(users);
        }

        [HttpGet]
        [Route("GetAllUsers")]
        [Authorize]
        public async Task<Results<Ok<List<UserDto>>, BadRequest>> GetAllUsers()
        {
            List<UserDto> users = await _userDataService.GetAllUsersAsync();
            return TypedResults.Ok(users);
        }

        [HttpPost]
        [Route("AddFriend")]
        [Authorize]
        public async Task<IResult> AddFriend([FromBody] FriendDto friend)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var addResult = await _userDataService.AddFriendAsync(userId, friend);
            if (addResult.IsFailure)
                return addResult.ToProblemDetails();

            return TypedResults.Ok(addResult.Value);
        }

        [HttpPost]
        [Route("RemoveFriend")]
        [Authorize]
        public async Task<IResult> RemoveFriend([FromBody] FriendDto friend)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var removeResult = await _userDataService.RemoveFriendAsync(userId, friend);
            if (removeResult.IsFailure)
                return removeResult.ToProblemDetails();

            return TypedResults.Ok(removeResult.Value);
        }

        [HttpGet]
        [Route("GetAllFriends")]
        [Authorize]
        public async Task<List<FriendDto>> GetAllFriends()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var friendsDtos = await _userDataService.GetAllFriendsAsync(userId);
            return friendsDtos;
        }
    }
}
