using GiantTeam.Postgres;
using GiantTeam.RecordsManagement.Data;
using GiantTeam.WorkspaceAdministration.Data;
using GiantTeam.WorkspaceInteraction.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Services
{
    public class CreateWorkspaceService
    {
        public class CreateWorkspaceInput
        {
            [Required]
            [PgLaxIdentifier]
            [StringLength(50, MinimumLength = 3)]
            public string? WorkspaceName { get; set; }

            public Guid? OwningTeamId { get; set; }
        }

        public class CreateWorkspaceOutput
        {
            public CreateWorkspaceOutput(CreateWorkspaceStatus status)
            {
                Status = status;
            }

            public CreateWorkspaceStatus Status { get; }

            public string? Message { get; init; }

            public string? WorkspaceId { get; set; }
        }

        public enum CreateWorkspaceStatus
        {
            /// <summary>
            /// Creation was not successful.
            /// Check out <see cref="CreateWorkspaceOutput.Message"/>.
            /// </summary>
            Problem = 400,

            /// <summary>
            /// Created the workspace.
            /// Check out the <see cref="CreateWorkspaceOutput.WorkspaceId"/>.
            /// </summary>
            Success = 200,
        }

        private readonly WorkspaceAdministrationDbContext wa;
        private readonly ValidationService validationService;
        private readonly SessionService sessionService;
        private readonly RecordsManagementDbContext db;
        private readonly CreateTeamService createTeamService;

        public CreateWorkspaceService(
            WorkspaceAdministrationDbContext workspaceAdministrationDbContext,
            ValidationService validationService,
            SessionService sessionService,
            RecordsManagementDbContext recordsManagementDbContext,
            CreateTeamService createTeamService)
        {
            this.db = recordsManagementDbContext;
            this.wa = workspaceAdministrationDbContext;
            this.validationService = validationService;
            this.sessionService = sessionService;
            this.createTeamService = createTeamService;
        }

        public async Task<CreateWorkspaceOutput> CreateWorkspaceAsync(CreateWorkspaceInput input)
        {
            try
            {
                return await ProcessAsync(input);
            }
            catch (ValidationException ex)
            {
                return new(CreateWorkspaceStatus.Problem)
                {
                    Message = ex.Message,
                };
            }
            catch (Exception)
            {
                // TODO: Handle key constraint violations gracefully
                throw;
            }
        }

        private async Task<CreateWorkspaceOutput> ProcessAsync(CreateWorkspaceInput input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            validationService.Validate(input);

            Guid teamId;
            if (input.OwningTeamId is not null)
            {
                teamId = input.OwningTeamId.Value;
            }
            else
            {
                string candiateTeamName = input.WorkspaceName + " Owners";
                var createTeamOutput = await createTeamService.CreateAsync(new()
                {
                    TeamName = candiateTeamName,
                });

                if (createTeamOutput.Status != CreateTeamService.CreateTeamStatus.Success)
                {
                    throw new ServiceException($"A workspace was not created because a team named \"{candiateTeamName}\" could be not created for the workspace. " + createTeamOutput.Message);
                }

                teamId = createTeamOutput.TeamId!.Value;
            }

            var sessionUser = sessionService.User;
            var owner = await db.Teams
                .Where(o => o.TeamId == teamId && o.Users!.Any(u => u.UserId == sessionUser.UserId))
                .SingleOrDefaultAsync() ??
                throw new ServiceException("The owning team was either not found, or you are not an immediate member of it.");

            var workspace = new Workspace()
            {
                WorkspaceId = input.WorkspaceName!,
                WorkspaceName = input.WorkspaceName!,
                OwnerId = owner.TeamId,
                Created = DateTimeOffset.UtcNow,
            };
            validationService.ValidateAll(workspace);
            db.Workspaces.Add(workspace);

            using var tx = await db.Database.BeginTransactionAsync();
            await db.SaveChangesAsync();

            string owningTeamRoleQuoted = PgQuote.Identifier(owner.DbRoleId);
            string databaseNameQuoted = PgQuote.Identifier(workspace.WorkspaceId);

            // Create the database
            await wa.Database.ExecuteSqlRawAsync($"""
GRANT {owningTeamRoleQuoted} TO CURRENT_USER;
CREATE DATABASE {databaseNameQuoted} OWNER {owningTeamRoleQuoted};
REVOKE {owningTeamRoleQuoted} FROM CURRENT_USER;
""");

            await tx.CommitAsync();

            return new(CreateWorkspaceStatus.Success)
            {
                WorkspaceId = workspace.WorkspaceId,
            };
        }
    }
}
