using Microsoft.EntityFrameworkCore;
using SimpleChatApp_BAL.Background;
using SimpleChatApp_BAL.Services;
using SimpleChatApp.Hubs;
using SimpleChatApp.Hubs.Services;
using SimpleChatApp_DAL.Models;
using SimpleChatApp_DAL;
using SimpleChatApp;

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

builder.Services.AddSingleton<IUserHubContextManager, EntryLockUserHubContextManager>();

builder.Services.AddHostedService<CleanDbBackgroundTask>();
var app = builder.Build();

app.UseHttpsRedirection();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<AppHub>("/ChatHub");


app.Run();

public partial class Program { }