using GiantTeam.RecordsManagement.Data;
using GiantTeam.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.WorkspaceInteraction.Services
{
    public class RecycleWorkspaceService
    {
        private readonly SessionService sessionService;
        private readonly WorkspaceConnectionService connectionService;
        private readonly RecordsManagementDbContext recordsManagementDbContext;
        private readonly ValidationService validationService;

        public class RecycleWorkspaceInput
        {
            [Required]
            public string? WorkspaceId { get; set; }
        }

        public class RecycleWorkspaceOutput
        {
            public RecycleWorkspaceOutput(RecycleWorkspaceStatus status)
            {
                Status = status;
            }

            public RecycleWorkspaceStatus Status { get; }

            public string? Message { get; init; }
        }

        public enum RecycleWorkspaceStatus
        {
            /// <summary>
            /// Unable to recycle the workspace.
            /// Clients should present the <see cref="RecycleWorkspaceOutput.Message"/>.
            /// </summary>
            Problem = 400,

            /// <summary>
            /// Workspace not found.
            /// Clients should present the <see cref="RecycleWorkspaceOutput.Message"/>.
            /// </summary>
            NotFound = 404,

            /// <summary>
            /// Workspace recycled.
            /// </summary>
            Success = 200,
        }

        public RecycleWorkspaceService(
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

        public async Task<RecycleWorkspaceOutput> RecycleAsync(RecycleWorkspaceInput input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (!validationService.TryValidate(input, out var validationResults))
            {
                return new(RecycleWorkspaceStatus.Problem)
                {
                    Message = string.Join(" ", validationResults.Select(o => o.ErrorMessage)),
                };
            }

            var workspaceId = input.WorkspaceId!;

            // Recycle workspace record
            var recycledWorkspaces = await recordsManagementDbContext
                .Workspaces
                .Where(o =>
                    o.WorkspaceId == workspaceId &&
                    o.OwnerId == sessionService.User.UserId &&
                    !o.Recycle)
                .ExecuteUpdateAsync(updater => updater
                    .SetProperty(o => o.Recycle, o => true)
                );

            if (recycledWorkspaces > 0)
            {
                // TODO: Disable connecting to the database once
                // we have a record of teams.
                return new(RecycleWorkspaceStatus.Success);
            }
            else
            {
                return new(RecycleWorkspaceStatus.NotFound)
                {
                    Message = "Workspace not found."
                };
            }
        }
    }
}
