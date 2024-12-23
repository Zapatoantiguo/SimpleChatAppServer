using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleChatApp.Hubs.Services;
using SimpleChatApp.IntegrationTests.TestModels;
using SimpleChatApp_DAL;

namespace SimpleChatApp.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public ServiceProvider ServiceProvider { get; set; }
        public List<AppUser> Guys { get; set; } = Generator.CreateTestUsers();
        public RegisteredUser Tom {  get; set; }
        public RegisteredUser Pom { get; set; }
        public RegisteredUser Bom { get; set; }
        public AnonUser Anon { get; set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var connStr = Environment.GetEnvironmentVariable("ChatAppConnStrTests");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<AppDbContext>();
                services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connStr));

                var serviceProvider = services.BuildServiceProvider();
                ServiceProvider = serviceProvider;

                using (var scope = serviceProvider.CreateScope())
                {
                    var _dbContext = serviceProvider.GetRequiredService<AppDbContext>();
                    _dbContext.Database.EnsureDeleted();
                    _dbContext.Database.EnsureCreated();
                }
            });

            InitTestData();
        }
        private void InitTestData()
        {
            Tom = Guys.FirstOrDefault(g => g.UserName == "Tom") as RegisteredUser;
            Pom = Guys.FirstOrDefault(g => g.UserName == "Pom") as RegisteredUser;
            Bom = Guys.FirstOrDefault(g => g.UserName == "Bom") as RegisteredUser;
            Anon = Guys.FirstOrDefault(g => g.UserName == "Anon1") as AnonUser;
        }
    }
}
