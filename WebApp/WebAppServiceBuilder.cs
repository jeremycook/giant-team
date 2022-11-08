using GiantTeam.Asp;
using GiantTeam.Data;
using GiantTeam.Services;
using GiantTeam.Startup;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

namespace WebApp
{
    public class WebAppServiceBuilder : IServiceBuilder
    {
        public WebAppServiceBuilder(
            IServiceCollection services,
            GiantTeamAspServiceBuilder giantTeamServiceBuilder)
        {
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

            services.AddControllers();

            // See https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "GiantTeam API",
                    Description = "TODO",
                    TermsOfService = new Uri("/terms", UriKind.Relative),
                    Contact = new OpenApiContact
                    {
                        Name = "Contact Us",
                        Url = new Uri("/contact", UriKind.Relative)
                    },
                    License = new OpenApiLicense
                    {
                        Name = "License",
                        Url = new Uri("/license", UriKind.Relative)
                    },
                });
            });

            services.AddScopedFromAssembly(typeof(WebAppServiceBuilder).Assembly);
        }
    }
}
