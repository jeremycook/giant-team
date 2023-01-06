using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Postgres;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organizations.Directory.Services
{
    public class FetchOrganizationService
    {
        private readonly ILogger<FetchOrganizationService> logger;
        private readonly ValidationService validationService;
        private readonly UserDirectoryDataService directoryDataService;

        public FetchOrganizationService(
            ILogger<FetchOrganizationService> logger,
            ValidationService validationService,
            UserDirectoryDataService directoryDataService)
        {
            this.logger = logger;
            this.validationService = validationService;
            this.directoryDataService = directoryDataService;
        }

        public async Task<FetchOrganizationOutput> FetchOrganizationAsync(FetchOrganizationInput input)
        {
            validationService.Validate(input);

            try
            {
                var output = await directoryDataService.SingleAsync<FetchOrganizationOutput>(Sql.Format($"FROM organizations WHERE organization_id = {input.OrganizationId}"));
                return output;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
            }

            throw new NotFoundException("Organization not found.");
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
