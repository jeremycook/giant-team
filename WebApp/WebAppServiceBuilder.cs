﻿using GiantTeam.Asp;
using GiantTeam.Asp.Routing;
using GiantTeam.Data;
using GiantTeam.Services;
using GiantTeam.Startup;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Net;
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

            services.AddControllers(options =>
            {
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