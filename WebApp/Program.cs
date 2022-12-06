using GiantTeam;
using GiantTeam.Asp.Startup;
using GiantTeam.DataProtection;
using GiantTeam.Postgres;
using GiantTeam.RecordsManagement.Data;
using Microsoft.Extensions.Options;

namespace WebApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.ConfigureWithServiceBuilders<WebAppServiceBuilder>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                //app.UseExceptionHandler("/error");
                //app.UseHsts();
                app.UseHttpsRedirection();
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

            app.Run();
        }
    }
}