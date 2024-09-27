using Frierun.Server;
using Frierun.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.RegisterServices();

var app = builder.Build();

// load packages
app.Services.GetRequiredService<PackageRegistry>().Load();

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

