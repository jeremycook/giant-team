using GiantTeam.Data;
using GiantTeam.EntityFramework;
using GiantTeam.Postgres;
using GiantTeam.Startup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;

namespace GiantTeam;

public class GiantTeamServiceBuilder : IServiceBuilder
{
    public GiantTeamServiceBuilder(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GiantTeamOptions>(configuration.GetRequiredSection("GiantTeam"));

        services.AddScopedFromAssembly(typeof(GiantTeamServiceBuilder).Assembly);

        services.AddPooledDbContextFactory<GiantTeamDbContext>((services, options) =>
        {
            var giantTeamOptions = services.GetRequiredService<IOptions<GiantTeamOptions>>().Value;

            NpgsqlConnectionStringBuilder connectionStringBuilder = new(giantTeamOptions.MainConnection.ConnectionString);
            if (giantTeamOptions.MainConnection.Password is not null)
            {
                connectionStringBuilder.Password = giantTeamOptions.MainConnection.Password;
            }

            NpgsqlConnection connection = new(connectionStringBuilder.ToString());

            if (giantTeamOptions.MainConnection.CaCertificate is string connectionCaCertificateText)
            {
                connection.ConfigureCaCertificateValidation(connectionCaCertificateText);
            }

            if (giantTeamOptions.MainConnection.SetRole is not null)
            {
                options.AddInterceptors(new OpenedDbConnectionInterceptor($"SET ROLE {PgQuote.Identifier(giantTeamOptions.MainConnection.SetRole)};"));
            }

            options.UseNpgsql(connection);
        });
    }
}
