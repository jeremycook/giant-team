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
    public class GrantSpaceInput
    {
        [Required, StringLength(50)]
        public string OrganizationId { get; set; } = null!;

        [Required, StringLength(50), DatumName]
        public string SpaceName { get; set; } = null!;

        [Required, MinLength(1)]
        public List<GrantSpaceInputGrant> Grants { get; set; } = null!;
    }

    public class GrantSpaceInputGrant
    {
        public Guid OrganizationRoleId { get; set; }
        public GrantSpaceInputPrivilege[] Privileges { get; set; } = null!;
    }

    public enum GrantSpaceInputPrivilege
    {
        USAGE = 0,
        CREATE = 1,
        ALL = 100,
    }

    public class GrantSpaceResult
    {
    }

    public class GrantSpaceService
    {
        private readonly ILogger<GrantSpaceService> logger;
        private readonly ValidationService validationService;
        private readonly FetchOrganizationService fetchOrganizationService;
        private readonly UserDataServiceFactory userDataServiceFactory;

        public GrantSpaceService(
            ILogger<GrantSpaceService> logger,
            ValidationService validationService,
            FetchOrganizationService fetchOrganizationService,
            UserDataServiceFactory userDataServiceFactory)
        {
            this.logger = logger;
            this.validationService = validationService;
            this.fetchOrganizationService = fetchOrganizationService;
            this.userDataServiceFactory = userDataServiceFactory;
        }

        public async Task<GrantSpaceResult> GrantSpaceAsync(GrantSpaceInput input)
        {
            try
            {
                return await ProcessAsync(input);
            }
            catch (Exception exception) when (exception.GetBaseException() is PostgresException ex)
            {
                logger.LogError(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                throw new ValidationException($"An error occurred that prevented creation of the \"{input.SpaceName}\" space. {ex.MessageText.TrimEnd('.')}. {ex.Detail}", ex);
            }
        }

        private async Task<GrantSpaceResult> ProcessAsync(GrantSpaceInput input)
        {
            validationService.Validate(input);

            var organization = await fetchOrganizationService.FetchOrganizationAsync(new() { OrganizationId = input.OrganizationId });
            var elevatedDataService = userDataServiceFactory.NewElevatedDataService(organization.DatabaseName);

            string schemaName = input.SpaceName;

            // Grant the SCHEMA as the pg_database_owner.
            var commands = new List<Sql>()
            {
                Sql.Format($"SET ROLE pg_database_owner"),
            };
            commands.AddRange(input.Grants.SelectMany(p => new[]
            {
                Sql.Format($"REVOKE ALL ON SCHEMA {Sql.Identifier(schemaName)} FROM {Sql.Identifier(DirectoryHelpers.OrganizationRole(p.OrganizationRoleId)!)}"),
                Sql.Format($"GRANT {(p.Privileges.Contains(GrantSpaceInputPrivilege.ALL) ?
                    Sql.Raw(GrantSpaceInputPrivilege.ALL.ToString()) :
                    Sql.Raw(p.Privileges.Select(o => o.ToString()).Join(","))
                )} ON SCHEMA {Sql.Identifier(schemaName)} TO {Sql.Identifier(DirectoryHelpers.OrganizationRole(p.OrganizationRoleId)!)}"),
            }));

            await elevatedDataService.ExecuteAsync(commands);

            return new()
            {
            };
        }
    }
}
