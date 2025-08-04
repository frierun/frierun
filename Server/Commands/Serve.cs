using Autofac;
using Autofac.Extensions.DependencyInjection;
using Frierun.Server.Data;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace Frierun.Server;

public class Serve(ILifetimeScope root) : BaseCommand("serve", "Start webserver")
{
    protected override void Execute()
    {
        CreateWebApplication().Run();
    }

    public IHost CreateWebApplication(Action<WebApplicationBuilder>? configure = null)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { ApplicationName = "Frierun.Server" });

        builder.Host.UseServiceProviderFactory(new AutofacChildLifetimeScopeServiceProviderFactory(root));

        builder.Logging.ClearProviders();
        
        builder.Services.AddSwaggerGen(options =>
            {
                options.SchemaGeneratorOptions.SupportNonNullableReferenceTypes = true;
                options.SchemaGeneratorOptions.NonNullableReferenceTypesAsRequired = true;

                options.UseOneOfForPolymorphism();
                options.UseAllOfForInheritance();
                options.EnableAnnotations(true, true);

                options.MapType(typeof(ContractId<>), () => new OpenApiSchema { Type = "string" });
                options.MapType(typeof(ContractId), () => new OpenApiSchema { Type = "string" });
                options.SchemaFilter<LazyHandlerSchemaFilter>();
                options.SchemaFilter<InstalledNotRequiredSchemaFilter>();
            }
        );
        builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-XSRF-TOKEN";
                options.SuppressXFrameOptionsHeader = false;
            }
        );

        builder.Services.AddControllersWithViews(options =>
            {
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = false;
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            }
        );
        builder.Services.ConfigureOptions<ConfigureJsonOptions>();

        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Storage.DirectoryName, "keys")));

        // create web root path to remove warnings in dev environment
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract required for integration tests
        var webRootPath = Path.Combine(
            builder.Environment.ContentRootPath, builder.Environment.WebRootPath ?? "wwwroot"
        );
        if (!Directory.Exists(webRootPath))
        {
            Directory.CreateDirectory(webRootPath);
        }

        if (configure != null)
        {
            configure(builder);
        }

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swagger, httpReq) =>
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
        app.Use(async (context, next) =>
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

        return app;
    }
}