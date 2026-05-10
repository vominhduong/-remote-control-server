using remoteControllerApp.Hubs;
using remoteControllerApp.Manager;
using remoteControllerApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSignalR();

builder.Services.AddSingleton<ConnectionManager>();
builder.Services.AddSingleton<SessionManager>();

builder.Services.AddScoped<ConnectionService>();
builder.Services.AddScoped<SessionService>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Tạm tắt khi dev local
// app.UseHttpsRedirection();

app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.MapHub<RemoteHub>("/remoteHub");

app.Run();