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
        private readonly SessionService sessionService;
        private readonly WorkspaceConnectionService connectionService;
        private readonly RecordsManagementDbContext recordsManagementDbContext;
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
            /// Workspace not found.
            /// Clients should present the <see cref="DeleteWorkspaceOutput.Message"/>.
            /// </summary>
            NotFound = 404,

            /// <summary>
            /// Workspace deleted.
            /// </summary>
            Success = 200,
        }

        public DeleteWorkspaceService(
            SessionService sessionService,
            WorkspaceConnectionService connectionService,
            RecordsManagementDbContext recordsManagementDbContext,
            ValidationService validationService)
        {
            this.sessionService = sessionService;
            this.connectionService = connectionService;
            this.recordsManagementDbContext = recordsManagementDbContext;
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

            var workspaceId = input.WorkspaceId!;


            var workspace = await recordsManagementDbContext
                .Workspaces
                .FirstOrDefaultAsync(o => o.WorkspaceId == workspaceId);

            if (workspace is null)
            {
                return new(DeleteWorkspaceStatus.NotFound)
                {
                    Message = "Workspace not found."
                };
            }
            else if (workspace.OwnerId != sessionService.User.UserId)
            {
                return new(DeleteWorkspaceStatus.Problem)
                {
                    Message = "A workspace can only be deleted by its owner."
                };
            }
            else if (!workspace.Recycle)
            {
                return new(DeleteWorkspaceStatus.Problem)
                {
                    Message = "The workspace is not flagged for recycling. The recycle flag must be set before it can be deleted."
                };
            }

            // Continue and delete the entire workspace

            using var workspaceConnection = await connectionService.OpenMaintenanceConnectionAsync(workspaceId);

            // Delete the database
            var droppedDatabases = await workspaceConnection.ExecuteAsync($"""
DROP DATABASE IF EXISTS {PgQuote.Identifier(workspaceId)};
""");

            // Delete the database owner
            var droppedRoles = await workspaceConnection.ExecuteAsync($"""
DROP ROLE IF EXISTS {PgQuote.Identifier(workspaceId)};
""");

            // Delete workspace record
            var deletedWorkspaces = await recordsManagementDbContext
                .Workspaces
                .Where(o => o.WorkspaceId == workspaceId)
                .ExecuteDeleteAsync();

            return new(DeleteWorkspaceStatus.Success);
        }
    }
}
