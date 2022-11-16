using GiantTeam.Postgres;
using GiantTeam.RecordsManagement.Data;
using GiantTeam.WorkspaceAdministration.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Services
{
    public class CreateWorkspaceService
    {
        public class CreateWorkspaceInput
        {
            [PgLaxIdentifier]
            [StringLength(25)]
            public string WorkspaceName { get; set; } = null!;
        }

        public class CreateWorkspaceOutput
        {
            public string WorkspaceId { get; set; } = null!;
        }

        private readonly RecordsManagementDbContext db;
        private readonly WorkspaceAdministrationDbContext workspaceAdministrationDbContext;
        private readonly ValidationService validationService;
        private readonly SessionService sessionService;

        public CreateWorkspaceService(
            RecordsManagementDbContext db,
            WorkspaceAdministrationDbContext workspaceAdministrationDbContext,
            ValidationService validationService,
            SessionService sessionService)
        {
            this.db = db;
            this.workspaceAdministrationDbContext = workspaceAdministrationDbContext;
            this.validationService = validationService;
            this.sessionService = sessionService;
        }

        public async Task<CreateWorkspaceOutput> CreateWorkspaceAsync(CreateWorkspaceInput input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            validationService.Validate(input);

            var sessionUser = sessionService.User;
            var ownerDbRole = new DbRole()
            {
                RoleId = input.WorkspaceName,
                Created = DateTimeOffset.UtcNow,
            };
            var owner = new Team()
            {
                TeamId = Guid.NewGuid(),
                Name = input.WorkspaceName,
                DbRoleId = ownerDbRole.RoleId,
                Created = DateTimeOffset.UtcNow,
                Users = new()
                {
                    new() { UserId = sessionUser.UserId },
                },
            };
            var workspace = new Workspace()
            {
                WorkspaceId = input.WorkspaceName,
                WorkspaceName = input.WorkspaceName,
                OwnerId = owner.TeamId,
                Created = DateTimeOffset.UtcNow,
            };
            // Validate
            validationService.ValidateAll(ownerDbRole, owner, workspace);
            // Insert
            db.DbRoles.Add(ownerDbRole);
            db.Teams.Add(owner);
            db.Workspaces.Add(workspace);
            using var tx = await db.Database.BeginTransactionAsync();
            await db.SaveChangesAsync();

            string quotedDbOwner = PgQuote.Identifier(owner.DbRoleId);
            string quotedDbUser = PgQuote.Identifier(sessionUser.DbRole);
            string quotedDbWorkspace = PgQuote.Identifier(workspace.WorkspaceId);

            await workspaceAdministrationDbContext.Database.ExecuteSqlRawAsync($"""
CREATE ROLE {quotedDbOwner} ROLE {quotedDbUser}, CURRENT_USER;
CREATE DATABASE {quotedDbWorkspace} OWNER {quotedDbOwner};
REVOKE {quotedDbOwner} FROM CURRENT_USER;
""");

            await tx.CommitAsync();

            return new()
            {
                WorkspaceId = workspace.WorkspaceId,
            };
        }
    }
}
