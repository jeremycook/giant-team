using Dapper;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Postgres;
using GiantTeam.RecordsManagement.Data;
using GiantTeam.UserManagement.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.WorkspaceAdministration.Services
{
    public class DeleteWorkspaceService
    {
        private readonly FetchWorkspaceService fetchWorkspaceService;
        private readonly UserConnectionService connectionService;
        private readonly ValidationService validationService;

        public class DeleteWorkspaceInput
        {
            [Required]
            public string? WorkspaceId { get; set; }
        }

        public class DeleteWorkspaceOutput
        {
            public DeleteWorkspaceOutput(DeleteWorkspaceStatus status)
            {
                Status = status;
            }

            public DeleteWorkspaceStatus Status { get; }

            public string? Message { get; init; }
        }

        public enum DeleteWorkspaceStatus
        {
            /// <summary>
            /// Unable to delete the workspace.
            /// Clients should present the <see cref="DeleteWorkspaceOutput.Message"/>.
            /// </summary>
            Problem = 400,

            /// <summary>
            /// Workspace deleted.
            /// </summary>
            Success = 200,
        }

        public DeleteWorkspaceService(
            FetchWorkspaceService fetchWorkspaceService,
            UserConnectionService connectionService,
            ValidationService validationService)
        {
            this.fetchWorkspaceService = fetchWorkspaceService;
            this.connectionService = connectionService;
            this.validationService = validationService;
        }

        public async Task<DeleteWorkspaceOutput> DeleteAsync(DeleteWorkspaceInput input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (!validationService.TryValidate(input, out var validationResults))
            {
                return new(DeleteWorkspaceStatus.Problem)
                {
                    Message = string.Join(" ", validationResults.Select(o => o.ErrorMessage)),
                };
            }

            var workspaceInfo = await fetchWorkspaceService.FetchWorkspaceAsync(new()
            {
                WorkspaceName = input.WorkspaceId,
            });

            // This will fail if the session user is not a member of the database owner role
            using var workspaceConnection = await connectionService.OpenInfoConnectionAsync(workspaceInfo.WorkspaceOwner);

            // Delete the database
            var droppedDatabases = await workspaceConnection.ExecuteAsync($"""
DROP DATABASE IF EXISTS {PgQuote.Identifier(workspaceInfo.WorkspaceName)};
""");

            return new(DeleteWorkspaceStatus.Success);
        }
    }
}
