using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleChatApp_BAL.DTO;
using SimpleChatApp_BAL.ErrorHandling.ResultPattern;
using SimpleChatApp_DAL.Models;
using System.Collections.Generic;
using SimpleChatApp_DAL;

namespace SimpleChatApp_BAL.Services
{

    public class MessageDataService : IMessageDataService
    {
        AppDbContext _context;
        public MessageDataService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Result<Message>> AddMessageAsync(string senderId, string chatName, string content)
        {
            User? sender = await _context.Users.FindAsync(senderId);
            if (sender == null)
                throw new Exception($"User ID {senderId} doesn't exist in DB");

            ChatRoom? chat = await _context.ChatRooms
                .SingleOrDefaultAsync(c => c.Name == chatName);
            if (chat == null)
                return Result<Message>.Failure(ChatErrors.NotFound(chatName));

            var senderInChat = await _context.UserChatRoom
                .AnyAsync(uc => uc.UserId == senderId && uc.ChatRoomId == chat.ChatRoomId);
            if (!senderInChat)
                return Result<Message>.Failure(ChatErrors.UserIsNotInChat());

            var msg = new Message
            {
                AuthorAlias = sender.UserName!,
                Author = sender,
                Content = content,
                ChatRoomId = chat.ChatRoomId,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            return Result<Message>.Success(msg);
        }

        public async Task<Result<List<MessageDto>>> GetLastMessagesAsync(string userId, string chatRoomName, int pageNumber, int pageSize)
        {
            if (pageNumber < 0 || pageSize < 1)
                return Result<List<MessageDto>>.Failure(Error
                    .Validation("MessageServiceValidation", "Incorrect pagination parameters"));

            var chat = await _context.ChatRooms
                .Include(ch => ch.Users)
                .SingleOrDefaultAsync(ch => ch.Name == chatRoomName);

            if (chat == null)
                return Result<List<MessageDto>>.Failure(ChatErrors.NotFound(chatRoomName));

            if (!chat.Users.Any(u => u.Id == userId))
                return Result<List<MessageDto>>.Failure(ChatErrors.UserIsNotInChat());

            int startIndex = pageNumber * pageSize;
            int endIndex = startIndex + (int)pageSize;

            var messages = await _context.Messages
                .Where(msg => msg.ChatRoomId == chat.ChatRoomId)
                .OrderByDescending(msg => msg.SentAt)
                .Skip(startIndex)
                .Take(pageSize)
                .Select(msg => new MessageDto
                {
                    AuthorAlias = msg.AuthorAlias,
                    Content = msg.Content,
                    SentAt = msg.SentAt
                })
                .ToListAsync();

            return Result<List<MessageDto>>.Success(messages);
        }
    }
}
