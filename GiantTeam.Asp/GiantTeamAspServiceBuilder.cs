using GiantTeam.DataProtection;
using GiantTeam.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace GiantTeam.Asp
{
    public class GiantTeamAspServiceBuilder : IServiceBuilder
    {
        public GiantTeamAspServiceBuilder(
            IServiceCollection services,
            GiantTeamServiceBuilder giantTeamServiceBuilder,
            DataProtectionServiceBuilder dataProtectionServiceBuilder)
        {
            services.AddHttpContextAccessor();
            services.AddScopedFromAssembly(typeof(GiantTeamAspServiceBuilder).Assembly);
        }
    }
}
