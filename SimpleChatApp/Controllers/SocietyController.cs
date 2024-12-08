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
            if (!ModelState.IsValid)
            {
                return TypedResults.BadRequest();
            }
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
        public async Task<Results<Ok<FriendDto>, BadRequest, NotFound>> AddFriend([FromBody] FriendDto friend)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var addedFriend = await _userDataService.AddFriendAsync(userId, friend);
            if (addedFriend == null)
                return TypedResults.NotFound();

            return TypedResults.Ok(addedFriend);
        }

        [HttpPost]
        [Route("RemoveFriend")]
        [Authorize]
        public async Task<Results<Ok<FriendDto>, BadRequest, NotFound>> RemoveFriend([FromBody] FriendDto friend)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var removedFriend = await _userDataService.RemoveFriendAsync(userId, friend);
            if (removedFriend == null)
                return TypedResults.NotFound();

            return TypedResults.Ok(removedFriend);
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
