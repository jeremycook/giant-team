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

            if (giantTeamOptions.DominionConnection.SetRole is not null)
            {
                options.AddInterceptors(new OpenedDbConnectionInterceptor($"SET ROLE {PgQuote.Identifier(giantTeamOptions.DominionConnection.SetRole)};"));
            }

            NpgsqlConnectionStringBuilder connectionStringBuilder = new(giantTeamOptions.DominionConnection.ConnectionString);
            if (giantTeamOptions.DominionConnection.Password is not null)
            {
                connectionStringBuilder.Password = giantTeamOptions.DominionConnection.Password;
            }

            NpgsqlConnection connection = new(connectionStringBuilder.ToString());

            if (giantTeamOptions.DominionConnection.CaCertificate is string connectionCaCertificateText)
            {
                connection.ConfigureCaCertificateValidation(connectionCaCertificateText);
            }

            options.UseNpgsql(connection);
        });
    }
}
