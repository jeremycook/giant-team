﻿using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.DatabaseDefinition.Models;
using GiantTeam.Postgres;
using GiantTeam.Text.Json;
using GiantTeam.WorkspaceAdministration.Services;
using GiantTeam.Workspaces.Models;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace GiantTeam.Workspaces.Services
{
    public class FetchWorkspaceInput
    {
        [Required, StringLength(50), PgIdentifier]
        public string? WorkspaceName { get; set; }
    }

    public class FetchWorkspaceService
    {
        private readonly ILogger<FetchWorkspaceService> logger;
        private readonly ValidationService validationService;
        private readonly UserConnectionService connectionService;

        public FetchWorkspaceService(
            ILogger<FetchWorkspaceService> logger,
            ValidationService validationService,
            UserConnectionService connectionService)
        {
            this.logger = logger;
            this.validationService = validationService;
            this.connectionService = connectionService;
        }

        public async Task<Workspace> FetchWorkspaceAsync(FetchWorkspaceInput input)
        {
            validationService.Validate(input);

            try
            {
                using var connection = await connectionService.OpenConnectionAsync(input.WorkspaceName!);
                using var cmd = connection.CreateCommand();
                cmd.CommandText = $"""
SELECT "name", "owner", "zones"
FROM ws.workspace
""";

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var output = new Workspace()
                    {
                        Name = reader.GetString("name"),
                        Owner = reader.GetString("owner"),
                        Zones = reader.GetRequiredFieldValue<Schema[]>("zones", GTJsonSerializerOptions.SnakeCase),
                    };
                    return output;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
            }

            throw new NotFoundException("Workspace not found.");
        }
    }
}