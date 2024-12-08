namespace SimpleChatApp.ErrorHandling.ResultPattern
{
    public static class NotificationErrors
    {
        public static Error InvitationAlreadyExists() => Error.Conflict(
            "Notifications.NotFound", $"User is invited already");
    }
}
