using GiantTeam.Cluster.Directory.Helpers;
using GiantTeam.Cluster.Directory.Services;
using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Linq;
using GiantTeam.Postgres;
using GiantTeam.UserData.Services;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organization.Services
{
    public class GrantTableInput
    {
        [RequiredGuid]
        public Guid OrganizationId { get; set; }

        [Required, StringLength(50), InodeName]
        public string SpaceName { get; set; } = null!;

        [Required, StringLength(50), InodeName]
        public string TableName { get; set; } = null!;

        [Required, MinLength(1)]
        public List<GrantTableInputGrant> Grants { get; set; } = null!;
    }

    public class GrantTableInputGrant
    {
        public Guid OrganizationRoleId { get; set; }
        public GrantTableInputPrivilege[] Privileges { get; set; } = null!;
    }

    public enum GrantTableInputPrivilege
    {
        SELECT = 0,
        INSERT = 1,
        UPDATE = 2,
        DELETE = 3,
        TRUNCATE = 4,
        REFERENCES = 5,
        TRIGGER = 6,
        ALL = 100,
    }

    public class GrantTableResult
    {
    }

    public class GrantTableService
    {
        private readonly ILogger<GrantTableService> logger;
        private readonly ValidationService validationService;
        private readonly FetchOrganizationService fetchOrganizationService;
        private readonly UserDataServiceFactory userDataServiceFactory;

        public GrantTableService(
            ILogger<GrantTableService> logger,
            ValidationService validationService,
            FetchOrganizationService fetchOrganizationService,
            UserDataServiceFactory userDataServiceFactory)
        {
            this.logger = logger;
            this.validationService = validationService;
            this.fetchOrganizationService = fetchOrganizationService;
            this.userDataServiceFactory = userDataServiceFactory;
        }

        public async Task<GrantTableResult> GrantTableAsync(GrantTableInput input)
        {
            try
            {
                return await ProcessAsync(input);
            }
            catch (Exception exception) when (exception.GetBaseException() is PostgresException ex)
            {
                logger.LogError(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                throw new ValidationException($"An error occurred that prevented granting access to the {input.SpaceName}.{input.TableName} table. {ex.MessageText.TrimEnd('.')}. {ex.Detail}", ex);
            }
        }

        private async Task<GrantTableResult> ProcessAsync(GrantTableInput input)
        {
            validationService.Validate(input);

            var elevatedDataService = userDataServiceFactory.NewElevatedDataService(input.OrganizationId);

            string schemaName = input.SpaceName;
            string tableName = input.TableName;

            // Grant the SCHEMA as the pg_database_owner.
            var commands = new List<Sql>()
            {
                Sql.Format($"SET ROLE pg_database_owner"),
            };
            commands.AddRange(input.Grants.SelectMany(p => new[]
            {
                Sql.Format($"REVOKE ALL ON TABLE {Sql.Identifier(schemaName, tableName)} FROM {Sql.Identifier(DirectoryHelpers.OrganizationRole(p.OrganizationRoleId)!)}"),
                Sql.Format($"GRANT {(p.Privileges.Contains(GrantTableInputPrivilege.ALL) ?
                    Sql.Raw(GrantTableInputPrivilege.ALL.ToString()) :
                    Sql.Raw(p.Privileges.Select(o => o.ToString()).Join(","))
                )} ON TABLE {Sql.Identifier(schemaName, tableName)} TO {Sql.Identifier(DirectoryHelpers.OrganizationRole(p.OrganizationRoleId)!)}"),
            }));

            await elevatedDataService.ExecuteAsync(commands);

            return new()
            {
            };
        }
    }
}
