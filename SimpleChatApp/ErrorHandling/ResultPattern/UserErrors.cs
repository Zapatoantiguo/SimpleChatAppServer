namespace SimpleChatApp.ErrorHandling.ResultPattern
{
    public static class UserErrors
    {
        public static Error NotFound() => Error.NotFound(
            "Users.NotFound", $"Specified user was not found");
        public static Error NotFound(string userName) => Error.NotFound(
            "Users.NotFound", $"User {userName} was not found");
        public static Error SelfFriendship() => Error.Validation(
            "Users.Validation", $"A user-requester and specified user-friend must not be the same");
        public static Error IsFriendAlready() => Error.Conflict(
            "Users.Conflict", $"A specified user is in friend list already");
        public static Error IsNotFriend() => Error.NotFound(
            "Users.NotFound", $"A specified user is not in friend list");
        public static Error UserHasProfileAlready() => Error.Conflict(
            "Users.Conflict", $"A specified user has profile already");
        public static Error ProfileNotFound() => Error.NotFound(
            "Users.NotFound", $"User profile for requested user was not found");
        public static Error NickExistsAlready() => Error.Conflict(
            "UserProfile.Conflict", $"A specified nickname already exists. Nickname must be unique");
    }
}
