﻿namespace SimpleChatApp.Models
{
    public class ChatRoom
    {
        public int ChatRoomId {  get; set; }
        public required string ChatRoomName { get; set; }
        public string? ChatRoomDescription { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
