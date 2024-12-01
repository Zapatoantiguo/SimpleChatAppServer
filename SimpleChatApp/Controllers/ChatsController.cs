using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleChatApp.Data;
using SimpleChatApp.Data.Services;
using SimpleChatApp.Hubs.Services;
using SimpleChatApp.Models;
using SimpleChatApp.Models.DTO;

namespace SimpleChatApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        UserManager<User> _userManager;
        IDbDataService _dbDataService;
        IUserHubContextManager _userHubContextManager;

        public ChatsController(UserManager<User> userManager,
                               IDbDataService dbDataService,
                               IUserHubContextManager userHubContextManager)
        {
            _userManager = userManager;
            _dbDataService = dbDataService;
            _userHubContextManager = userHubContextManager;
        }
        [HttpPost]
        [Route("CreateChat")]
        [Authorize]
        public async Task<Results<Ok<ChatRoomDto>, ValidationProblem>> CreateChat(ChatRoomDto chatDto)
        {
            // TODO: add chat room name validation
            User? user = await _userManager.GetUserAsync(HttpContext.User);
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            ChatRoomDto created;
            created = await _dbDataService.CreateChatAsync(user, chatDto);

            var userHubConnections = _userHubContextManager.GetUserHubContexts(user.Id);
            if (userHubConnections?.Count > 0)
            {
                List<string> groupNames = new() { created.Name };
                _userHubContextManager.AddToGroups(user.Id, groupNames);
            }
                
            return TypedResults.Ok(created);
        }
        [HttpGet]
        [Route("GetUserChats")]
        [Authorize]
        public async Task<List<ChatRoomDto>> GetUserChats()
        {
            User? user = await _userManager.GetUserAsync(HttpContext.User);
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var result = await _dbDataService.GetUserChatsAsync(user);
            return result;
        }
        [HttpGet]
        [Route("GetChatMembers")]
        [Authorize]
        public async Task<Results<Ok<List<UserDto>>, NotFound>> GetChatMembers(string chatRoomName)
        {
            User? user = await _userManager.GetUserAsync(HttpContext.User);
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var result = await _dbDataService.GetChatMembersAsync(user, chatRoomName);

            if (result == null)
                return TypedResults.NotFound();

            return TypedResults.Ok(result);
        }
        [HttpGet]
        [Route("GetLastMessages")]
        [Authorize]
        public async Task<Results<Ok<List<MessageDto>>, NotFound, BadRequest>> GetLastMessages
            (string chatRoomName, uint pageNumber, uint pageSize)
        {
            // TODO: add pageSize limit
            if (pageSize == 0)
                return TypedResults.BadRequest();

            User? user = await _userManager.GetUserAsync(HttpContext.User);
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var messages = await _dbDataService.GetLastMessagesAsync(user, chatRoomName, pageNumber, pageSize);

            if (messages == null)
                return TypedResults.NotFound();

            return TypedResults.Ok(messages);
        }
    }
}
