using GiantTeam.Asp;
using GiantTeam.Asp.Filters;
using GiantTeam.Asp.Routing;
using GiantTeam.Cluster.Security.Services;
using GiantTeam.Startup;
using GiantTeam.UserManagement.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
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
                    if (forwardedHeaders.GetSection("KnownNetworks").Get<string[]>() is string[] knownNetworks)
                    {
                        binderOptions.KnownNetworks.Clear();
                        foreach (var value in knownNetworks)
                        {
                            var segments = value.Split('/');
                            var prefix = IPAddress.Parse(segments[0]);
                            var prefixLength = int.Parse(segments[1]);
                            var network = new IPNetwork(prefix, prefixLength);
                            binderOptions.KnownNetworks.Add(network);
                        }
                    }
                    if (forwardedHeaders.GetSection("KnownProxies").Get<string[]>() is string[] knownProxies)
                    {
                        binderOptions.KnownProxies.Clear();
                        foreach (var value in knownProxies)
                        {
                            var address = IPAddress.Parse(value);
                            binderOptions.KnownProxies.Add(address);
                        }
                    }
                });
            }

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
                            IClusterSecurityService security = context.HttpContext.RequestServices
                                .GetRequiredService<IClusterSecurityService>();

                            // Synchronize the lifespan of the passwords with the authentication cookie
                            DateTime validUntil = DateTime.UtcNow.Add(context.Options.ExpireTimeSpan).AddMinutes(1);

                            // Extend life of db login's password
                            await security.SetLoginExpirationAsync(sessionService.User, validUntil);
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                // Keep the default since it requires authenticated users
                // options.DefaultPolicy = ...
            });

            services.AddControllers(options =>
            {
                // Enforce the default authorization policy on controllers
                options.Filters.Add(new AuthorizeFilter());

                // Exception filters we control
                options.Filters.Add<GiantTeamExceptionFilter>();

                // Slugify paths
                options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
            })
            .AddJsonOptions(configure =>
            {
                // Ensure actions and action filters have access to formatter exception messages
                configure.AllowInputFormatterExceptionMessages = true;

                // Be more forgiving about JSON input
                configure.JsonSerializerOptions.AllowTrailingCommas = true;
                configure.JsonSerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
                // TODO: Write a JsonEnumConverter that can deserialize from string or integer,
                // and serialize to int. Since JsonStringEnumConverter serializes to string
                // that is in conflict with using enums in TypeScript.
                //configure.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.Configure<ApiBehaviorOptions>(options =>
            {
                // Disabling the automatic model binding validation so that
                // GiantTeamExceptionFilter will get used instead.
                options.SuppressModelStateInvalidFilter = true;
            });

            // More info at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "API",
                    Description = "This API may change at anytime. It is provided \"as is\", without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose and noninfringement.",
                });
            });

            services.AddOrReplaceScopedFromAssembly(typeof(WebAppServiceBuilder).Assembly);

            services.AddHostedService<StartupWorker>();
        }
    }
}
