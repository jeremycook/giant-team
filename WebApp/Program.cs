using GiantTeam;
using GiantTeam.Asp.Startup;
using GiantTeam.DataProtection;
using GiantTeam.Logging;
using GiantTeam.Postgres;
using GiantTeam.RecordsManagement.Data;
using GiantTeam.Startup.DatabaseConfiguration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace WebApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Console.WriteLine("Initial Configuration Sources:\n\t" + string.Join("\n\t", builder.Configuration.Sources.Select(o => o switch
            {
                JsonConfigurationSource json => o.GetType().Name + ": " + json.FileProvider?.GetFileInfo(json?.Path ?? string.Empty).PhysicalPath,
                _ => o.GetType().Name,
            })));

            Log.Factory = LoggerFactory.Create(options => options
                .AddConfiguration(builder.Configuration.GetSection("Logging"))
                .AddConsole());

            builder.Configuration.AddDatabase(
                builder.Configuration,
                builder.Environment,
                optional: builder.Environment.IsDevelopment());

            Log.Info<Program>("Final Configuration Sources: {ConfigurationSources}", builder.Configuration.Sources.Select(o => o switch
            {
                JsonConfigurationSource json => o.GetType().Name + ": " + json.FileProvider?.GetFileInfo(json?.Path ?? string.Empty).PhysicalPath,
                _ => o.GetType().Name,
            }));

            builder.ConfigureWithServiceBuilders<WebAppServiceBuilder>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            if (builder.Configuration.GetSection("ForwardedHeaders").Exists())
            {
                app.UseForwardedHeaders();
            }

            if (app.Configuration.GetSection("ForceHttps").Get<bool>() == true)
            {
                const string https = "https";
                app.Use((context, next) =>
                {
                    context.Request.Scheme = https;
                    return next(context);
                });
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = ctx =>
                {
                    // Everything under /assets/ can be cached Vite embeds a
                    // hash like "/assets/404.367b9fbf.js".
                    if (ctx.Context.Request.Path.StartsWithSegments("/assets/"))
                    {
                        const int durationInSeconds = 60 * 60 * 24;
                        ctx.Context.Response.Headers[HeaderNames.CacheControl] =
                            "public,max-age=" + durationInSeconds;
                    }
                }
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}