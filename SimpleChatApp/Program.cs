using Microsoft.EntityFrameworkCore;
using SimpleChatApp.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();

var connString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connString));

var app = builder.Build();

app.MapControllers();

app.Run();
