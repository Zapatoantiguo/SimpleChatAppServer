
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleChatApp_BAL;
using SimpleChatApp_DAL;

namespace SimpleChatApp_BAL.Background
{
    public class CleanDbBackgroundTask : BackgroundService
    {
        // to implement deletion of main entity when dependent entities no more exist
        // (for 1-to-many relations; in this app delete ChatRoom when there are no users in it)
        // i found 2 ways: DB triggers and periodic task. I chose last one to keep more logic in app
        // in contrast with split function between app and dbms
        // This class do such functions

        private const double CALL_INTERVAL = 1800;    // TODO: add config value 
        private readonly IServiceProvider _serviceProvider; // only transient and singletons available. Provider for scoped dbCondext
        private readonly TimeSpan _period = TimeSpan.FromSeconds(CALL_INTERVAL);
        public CleanDbBackgroundTask(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new PeriodicTimer(_period);
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                using IServiceScope scope = _serviceProvider.CreateScope();
                await using AppDbContext dbContext = scope.ServiceProvider.GetService<AppDbContext>();

                var chatsWithoutMembers = await dbContext.ChatRooms
                    .Include(ch => ch.UserChatRoom)
                    .Where(ch => ch.UserChatRoom.Count == 0)
                    .ToListAsync();

                dbContext.ChatRooms.RemoveRange(chatsWithoutMembers);
                await dbContext.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
