using GiantTeam.Asp.Startup;
using GiantTeam.DatabaseModeling;
using GiantTeam.DataProtection;
using GiantTeam.EntityFramework;
using GiantTeam.Postgres;
using GiantTeam.RecordsManagement.Data;
using GiantTeam.WorkspaceAdministration.Data;
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
            var logger = app.Services.GetRequiredService<ILogger<Program>>();

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
                using var scope = app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DataProtectionDbContext>();

                Database database = new();
                EntityFrameworkDatabaseContributor.Singleton.Contribute(database, db.Model, DataProtectionDbContext.DefaultSchema);
                PgDatabaseScripter scripter = new();
                string sql = scripter.Script(database);

                logger.LogDebug("Data Protection:\n\n{Sql}", sql);
                //db.Database.ExecuteSqlRaw("REVOKE ALL PRIVILEGES ON SCHEMA public FROM PUBLIC;");
                //db.Database.ExecuteSqlRaw(sql);
            }

            {
                var factory = app.Services.GetRequiredService<IDbContextFactory<RecordsManagementDbContext>>();
                using var db = factory.CreateDbContext();

                Database database = new();
                EntityFrameworkDatabaseContributor.Singleton.Contribute(database, db.Model, RecordsManagementDbContext.DefaultSchema);

                PgDatabaseScripter scripter = new();
                string sql = scripter.Script(database);

                logger.LogDebug("Records Management:\n\n{Sql}", sql);
                //db.Database.ExecuteSqlRaw("REVOKE ALL PRIVILEGES ON SCHEMA public FROM PUBLIC;");
                //db.Database.ExecuteSqlRaw(sql);
            }

            {
                using var scope = app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<WorkspaceAdministrationDbContext>();

                Database database = new();
                EntityFrameworkDatabaseContributor.Singleton.Contribute(database, db.Model, WorkspaceAdministrationDbContext.DefaultSchema);
                EmbeddedResourcesDatabaseScriptsContributor.Singleton.Contribute<WorkspaceAdministrationDbContext>(database);
                PgDatabaseScripter scripter = new();
                string sql = scripter.Script(database);

                logger.LogDebug("Workspace Administration:\n\n{Sql}", sql);
                //db.Database.ExecuteSqlRaw("REVOKE ALL PRIVILEGES ON SCHEMA public FROM PUBLIC;");
                //db.Database.ExecuteSqlRaw(sql);
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