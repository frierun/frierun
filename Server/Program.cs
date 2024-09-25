using Frierun.Server.Models;
using Frierun.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddSingleton<DockerService>();
builder.Services.AddSingleton<PackageRegistry>();
builder.Services.AddSingleton<StateManager>();
builder.Services.AddSingleton<InstallService>();
builder.Services.AddSingleton<UninstallService>();
builder.Services.AddSingleton<State>(services => services.GetRequiredService<StateManager>().Load());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();

