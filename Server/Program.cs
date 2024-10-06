using Frierun.Server;
using Frierun.Server.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen(options =>
{
    options.SchemaGeneratorOptions.SupportNonNullableReferenceTypes = true;
    options.SchemaGeneratorOptions.NonNullableReferenceTypesAsRequired = true;
});
builder.Services.AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = false);
builder.Services.RegisterServices();

var app = builder.Build();

// load packages
app.Services.GetRequiredService<PackageRegistry>().Load();

// Configure the HTTP request pipeline.
app.UseSwagger(c =>
{
    c.PreSerializeFilters.Add((swagger, httpReq) =>
    {
        swagger.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}/api/v1" } };
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UsePathBase("/api/v1");
app.UseRouting();
app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();

