using Dapper;
using GiantTeam.Postgres;
using GiantTeam.RecordsManagement.Data;
using GiantTeam.WorkspaceAdministration.Data;
using GiantTeam.WorkspaceInteraction.Services;
using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Services
{
    public class CreateWorkspaceService
    {
        private readonly RecordsManagementDbContext db;
        private readonly WorkspaceAdministrationDbContext workspaceAdministrationDbContext;
        private readonly ValidationService validationService;
        private readonly SessionService sessionService;
        private readonly DatabaseConnectionService databaseConnectionService;

        public CreateWorkspaceService(
            RecordsManagementDbContext db,
            WorkspaceAdministrationDbContext workspaceAdministrationDbContext,
            ValidationService validationService,
            SessionService sessionService,
            DatabaseConnectionService databaseConnectionService)
        {
            this.db = db;
            this.workspaceAdministrationDbContext = workspaceAdministrationDbContext;
            this.validationService = validationService;
            this.sessionService = sessionService;
            this.databaseConnectionService = databaseConnectionService;
        }

        public async Task CreateWorkspaceAsync(CreateWorkspaceInput input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            validationService.Validate(input);

            var sessionUser = sessionService.User;

            string quotedDesignUser = PgQuote.Identifier(DatabaseHelper.DesignUser(sessionUser.DatabaseUsername));
            string quotedManipulateUser = PgQuote.Identifier(DatabaseHelper.ManipulateUser(sessionUser.DatabaseUsername));
            string quotedQueryUser = PgQuote.Identifier(DatabaseHelper.QueryUser(sessionUser.DatabaseUsername));

            string quotedDbName = PgQuote.Identifier(input.WorkspaceName);

            string quotedDesignRole = PgQuote.Identifier(DatabaseHelper.DesignRole(input.WorkspaceName));
            string quotedManipulateRole = PgQuote.Identifier(DatabaseHelper.ManipulateRole(input.WorkspaceName));
            string quotedQueryRole = PgQuote.Identifier(DatabaseHelper.QueryRole(input.WorkspaceName));

            using (var tx = await db.Database.BeginTransactionAsync())
            {
                db.Workspaces.Add(new Workspace
                {
                    WorkspaceId = input.WorkspaceName,
                    OwnerId = sessionUser.UserId,
                    Created = DateTimeOffset.UtcNow,
                });
                await db.SaveChangesAsync();

                await workspaceAdministrationDbContext.Database.ExecuteSqlRawAsync($@"
CREATE ROLE {quotedDesignRole} ROLE {quotedDesignUser}, CURRENT_USER;
CREATE ROLE {quotedManipulateRole} ROLE {quotedManipulateUser};
CREATE ROLE {quotedQueryRole} ROLE {quotedQueryUser};

CREATE DATABASE {quotedDbName} OWNER {quotedDesignRole};

REVOKE {quotedDesignRole} FROM CURRENT_USER;
");

                using (var connection = databaseConnectionService.CreateDesignConnection(input.WorkspaceName))
                {
                    await connection.ExecuteAsync($@"
GRANT ALL ON DATABASE {quotedDbName} TO {quotedDesignRole};
GRANT TEMPORARY, CONNECT ON DATABASE {quotedDbName} TO {quotedManipulateRole};
GRANT CONNECT ON DATABASE {quotedDbName} TO {quotedQueryRole};

REVOKE ALL ON DATABASE {quotedDbName} FROM PUBLIC;

ALTER DEFAULT PRIVILEGES GRANT ALL ON TABLES TO {quotedDesignRole};
ALTER DEFAULT PRIVILEGES GRANT ALL ON TABLES TO {quotedManipulateRole};
ALTER DEFAULT PRIVILEGES GRANT SELECT ON TABLES TO {quotedQueryRole};

ALTER DEFAULT PRIVILEGES GRANT ALL ON SEQUENCES TO {quotedDesignRole};
ALTER DEFAULT PRIVILEGES GRANT SELECT, USAGE ON SEQUENCES TO {quotedManipulateRole};

ALTER DEFAULT PRIVILEGES GRANT EXECUTE ON FUNCTIONS TO {quotedDesignRole};
ALTER DEFAULT PRIVILEGES GRANT EXECUTE ON FUNCTIONS TO {quotedManipulateRole};

ALTER DEFAULT PRIVILEGES GRANT USAGE ON TYPES TO {quotedDesignRole};
ALTER DEFAULT PRIVILEGES GRANT USAGE ON TYPES TO {quotedManipulateRole};
ALTER DEFAULT PRIVILEGES GRANT USAGE ON TYPES TO {quotedQueryRole};
");
                }

                await tx.CommitAsync();
            }
        }
    }
}
