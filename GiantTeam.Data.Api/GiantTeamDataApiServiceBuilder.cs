using GiantTeam.Organizations.Organization.Services;
using GiantTeam.Startup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace GiantTeam.Asp
{
    public class GiantTeamDataApiServiceBuilder : IServiceBuilder
    {
        public GiantTeamDataApiServiceBuilder(
            IServiceCollection services,
            GiantTeamAspServiceBuilder giantTeamAspServiceBuilder)
        {
            services.AddScoped(svc =>
            {
                var httpContext = svc.GetRequiredService<IHttpContextAccessor>().HttpContext;
                if (httpContext?.Request.Query.TryGetValue("organization", out var value) == true)
                {
                    return new OrganizationInfo(value.ToString());
                }
                else
                {
                    return new OrganizationInfo(null);
                }
            });

            services.AddScoped(svc =>
            {
                var httpContext = svc.GetRequiredService<IHttpContextAccessor>().HttpContext;
                if (httpContext?.Request.Query.TryGetValue("space", out var value) == true)
                {
                    return new SpaceInfo(value.ToString());
                }
                else
                {
                    return new SpaceInfo(null);
                }
            });
        }
    }
}
