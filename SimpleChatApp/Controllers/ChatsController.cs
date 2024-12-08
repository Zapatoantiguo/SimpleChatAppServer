using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleChatApp.Data;
using SimpleChatApp.Data.Services;
using SimpleChatApp.ErrorHandling.ResultPattern;
using SimpleChatApp.Hubs.Services;
using SimpleChatApp.Models;
using SimpleChatApp.Models.DTO;
using System.Security.Claims;

namespace SimpleChatApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        IChatDataService _chatDataService;
        IMessageDataService _messageDataService;
        IUserHubContextManager _userHubContextManager;

        public ChatsController(IChatDataService chatDataService,
                               IMessageDataService messageDataService,
                               IUserHubContextManager userHubContextManager)
        {
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

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var createdChatDto = await _chatDataService.CreateChatAsync(userId, chatDto);
            if (createdChatDto == null)
                return TypedResults.BadRequest();

            var userHubConnections = _userHubContextManager.GetUserHubContexts(userId);
            if (userHubConnections?.Count > 0)
            {
                List<string> groupNames = new() { createdChatDto.Name };
                _userHubContextManager.AddToGroups(userId, groupNames);
            }
                
            return TypedResults.Ok(createdChatDto);
        }
        [HttpGet]
        [Route("GetUserChats")]
        [Authorize]
        public async Task<List<ChatRoomDto>> GetUserChats()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _chatDataService.GetUserChatsAsync(userId);
            return result;
        }
        [HttpGet]
        [Route("GetChatMembers")]
        [Authorize]
        public async Task<Results<Ok<List<UserDto>>, NotFound>> GetChatMembers(string chatRoomName)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _chatDataService.GetChatMembersAsync(userId, chatRoomName);

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

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var messages = await _messageDataService.GetLastMessagesAsync(userId, chatRoomName, pageNumber, pageSize);

            if (messages == null)
                return TypedResults.NotFound();

            return TypedResults.Ok(messages);
        }
        
    }
}
