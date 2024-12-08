using Microsoft.EntityFrameworkCore;
using SimpleChatApp.Models;
using SimpleChatApp.Models.DTO;

namespace SimpleChatApp.Data.Services
{

    public class MessageDataService : IMessageDataService
    {
        AppDbContext _context;
        public MessageDataService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Message?> AddMessageAsync(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return message;
        }

        public async Task<List<MessageDto>?> GetLastMessagesAsync(string userId, string chatRoomName, int pageNumber, int pageSize)
        {
            var chat = await _context.ChatRooms
                .Include(ch => ch.Users)
                .SingleOrDefaultAsync(ch => ch.Name == chatRoomName);

            if (chat == null) return null;

            if (!chat.Users.Any(u => u.Id == userId))
                return null;

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

            return messages;
        }
    }
}
