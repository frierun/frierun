using Autofac;
using Autofac.Extensions.DependencyInjection;
using Frierun.Server;
using Frierun.Server.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(autofacBuilder => autofacBuilder.RegisterModule(new AutofacModule()));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen(
    options =>
    {
        options.SchemaGeneratorOptions.SupportNonNullableReferenceTypes = true;
        options.SchemaGeneratorOptions.NonNullableReferenceTypesAsRequired = true;

        options.UseOneOfForPolymorphism();
        options.UseAllOfForInheritance();
        options.EnableAnnotations(true, true);
    }
);
builder.Services.AddAntiforgery(
    options =>
    {
        options.HeaderName = "X-XSRF-TOKEN";
        options.SuppressXFrameOptionsHeader = false;
    }
);
builder.Services.AddControllersWithViews(
    options =>
    {
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = false;
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    }
);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Storage.DirectoryName, "keys")));

// create web root path to remove warnings in dev environment
var webRootPath = Path.Combine(builder.Environment.ContentRootPath, builder.Environment.WebRootPath ?? "wwwroot");
if (!Directory.Exists(webRootPath))
{
    Directory.CreateDirectory(webRootPath);
}

var app = builder.Build();

// load packages
app.Services.GetRequiredService<PackageRegistry>().Load();

// Configure the HTTP request pipeline.
app.UseSwagger(
    c =>
    {
        c.PreSerializeFilters.Add(
            (swagger, httpReq) =>
            {
                swagger.Servers = new List<OpenApiServer>
                    { new() { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}/api/v1" } };
            }
        );
    }
);

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
}

var antiForgery = app.Services.GetRequiredService<IAntiforgery>();
app.Use(
    async (context, next) =>
    {
        if (!context.Request.Cookies.ContainsKey("XSRF-TOKEN"))
        {
            var tokenSet = antiForgery.GetAndStoreTokens(context);
            context.Response.Cookies.Append(
                "XSRF-TOKEN",
                tokenSet.RequestToken!,
                new CookieOptions { HttpOnly = false }
            );
        }

        await next(context);
    }
);

app.UseStaticFiles();
app.UsePathBase("/api/v1");
app.UseRouting();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();