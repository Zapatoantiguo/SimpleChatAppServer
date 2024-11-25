﻿using Microsoft.AspNetCore.Authorization;
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
    public class SocietyController : ControllerBase
    {
        IDbDataService _dbDataService;
        UserManager<User> _userManager;

        public SocietyController(IDbDataService dbDataService,
                                 UserManager<User> userManager)
        {
            _dbDataService = dbDataService;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("GetUsers")]
        [Authorize]
        public async Task<Results<Ok<List<UserDto>>, BadRequest>> GetUsers([FromBody] UserSearchDto searchDto)
        {
            if (!ModelState.IsValid)
            {
                return TypedResults.BadRequest();
            }
            List<UserDto> users = await _dbDataService.GetUsersByPatternAsync(searchDto);

            return TypedResults.Ok(users);
        }

        [HttpPost]
        [Route("AddFriend")]
        [Authorize]
        public async Task<Results<Ok<FriendDto>, BadRequest, NotFound>> AddFriend([FromBody] FriendDto friend)
        {
            if (!ModelState.IsValid)
                return TypedResults.BadRequest();

            var user = await _userManager.GetUserAsync(HttpContext.User);

            var addedFriend = await _dbDataService.AddFriendAsync(user!, friend);
            if (addedFriend == null)
                return TypedResults.NotFound();

            return TypedResults.Ok(addedFriend);
        }

        [HttpPost]
        [Route("RemoveFriend")]
        [Authorize]
        public async Task<Results<Ok<FriendDto>, BadRequest, NotFound>> RemoveFriend([FromBody] FriendDto friend)
        {
            if (!ModelState.IsValid)
                return TypedResults.BadRequest();

            var user = await _userManager.GetUserAsync(HttpContext.User);

            var removedFriend = await _dbDataService.AddFriendAsync(user!, friend);
            if (removedFriend == null)
                return TypedResults.NotFound();

            return TypedResults.Ok(removedFriend);
        }

        [HttpGet]
        [Route("GetAllFriends")]
        [Authorize]
        public async Task<List<FriendDto>> GetAllFriends()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var friendsDtos = await _dbDataService.GetAllFriendsAsync(user!);
            return friendsDtos;
        }
    }
}
