using GiantTeam;
using GiantTeam.Asp.Startup;
using GiantTeam.DataProtection;
using GiantTeam.Logging;
using GiantTeam.Postgres;
using GiantTeam.RecordsManagement.Data;
using GiantTeam.Startup.DatabaseConfiguration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Options;

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

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                //app.UseExceptionHandler("/error");
            }

            ConnectionOptions? migrationConnectionOptions = app.Configuration
                .GetSection("MigrationConnection")
                .Get<ConnectionOptions>();

            if (migrationConnectionOptions is not null)
            {
                DataProtectionOptions dataProtectionOptions = app.Services.GetRequiredService<IOptions<DataProtectionOptions>>().Value;
                GiantTeamOptions giantTeamOptions = app.Services.GetRequiredService<IOptions<GiantTeamOptions>>().Value;

                try
                {
                    await app.Services.MigrateDbContextAsync<DataProtectionDbContext>(migrationConnectionOptions, dataProtectionOptions.DataProtectionConnection);
                    await app.Services.MigrateDbContextAsync<RecordsManagementDbContext>(migrationConnectionOptions, giantTeamOptions.MgmtConnection);
                }
                catch (Exception ex)
                {
                    app.Logger.LogError(ex, "Suppressed migration exception {Exception}: {ExceptionMessage}", ex.GetBaseException(), ex.GetBaseException().Message);
                }
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}