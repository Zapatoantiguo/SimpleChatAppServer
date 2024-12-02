﻿using SimpleChatApp.Models.DTO;
using SimpleChatApp.Models;

namespace SimpleChatApp.Data.Services
{
    public interface IMessageDataService
    {
        public Task<List<MessageDto>?> GetLastMessagesAsync(User user, string chatRoomName, uint pageNumber, uint pageSize);
        public Task<Message?> AddMessageAsync(Message message);
    }
}