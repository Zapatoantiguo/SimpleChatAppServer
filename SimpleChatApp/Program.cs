using Microsoft.EntityFrameworkCore;
using SimpleChatApp.Background;
using SimpleChatApp.Data;
using SimpleChatApp.Data.Services;
using SimpleChatApp.ErrorHandling;
using SimpleChatApp.Hubs;
using SimpleChatApp.Hubs.Services;
using SimpleChatApp.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();

var connString = builder.Configuration.GetValue<string>("ChatAppConnStr");  // env var for pg
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connString));

builder.Services.AddIdentityApiEndpoints<User>().AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthorization();

builder.Services.AddScoped<IUserDataService, UserDataService>();
builder.Services.AddScoped<IChatDataService, ChatDataService>();
builder.Services.AddScoped<INotificationDataService, NotificationDataService>();
builder.Services.AddScoped<IMessageDataService, MessageDataService>();
builder.Services.AddScoped<IInvitationService, InvitationService>();

builder.Services.AddSingleton<IUserHubContextManager, UserHubContextManager>();

builder.Services.AddHostedService<CleanDbBackgroundTask>();
var app = builder.Build();

app.UseHttpsRedirection();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<AppHub>("/ChatHub");


app.Run();
