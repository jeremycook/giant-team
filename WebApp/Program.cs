using GiantTeam.DatabaseModel;
using GiantTeam.DataProtection;
using GiantTeam.EntityFramework;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Security.Claims;
using WebApp.Data;
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

            services.AddHttpContextAccessor();

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

                    options.Events.OnSigningIn = async (context) =>
                    {
                        //context.Options.ExpireTimeSpan
                    };
                    options.Events.OnSignedIn = async (context) =>
                    {
                        //context.Options.ExpireTimeSpan
                    };
                    options.Events.OnCheckSlidingExpiration = async (context) =>
                    {
                    };
                    options.Events.OnValidatePrincipal = async (context) =>
                    {
                        if (context.ShouldRenew)
                        {
                            IDbContextFactory<GiantTeamDbContext> dbContextFactory = context.HttpContext.RequestServices
                                .GetRequiredService<IDbContextFactory<GiantTeamDbContext>>();

                            ClaimsIdentity identity =
                                context.Principal?.Identity as ClaimsIdentity ??
                                throw new NullReferenceException("The ClaimsIdentity is null.");

                            // Synchronize the lifespan of the passwords with the authentication cookie
                            DateTimeOffset databasePasswordValidUntil = DateTimeOffset.UtcNow.Add(context.Options.ExpireTimeSpan);

                            SessionUser sessionUser = new(identity, databasePasswordValidUntil);

                            using (var db = await dbContextFactory.CreateDbContextAsync())
                            {
                                // Set database passwords
                                await db.SetDatabaseUserPasswordsAsync(sessionUser.DatabaseUsername, sessionUser.DatabaseSlot, sessionUser.DatabasePassword, sessionUser.DatabasePasswordValidUntil);
                            }

                            ClaimsPrincipal principal = new(sessionUser.CreateIdentity());

                            context.ReplacePrincipal(principal);
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                // Use the fallback policy to require all users to be authenticated
                // except when accessing Razor Pages, controllers or action methods
                // with an authorization or anonymous attribute.
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            services.AddRazorPages();

            services.Configure<EncryptionOptions>(configuration.GetSection("Encryption"));

            services.AddDbContext<DataProtectionDbContext>(options =>
            {
                string connectionString = configuration.GetConnectionString("DataProtection");
                NpgsqlConnection connection = new(connectionString);

                if (configuration.GetSection("ConnectionStrings:DataProtectionCaCertificate").Get<string>() is string caCertificate)
                {
                    connection.ConfigureCaCertificateValidation(caCertificate);
                }

                options
                .AddInterceptors(new OpenedDbConnectionInterceptor($"SET ROLE {PgQuote.Identifier("giantteam")};"))
                .UseNpgsql(connection);
            });
            services.AddDataProtection().PersistKeysToDbContext<DataProtectionDbContext>();

            services.AddPooledDbContextFactory<GiantTeamDbContext>(options =>
            {
                string connectionString = configuration.GetConnectionString("Main");
                NpgsqlConnection connection = new(connectionString);

                if (configuration.GetSection("ConnectionStrings:MainCaCertificate").Get<string>() is string connectionCaCertificateText)
                {
                    connection.ConfigureCaCertificateValidation(connectionCaCertificateText);
                }

                options
                .AddInterceptors(new OpenedDbConnectionInterceptor($"SET ROLE {PgQuote.Identifier("giantteam")};"))
                .UseNpgsql(connection);
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
                app.UseHttpsRedirection();
            }

            {
                var gtDbContextFactory = app.Services.GetRequiredService<IDbContextFactory<GiantTeamDbContext>>();
                using var giantTeamDbContext = gtDbContextFactory.CreateDbContext();

                Database database = new();
                EntityFrameworkDatabaseContributor.Singleton.Contribute(database, giantTeamDbContext.Model, GiantTeamDbContext.DefaultSchema);
                ObjectDatabaseScriptsContributor.Singleton.Contribute(database, "./Data/Scripts");

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
    }
}