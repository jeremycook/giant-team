using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebApp.Asp;
using WebApp.Data;
using WebApp.DatabaseModel;
using WebApp.EntityFramework;
using WebApp.Postgres;
using WebApp.Services;

namespace WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var configuration = builder.Configuration;

            // Add services to the container.
            var services = builder.Services;

            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.HttpOnly = true;

                    options.AccessDeniedPath = "/access-denied";
                    options.LoginPath = "/login";
                    options.LogoutPath = "/logout";

                    //// Controls the lifetime of the authentication session and cookie
                    //// when AuthenticationProperties.IsPersistent is set to true.
                    //options.ExpireTimeSpan = TimeSpan.FromDays(2);
                    //options.SlidingExpiration = true;
                });

            services.AddRazorPages();

            services.Configure<EncryptionOptions>(configuration.GetSection("Encryption"));

            services.AddDbContext<AspDbContext>(options =>
            {
                string connectionString = configuration.GetConnectionString("Main");
                NpgsqlConnection connection = new(connectionString);

                if (configuration.GetSection("ConnectionStrings:MainCaCertificate").Get<string>() is string connectionCaCertificateText)
                {
                    connection.ConfigureCaCertificateValidation(connectionCaCertificateText);
                }

                options.UseSnakeCaseNamingConvention().UseNpgsql(connection);
            });
            services.AddDataProtection().PersistKeysToDbContext<AspDbContext>();

            services.AddPooledDbContextFactory<GiantTeamDbContext>(options =>
            {
                string connectionString = configuration.GetConnectionString("Main");
                NpgsqlConnection connection = new(connectionString);

                if (configuration.GetSection("ConnectionStrings:MainCaCertificate").Get<string>() is string connectionCaCertificateText)
                {
                    connection.ConfigureCaCertificateValidation(connectionCaCertificateText);
                }

                options.UseSnakeCaseNamingConvention().UseNpgsql(connection);
            });

            foreach (Type type in from t in typeof(Program).Assembly.ExportedTypes
                                  where
                                      t.Namespace!.EndsWith(".Services") &&
                                      t.Name.EndsWith("Service")
                                  select t)
            {
                services.AddScoped(type);
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
            }

            {
                var gtDbContextFactory = app.Services.GetRequiredService<IDbContextFactory<GiantTeamDbContext>>();
                using var gt = gtDbContextFactory.CreateDbContext();

                Database database = new();
                EntityFrameworkDatabaseContributor.Singleton.Contribute(database, gt.Model, "gt");
                ObjectDatabaseScriptsContributor.Singleton.Contribute(database, "./Data/Scripts");

                PgDatabaseScripter scripter = new();
                string sql = scripter.Script(database);

                gt.Database.ExecuteSqlRaw("REVOKE ALL PRIVILEGES ON SCHEMA public FROM PUBLIC;");
                gt.Database.ExecuteSqlRaw(sql);
            }

            {
                using var scope = app.Services.CreateScope();
                var asp = scope.ServiceProvider.GetRequiredService<AspDbContext>();

                Database database = new();
                EntityFrameworkDatabaseContributor.Singleton.Contribute(database, asp.Model, AspDbContext.DefaultSchema);
                PgDatabaseScripter scripter = new();
                string sql = scripter.Script(database);

                asp.Database.ExecuteSqlRaw("REVOKE ALL PRIVILEGES ON SCHEMA public FROM PUBLIC;");
                asp.Database.ExecuteSqlRaw(sql);
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}