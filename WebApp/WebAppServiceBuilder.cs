using GiantTeam.Asp;
using GiantTeam.Asp.Filters;
using GiantTeam.Asp.Routing;
using GiantTeam.Startup;
using GiantTeam.UserManagement.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Net;

namespace WebApp
{
    public class WebAppServiceBuilder : IServiceBuilder
    {
        public WebAppServiceBuilder(
            IServiceCollection services,
            IConfiguration configuration,
            GiantTeamAspServiceBuilder giantTeamServiceBuilder)
        {
            if (configuration.GetSection("ForwardedHeaders") is var forwardedHeaders &&
                forwardedHeaders.Exists())
            {
                services.Configure<ForwardedHeadersOptions>(forwardedHeaders);
                services.Configure<ForwardedHeadersOptions>(binderOptions =>
                {
                    if (forwardedHeaders.GetSection("KnownProxies").Get<string[]>() is string[] knownProxies)
                    {
                        foreach (var item in knownProxies)
                        {
                            if (IPAddress.TryParse(item, out var address))
                                binderOptions.KnownProxies!.Add(address!);
                        }
                    }
                });
            }

            services.AddHttpContextAccessor();

            services.AddCookiePolicy(options =>
            {
                options.Secure = CookieSecurePolicy.Always;
            });

            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.HttpOnly = true;

                    // Controls the lifetime of the authentication session and cookie
                    // when AuthenticationProperties.IsPersistent is set to true.
                    options.ExpireTimeSpan = TimeSpan.FromDays(45);
                    options.SlidingExpiration = true;

                    // This is an API, do not redirect the client
                    options.AccessDeniedPath = string.Empty;
                    options.LoginPath = string.Empty;
                    options.LogoutPath = string.Empty;
                    options.Events.OnRedirectToAccessDenied = innerOptions =>
                    {
                        innerOptions.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        return Task.CompletedTask;
                    };
                    options.Events.OnRedirectToLogin = innerOptions =>
                    {
                        innerOptions.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        return Task.CompletedTask;
                    };
                    options.Events.OnRedirectToLogout = innerOptions =>
                    {
                        throw new NotSupportedException();
                    };
                    options.Events.OnRedirectToReturnUrl = innerOptions =>
                    {
                        throw new NotSupportedException();
                    };

                    options.Events.OnValidatePrincipal = async (context) =>
                    {
                        if (context.ShouldRenew)
                        {
                            SessionService sessionService = context.HttpContext.RequestServices
                                .GetRequiredService<SessionService>();
                            DatabaseSecurityService security = context.HttpContext.RequestServices
                                .GetRequiredService<DatabaseSecurityService>();

                            // Synchronize the lifespan of the passwords with the authentication cookie
                            DateTimeOffset validUntil = DateTimeOffset.UtcNow.Add(context.Options.ExpireTimeSpan).AddMinutes(1);

                            // Extend life of db login's password
                            await security.SetLoginExpirationAsync(sessionService.User, validUntil);
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                // Use the fallback policy to require all users to be authenticated
                // except when accessing Razor Pages, controllers or action methods
                // with an authorization or anonymous attribute.
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            services.AddControllers(options =>
            {
                // Exception filters we control
                options.Filters.Add<GiantTeamExceptionFilter>();

                // Slugify paths
                options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
            });
            services.Configure<ApiBehaviorOptions>(options =>
            {
                // Disabling the automatic validation
                options.SuppressModelStateInvalidFilter = true;
            });

            // More info at https://aka.ms/aspnetcore/swashbuckle
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
