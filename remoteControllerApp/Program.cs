using remoteControllerApp.Firebase;
using remoteControllerApp.Hubs;
using remoteControllerApp.Manager;
using remoteControllerApp.Repositories;
using remoteControllerApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024;
});

builder.Services.Configure<FirebaseSettings>(
    builder.Configuration.GetSection("Firebase")
);

builder.Services.AddSingleton<FirebaseAuthService>();
builder.Services.AddHttpClient<RealtimeDatabaseClient>();
builder.Services.AddScoped<IRealtimeDatabaseRepository, RealtimeDatabaseRepository>();

builder.Services.AddSingleton<ConnectionManager>();
builder.Services.AddSingleton<SessionManager>();

builder.Services.AddScoped<ConnectionService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.MapHub<RemoteHub>("/remoteHub");

app.Run();