﻿using GiantTeam.Cluster.Directory.Helpers;
using GiantTeam.Postgres;
using Microsoft.Extensions.Options;
using Npgsql;

namespace GiantTeam.Cluster.Directory.Services;

public class DirectoryManagementDataService : PgDataServiceBase
{
    private readonly IOptions<GiantTeamOptions> options;
    private string? _connectionString;

    protected override string ConnectionString
    {
        get
        {
            if (_connectionString is null)
            {
                var connectionOptions = options.Value.DirectoryManagerConnection;
                var connectionStringBuilder = connectionOptions.ToConnectionStringBuilder();

                if (!string.IsNullOrEmpty(connectionStringBuilder.SearchPath))
                {
                    throw new NotSupportedException($"Setting the {nameof(NpgsqlConnectionStringBuilder.SearchPath)} of the {nameof(GiantTeamOptions.UserConnectionString)}.{nameof(ConnectionOptions.ConnectionString)} is not supported.");
                }

                connectionStringBuilder.SearchPath = DirectoryHelpers.Schema;

                _connectionString = connectionStringBuilder.ToString();
            }

            return _connectionString;
        }
    }

    protected override ILogger Logger { get; }

    public DirectoryManagementDataService(
        ILogger<DirectoryManagementDataService> logger,
        IOptions<GiantTeamOptions> options)
    {
        Logger = logger;
        this.options = options;
    }
}
