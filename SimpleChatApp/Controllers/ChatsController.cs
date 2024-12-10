using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SimpleChatApp.Data;
using SimpleChatApp.Data.Services;
using SimpleChatApp.ErrorHandling.ResultPattern;
using SimpleChatApp.Hubs;
using SimpleChatApp.Hubs.Services;
using SimpleChatApp.Models;
using SimpleChatApp.Models.DTO;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SimpleChatApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        IChatDataService _chatDataService;
        IMessageDataService _messageDataService;
        IUserHubContextManager _userHubContextManager;
        IHubContext<AppHub, IHubClient> _hubContext;
        IInvitationService _invitationService;
        public ChatsController(IChatDataService chatDataService,
                               IMessageDataService messageDataService,
                               IUserHubContextManager userHubContextManager,
                               IHubContext<AppHub, IHubClient> hubContext,
                               IInvitationService invitationService)
        {
            _chatDataService = chatDataService;
            _messageDataService = messageDataService;
            _userHubContextManager = userHubContextManager;
            _hubContext = hubContext;
            _invitationService = invitationService;
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

            var userHubConnections = _userHubContextManager.GetUserConnectionIds(userId);
            foreach (var connectionId in userHubConnections!)
                await _hubContext.Groups.AddToGroupAsync(connectionId, creationResult.Value.Name);
                
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
        [HttpPost]
        [Route("SendMessage")]
        [Authorize]
        public async Task<IResult> SendMessage(string chatRoomName,
            [FromBody] MessageDto message)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var addMsgResult = await _messageDataService.AddMessageAsync(userId, chatRoomName, message.Content);

            if (addMsgResult.IsFailure) 
                return addMsgResult.ToProblemDetails();

            var addedMsg = addMsgResult.Value;
            MessageDto msgDto = new()
            {
                AuthorAlias = addedMsg.AuthorAlias,
                Content = addedMsg.Content,
                SentAt = addedMsg.SentAt
            };
            await _hubContext.Clients.Group(chatRoomName).OnMessageReceived(msgDto);
            return TypedResults.Ok(addedMsg);
        }
        [HttpPost]
        [Route("LeaveChat")]
        [Authorize]
        public async Task<IResult> LeaveChat(string chatRoomName)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var removeResult = await _chatDataService.RemoveUserFromChatAsync(userId, chatRoomName);

            if (removeResult.IsFailure) 
                return removeResult.ToProblemDetails();

            var hubConnIds = _userHubContextManager.GetUserConnectionIds(userId);

            if (hubConnIds?.Count > 0)
            foreach (var hubConnId in hubConnIds)
                await _hubContext.Groups.RemoveFromGroupAsync(hubConnId, chatRoomName);

            await _hubContext.Clients.Group(chatRoomName)
                .OnUserLeavedChat(removeResult.Value.UserName!, chatRoomName);

            return TypedResults.Ok();
        }
        [HttpPost]
        [Route("InviteToChatRoom")]
        [Authorize]
        public async Task<IResult> InviteToChatRoom(string targetUserName, string chatRoomName)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var invResult = await _invitationService.HandleInviteRequestAsync(userId, targetUserName, chatRoomName);

            if (invResult.IsFailure)
                return invResult.ToProblemDetails();

            var invitation = invResult.Value;
            // Send invitation to a target if one is connected
            if (_hubContext.Clients.User(invitation.TargetId) is not null)
            {
                await _hubContext.Clients.User(invitation.TargetId).OnInvited(invitation.SourceUserName, chatRoomName);
            }
            return TypedResults.Ok();
        }
        [HttpPost]
        [Route("RespondToInvite")]
        [Authorize]
        public async Task<IResult> RespondToInvite(string chatRoomName, bool accept)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var invHandleResult = await _invitationService
                .HandleInviteRespondAsync(userId, chatRoomName, accept);

            if (invHandleResult.IsFailure)
                return invHandleResult.ToProblemDetails();

            if (accept)
            {
                // add current and other connections of this user to hub group
                List<string>? userConnections = _userHubContextManager.GetUserConnectionIds(userId);
                foreach (var connectionId in userConnections!)
                    await _hubContext.Groups.AddToGroupAsync(connectionId, chatRoomName);

                string userName = User.FindFirstValue(ClaimTypes.Name)!;
                await _hubContext.Clients.Group(chatRoomName).OnUserJoinedChat(userName, chatRoomName);
            }
            return TypedResults.Ok();
        }
    }
}
