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
        IChatDataService _chatDataService;
        IMessageDataService _messageDataService;
        IUserHubContextManager _userHubContextManager;

        public ChatsController(UserManager<User> userManager,
                               IChatDataService chatDataService,
                               IMessageDataService messageDataService,
                               IUserHubContextManager userHubContextManager)
        {
            _userManager = userManager;
            _chatDataService = chatDataService;
            _messageDataService = messageDataService;
            _userHubContextManager = userHubContextManager;
        }
        [HttpPost]
        [Route("CreateChat")]
        [Authorize]
        public async Task<Results<Ok<ChatRoomDto>, BadRequest>> CreateChat(ChatRoomDto chatDto)
        {
            // TODO: add chat room name validation
            User? user = await _userManager.GetUserAsync(HttpContext.User);
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var createdChatDto = await _chatDataService.CreateChatAsync(user, chatDto);
            if (createdChatDto == null)
                return TypedResults.BadRequest();

            var userHubConnections = _userHubContextManager.GetUserHubContexts(user.Id);
            if (userHubConnections?.Count > 0)
            {
                List<string> groupNames = new() { createdChatDto.Name };
                _userHubContextManager.AddToGroups(user.Id, groupNames);
            }
                
            return TypedResults.Ok(createdChatDto);
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

            var result = await _chatDataService.GetUserChatsAsync(user);
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

            var result = await _chatDataService.GetChatMembersAsync(user, chatRoomName);

            if (result == null)
                return TypedResults.NotFound();

            return TypedResults.Ok(result);
        }
        [HttpGet]
        [Route("GetLastMessages")]
        [Authorize]
        public async Task<Results<Ok<List<MessageDto>>, NotFound, BadRequest>> GetLastMessages
            (string chatRoomName, int pageNumber, int pageSize)
        {
            // TODO: add pageSize limit
            if (pageSize == 0)
                return TypedResults.BadRequest();

            User? user = await _userManager.GetUserAsync(HttpContext.User);
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var messages = await _messageDataService.GetLastMessagesAsync(user, chatRoomName, pageNumber, pageSize);

            if (messages == null)
                return TypedResults.NotFound();

            return TypedResults.Ok(messages);
        }
    }
}
