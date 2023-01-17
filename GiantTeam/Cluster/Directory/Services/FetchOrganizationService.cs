using GiantTeam.Cluster.Directory.Data;
using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.UserData.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Cluster.Directory.Services
{
    public class FetchOrganizationService
    {
        private readonly ValidationService validationService;
        private readonly UserDirectoryDbContextFactory userDirectoryDbContextFactory;

        public FetchOrganizationService(
            ValidationService validationService,
            UserDirectoryDbContextFactory userDirectoryDbContextFactory)
        {
            this.validationService = validationService;
            this.userDirectoryDbContextFactory = userDirectoryDbContextFactory;
        }

        /// <summary>
        /// Returns organization information from the directory if the user has access to it.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="System.Data.Common.DbException"></exception>
        /// <exception cref="NotFoundException"></exception>
        public async Task<FetchOrganizationOutput> FetchOrganizationAsync(FetchOrganizationInput input)
        {
            validationService.Validate(input);

            await using var db = userDirectoryDbContextFactory.NewDbContext();
            var organization = await db.Organizations
                .Include(o => o.Roles)
                .SingleOrDefaultAsync(o => o.OrganizationId == input.OrganizationId)
                ?? throw new NotFoundException($"The \"{input.OrganizationId}\" organization was not found.");

            var output = new FetchOrganizationOutput(organization);

            return output;
        }
    }

    public class FetchOrganizationInput
    {
        [RequiredGuid]
        public Guid OrganizationId { get; set; }
    }

    public class FetchOrganizationOutput
    {
        public FetchOrganizationOutput(Data.Organization organization)
        {
            OrganizationId = organization.OrganizationId;
            Name = organization.Name;
            DatabaseName = organization.DatabaseName;
            DatabaseOwnerOrganizationRoleId = organization.DatabaseOwnerOrganizationRoleId;
            Created = organization.Created;
            Roles = organization.Roles!.Select(r => new FetchOrganizationOutputRole(r)).ToArray();
        }

        public Guid OrganizationId { get; set; }
        public string Name { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public Guid DatabaseOwnerOrganizationRoleId { get; set; }
        public DateTime Created { get; set; }
        public FetchOrganizationOutputRole[] Roles { get; set; }
    }

    public class FetchOrganizationOutputRole
    {
        public FetchOrganizationOutputRole(OrganizationRole role)
        {
            OrganizationRoleId = role.OrganizationRoleId;
            Name = role.Name;
            Description = role.Description;
            DbRole = role.DbRole;
            Created = role.Created;
        }

        public Guid OrganizationRoleId { get; set; }
        public DateTime Created { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string DbRole { get; set; } = null!;
    }
}
