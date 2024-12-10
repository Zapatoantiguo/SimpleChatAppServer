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
        public async Task<IResult> CreateChat(ChatRoomDto chatDto)
        {
            // TODO: add chat room name validation

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var creationResult = await _chatDataService.CreateChatAsync(userId, chatDto);

            if (creationResult.IsFailure)
                return creationResult.ToProblemDetails();

            var userHubConnections = _userHubContextManager.GetUserHubContexts(userId);
            if (userHubConnections?.Count > 0)
            {
                List<string> groupNames = new() { creationResult.Value.Name };
                _userHubContextManager.AddToGroups(userId, groupNames);
            }
                
            return TypedResults.Ok(creationResult.Value);
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
        public async Task<IResult> GetChatMembers(string chatRoomName)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _chatDataService.GetChatMembersAsync(userId, chatRoomName);

            if (result.IsFailure)
                return result.ToProblemDetails();

            return TypedResults.Ok(result.Value);
        }
        [HttpGet]
        [Route("GetLastMessages")]
        [Authorize]
        public async Task<IResult> GetLastMessages
            (string chatRoomName, int pageNumber, int pageSize)
        {
            // TODO: add pageSize limit
            if (pageSize == 0)
                return TypedResults.BadRequest();

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var getMsgResult = await _messageDataService.GetLastMessagesAsync(userId, chatRoomName, pageNumber, pageSize);

            if (getMsgResult.IsFailure)
                return getMsgResult.ToProblemDetails();

            return TypedResults.Ok(getMsgResult.Value);
        }
        
    }
}
