using GiantTeam.Data;
using GiantTeam.EntityFramework;
using GiantTeam.Postgres;
using GiantTeam.Startup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace GiantTeam;

public class GiantTeamServiceBuilder : IServiceBuilder
{
    public GiantTeamServiceBuilder(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScopedFromAssembly(typeof(GiantTeamServiceBuilder).Assembly);

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
    }
}
