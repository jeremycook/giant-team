using GiantTeam.ComponentModel.Services;
using GiantTeam.DatabaseModeling.Models;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace GiantTeam.WorkspaceAdministration.Services
{
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

        public async Task<FetchWorkspaceOutput> FetchWorkspaceAsync(FetchWorkspaceInput input)
        {
            validationService.Validate(input);

            try
            {
                using var connection = await connectionService.OpenConnectionAsync(input.WorkspaceName!);
                using var cmd = connection.CreateCommand();
                cmd.CommandText = $"""
SELECT "Name", "Owner", "Schemas"
FROM gt.workspace
""";

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var output = new FetchWorkspaceOutput()
                    {
                        Name = reader.GetString("Name"),
                        Owner = reader.GetString("Owner"),
                        Schemas = reader.GetFieldValue<Schema[]>("Schemas"),
                    };
                    return output;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
            }

            throw new ValidationException($"Workspace not found.");
        }
    }
}
