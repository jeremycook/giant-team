using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Postgres;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Cluster.Directory.Services
{
    public class FetchOrganizationService
    {
        private readonly ValidationService validationService;
        private readonly UserDirectoryDataService directoryDataService;

        public FetchOrganizationService(
            ValidationService validationService,
            UserDirectoryDataService directoryDataService)
        {
            this.validationService = validationService;
            this.directoryDataService = directoryDataService;
        }

        /// <summary>
        /// Returns organization information from the directory.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="System.Data.Common.DbException"></exception>
        /// <exception cref="NotFoundException"></exception>
        public async Task<FetchOrganizationOutput> FetchOrganizationAsync(FetchOrganizationInput input)
        {
            validationService.Validate(input);

            var output = await directoryDataService
                .SingleOrDefaultAsync<FetchOrganizationOutput>(Sql.Format($"FROM directory.organization WHERE organization_id = {input.OrganizationId}")) ??
                throw new NotFoundException($"The \"{input.OrganizationId}\" organization was not found.");

            return output;
        }
    }

    public class FetchOrganizationInput
    {
        [Required]
        public string OrganizationId { get; set; } = null!;
    }

    public class FetchOrganizationOutput
    {
        public string OrganizationId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public DateTimeOffset Created { get; private set; }
    }
}
