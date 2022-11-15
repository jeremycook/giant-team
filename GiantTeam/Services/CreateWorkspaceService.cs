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

            using var tx = await db.Database.BeginTransactionAsync();
            Workspace workspace = new Workspace
            {
                WorkspaceId = input.WorkspaceName,
                WorkspaceName = input.WorkspaceName,
                OwnerId = sessionUser.UserId,
                Created = DateTimeOffset.UtcNow,
            };
            db.Workspaces.Add(workspace);
            await db.SaveChangesAsync();

            string quotedDbUser = PgQuote.Identifier(sessionUser.DbRole);
            string quotedDbWorkspace = PgQuote.Identifier(workspace.WorkspaceId);

            await workspaceAdministrationDbContext.Database.ExecuteSqlRawAsync($"""
CREATE ROLE {quotedDbWorkspace} ROLE {quotedDbUser}, CURRENT_USER;
CREATE DATABASE {quotedDbWorkspace} OWNER {quotedDbWorkspace};
REVOKE {quotedDbWorkspace} FROM CURRENT_USER;
""");

            //                // TODO: Configure privileges of new database
            //                await newDatabaseConnection.ExecuteSqlAsync($@"
            //GRANT ALL ON DATABASE {quotedDbName} TO {quotedDesignRole};
            //GRANT TEMPORARY, CONNECT ON DATABASE {quotedDbName} TO {quotedManipulateRole};
            //GRANT CONNECT ON DATABASE {quotedDbName} TO {quotedQueryRole};

            //REVOKE ALL ON DATABASE {quotedDbName} FROM PUBLIC;

            //ALTER DEFAULT PRIVILEGES GRANT ALL ON TABLES TO {quotedDesignRole};
            //ALTER DEFAULT PRIVILEGES GRANT ALL ON TABLES TO {quotedManipulateRole};
            //ALTER DEFAULT PRIVILEGES GRANT SELECT ON TABLES TO {quotedQueryRole};

            //ALTER DEFAULT PRIVILEGES GRANT ALL ON SEQUENCES TO {quotedDesignRole};
            //ALTER DEFAULT PRIVILEGES GRANT SELECT, USAGE ON SEQUENCES TO {quotedManipulateRole};

            //ALTER DEFAULT PRIVILEGES GRANT EXECUTE ON FUNCTIONS TO {quotedDesignRole};
            //ALTER DEFAULT PRIVILEGES GRANT EXECUTE ON FUNCTIONS TO {quotedManipulateRole};

            //ALTER DEFAULT PRIVILEGES GRANT USAGE ON TYPES TO {quotedDesignRole};
            //ALTER DEFAULT PRIVILEGES GRANT USAGE ON TYPES TO {quotedManipulateRole};
            //ALTER DEFAULT PRIVILEGES GRANT USAGE ON TYPES TO {quotedQueryRole};
            //");

            await tx.CommitAsync();

            return new()
            {
                WorkspaceId = workspace.WorkspaceId,
            };
        }
    }
}
