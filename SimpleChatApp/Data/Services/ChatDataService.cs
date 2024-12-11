using Microsoft.EntityFrameworkCore;
using SimpleChatApp.ErrorHandling.ResultPattern;
using SimpleChatApp.Models;
using SimpleChatApp.Models.DTO;

namespace SimpleChatApp.Data.Services
{
    public class ChatDataService : IChatDataService
    {
        AppDbContext _context;
        public ChatDataService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Result<User>> AddUserToChatAsync(string userId, string chatRoomName)
        {
            User? user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new Exception($"User ID {userId} doesn't exist in DB");

            ChatRoom? chat = await _context.ChatRooms.SingleOrDefaultAsync(c => c.Name == chatRoomName);
            if (chat == null)
                return Result<User>.Failure(ChatErrors.NotFound(chatRoomName));

            if (chat.Users.Any(u => u.Id == userId))
                return Result<User>.Failure(ChatErrors.UserInChatAlready());

            UserChatRoom joinEntry = new()
            {
                ChatRoomId = chat.ChatRoomId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            };
            chat.UserChatRoom.Add(joinEntry);

            await _context.SaveChangesAsync();
            return Result<User>.Success(user);
        }

        public async Task<Result<ChatRoomDto>> CreateChatAsync(string creatorId, ChatRoomDto chatDto)
        {
            ArgumentNullException.ThrowIfNull(creatorId);
            ArgumentNullException.ThrowIfNull(chatDto);

            var nameIsNotUnique = await _context.ChatRooms
                .AnyAsync(c => c.Name == chatDto.Name);

            if (nameIsNotUnique) 
                return Result<ChatRoomDto>.Failure(ChatErrors.NameIsNotUnique());

            var creator = await _context.Users.SingleOrDefaultAsync(u => u.Id == creatorId);
            if (creator == null)
                throw new Exception($"User ID {creatorId} doesn't exist in DB");

            var chat = new ChatRoom
            {
                Name = chatDto.Name,
                Description = chatDto.Description,
                Users = new List<User> { creator }
            };

            _context.ChatRooms.Add(chat);
            await _context.SaveChangesAsync();

            return Result<ChatRoomDto>.Success(
                new ChatRoomDto
                {
                    Name = chat.Name,
                    Description = chat.Description
                });
        }

        public async Task<Result<int>> GetChatIdByName(string chatRoomName)
        {
            var chat = await _context.ChatRooms.SingleOrDefaultAsync(chat => chat.Name == chatRoomName);
            if (chat == null)
                return Result<int>.Failure(ChatErrors.NotFound(chatRoomName));

            return Result<int>.Success(chat.ChatRoomId);
        }

        public async Task<Result<List<UserDto>>> GetChatMembersAsync(string requesterId, string chatRoomName)
        {
            var chat = await _context.ChatRooms
                .Include(ch => ch.Users)
                .SingleOrDefaultAsync(ch => ch.Name == chatRoomName);

            if (chat == null) 
                return Result<List<UserDto>>.Failure(ChatErrors.NotFound(chatRoomName));

            if (!chat.Users.Any(u => u.Id == requesterId))
                return Result<List<UserDto>>.Failure(ChatErrors.UserIsNotInChat());

            var result = chat.Users
                .Select(u => new UserDto { Name = u.UserName!, IsAnonimous = u.IsAnonimous })
                .ToList();

            return Result<List<UserDto>>.Success(result);
        }

        public async Task<List<ChatRoomDto>> GetUserChatsAsync(string userId)
        {
            var userChats = (await _context.Users
                .Where(u => u.Id == userId)
                .Include(u => u.ChatRooms)
                .Select(u => u.ChatRooms.Select(chat => new ChatRoomDto { Name = chat.Name, Description = chat.Description }))
                .SingleOrDefaultAsync())?.ToList();

            return userChats ?? new List<ChatRoomDto>();
        }

        public async Task<bool> IsUserInChat(string userId, string chatRoomName)
        {
            var chat = await _context.ChatRooms
                .Where(c => c.Name == chatRoomName)
                .Include(c => c.UserChatRoom.Where(uc => uc.UserId == userId))
                .SingleOrDefaultAsync();

            if (chat == null)
                return false;

            if (chat.UserChatRoom?.Count > 0)
                return true;

            return false;
        }

        public async Task<Result<User>> RemoveUserFromChatAsync(string userId, string chatRoomName)
        {
            User? user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new Exception($"User ID {userId} doesn't exist in DB");

            ChatRoom? chat = await _context.ChatRooms
                .Include(cr => cr.UserChatRoom)
                .SingleOrDefaultAsync(c => c.Name == chatRoomName);

            if (chat == null)
                return Result<User>.Failure(ChatErrors.NotFound(chatRoomName));

            chat.UserChatRoom.RemoveAll(ucr => ucr.UserId == userId);    
            await _context.SaveChangesAsync();
            return Result<User>.Success(user);
        }
    }
}
