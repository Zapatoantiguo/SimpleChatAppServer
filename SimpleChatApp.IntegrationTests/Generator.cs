using SimpleChatApp.IntegrationTests.TestModels;

namespace SimpleChatApp.IntegrationTests
{
    public static class Generator
    {
        const string commonPwd = "123QWEasd1!";
        public static List<AppUser> CreateTestUsers()
        {
            RegisteredUser tom = new()
            {
                UserName = "Tom",
                Password = commonPwd,
                Email = "Tom@abc.com"
            };
            RegisteredUser pom = new()
            {
                UserName = "Pom",
                Password = commonPwd,
                Email = "Pom@abc.com"
            };
            RegisteredUser bom = new()
            {
                UserName = "Bom",
                Password = commonPwd,
                Email = "Bom@abc.com"
            };
            AnonUser anon = new()
            {
                UserName = "Anon1"
            };
            List<AppUser> result = new() { tom, pom, bom, anon };
            return result;
        }
    }
}
