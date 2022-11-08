using GiantTeam.Data;
using GiantTeam.DatabaseModel;
using GiantTeam.DataProtection;
using GiantTeam.EntityFramework;
using GiantTeam.Postgres;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var serviceBuilderCollection = new ServiceCollection();
            serviceBuilderCollection.AddSingleton(builder.Services);
            serviceBuilderCollection.AddSingleton(builder.Environment);
            serviceBuilderCollection.AddSingleton<IHostEnvironment>(builder.Environment);
            serviceBuilderCollection.AddSingleton(builder.Configuration);
            serviceBuilderCollection.AddSingleton<IConfiguration>(builder.Configuration);
            serviceBuilderCollection.AddSingleton(builder.Logging);
            serviceBuilderCollection.AddSingleton(builder.Host);
            serviceBuilderCollection.AddSingleton<IHostBuilder>(builder.Host);
            serviceBuilderCollection.AddSingleton(builder.WebHost);
            serviceBuilderCollection.AddSingleton<IWebHostBuilder>(builder.WebHost);
            serviceBuilderCollection.AddLogging();
            var standardServiceTypes = serviceBuilderCollection.Select(s => s.ServiceType).ToImmutableHashSet();
            var serviceBuilderTypes = GetDependentTypes(typeof(WebAppServiceBuilder)).Append(typeof(WebAppServiceBuilder))
                .Distinct()
                .Where(t => !standardServiceTypes.Contains(t))
                .ToList();
            foreach (var type in serviceBuilderTypes)
            {
                serviceBuilderCollection.AddSingleton(type);
            }
#pragma warning disable ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
            var serviceBuilderServices = serviceBuilderCollection.BuildServiceProvider();
#pragma warning restore ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
            foreach (var type in serviceBuilderTypes)
            {
                serviceBuilderServices.GetService(type);
            }

            // Configure the HTTP request pipeline.
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
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

            app.MapRazorPages();

            app.Run();
        }

        private static IEnumerable<Type> GetDependentTypes(Type type)
        {
            return type.GetConstructors()
                .SelectMany(ctor => ctor.GetParameters())
                .SelectMany(p => GetDependentTypes(p.ParameterType).Append(p.ParameterType));
        }
    }
}