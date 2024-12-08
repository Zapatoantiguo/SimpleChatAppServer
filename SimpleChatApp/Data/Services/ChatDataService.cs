﻿using Microsoft.EntityFrameworkCore;
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
        public async Task<User?> AddUserToChatAsync(string userId, string chatRoomName)
        {
            User? user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return null;

            ChatRoom? chat = await _context.ChatRooms.SingleOrDefaultAsync(c => c.Name == chatRoomName);
            if (chat == null)
                return null;

            if (chat.Users.Any(u => u.Id == userId))
                return null;

            UserChatRoom joinEntry = new()
            {
                ChatRoomId = chat.ChatRoomId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            };
            chat.UserChatRoom.Add(joinEntry);
            //chat.Users.Add(user);

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<ChatRoomDto?> CreateChatAsync(string creatorId, ChatRoomDto chatDto)
        {
            ArgumentNullException.ThrowIfNull(creatorId);
            ArgumentNullException.ThrowIfNull(chatDto);

            var nameIsNotUnique = await _context.ChatRooms
                .AnyAsync(c => c.Name == chatDto.Name);

            if (nameIsNotUnique) return null;


            var creator = await _context.Users.SingleOrDefaultAsync(u => u.Id == creatorId);
            if (creator == null) return null;

            var chat = new ChatRoom
            {
                Name = chatDto.Name,
                Description = chatDto.Description,
                Users = new List<User> { creator }
            };

            _context.ChatRooms.Add(chat);
            await _context.SaveChangesAsync();

            return new ChatRoomDto
            {
                Name = chat.Name,
                Description = chat.Description
            };
        }

        public async Task<int?> GetChatIdByName(string chatRoomName)
        {
            var chat = await _context.ChatRooms.SingleOrDefaultAsync(chat => chat.Name == chatRoomName);

            return chat?.ChatRoomId;
        }

        public async Task<List<UserDto>?> GetChatMembersAsync(string requesterId, string chatRoomName)
        {
            var chat = await _context.ChatRooms
                .Include(ch => ch.Users)
                .SingleOrDefaultAsync(ch => ch.Name == chatRoomName);

            if (chat == null) return null;

            if (!chat.Users.Any(u => u.Id == requesterId))
                return null;

            var result = chat.Users
                .Select(u => new UserDto { Name = u.UserName!, IsAnonimous = u.IsAnonimous })
                .ToList();

            return result;
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

        public async Task<User?> RemoveUserFromChatAsync(string userId, string chatRoomName)
        {
            User? user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return null;

            ChatRoom? chat = await _context.ChatRooms.SingleOrDefaultAsync(c => c.Name == chatRoomName);
            if (chat == null)
                return null;

            chat.Users.Remove(user);    // TODO: add Equals override on User?
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
