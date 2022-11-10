using GiantTeam.Asp.Startup;
using GiantTeam.Data;
using GiantTeam.DatabaseModel;
using GiantTeam.DataProtection;
using GiantTeam.EntityFramework;
using GiantTeam.Postgres;
using Microsoft.EntityFrameworkCore;

namespace WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.ConfigureServicesWithServiceBuilders<WebAppServiceBuilder>();

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

            {
                var gtDbContextFactory = app.Services.GetRequiredService<IDbContextFactory<GiantTeamDbContext>>();
                using var giantTeamDbContext = gtDbContextFactory.CreateDbContext();

                Database database = new();
                EntityFrameworkDatabaseContributor.Singleton.Contribute(database, giantTeamDbContext.Model, GiantTeamDbContext.DefaultSchema);
                EmbeddedResourcesDatabaseScriptsContributor.Singleton.Contribute<GiantTeamDbContext>(database);

                PgDatabaseScripter scripter = new();
                string sql = scripter.Script(database);

                giantTeamDbContext.Database.ExecuteSqlRaw("REVOKE ALL PRIVILEGES ON SCHEMA public FROM PUBLIC;");
                giantTeamDbContext.Database.ExecuteSqlRaw(sql);
            }

            {
                using var scope = app.Services.CreateScope();
                var dataProtectionDbContext = scope.ServiceProvider.GetRequiredService<DataProtectionDbContext>();

                Database database = new();
                EntityFrameworkDatabaseContributor.Singleton.Contribute(database, dataProtectionDbContext.Model, DataProtectionDbContext.DefaultSchema);
                PgDatabaseScripter scripter = new();
                string sql = scripter.Script(database);

                dataProtectionDbContext.Database.ExecuteSqlRaw("REVOKE ALL PRIVILEGES ON SCHEMA public FROM PUBLIC;");
                dataProtectionDbContext.Database.ExecuteSqlRaw(sql);
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