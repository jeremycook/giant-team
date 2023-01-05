using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organizations.Services;
using GiantTeam.Postgres;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organizations.Directory.Services
{
    public class FetchOrganizationService
    {
        private readonly ILogger<FetchOrganizationService> logger;
        private readonly ValidationService validationService;
        private readonly DirectoryDataService directoryDataService;

        public FetchOrganizationService(
            ILogger<FetchOrganizationService> logger,
            ValidationService validationService,
            DirectoryDataService directoryDataService)
        {
            this.logger = logger;
            this.validationService = validationService;
            this.directoryDataService = directoryDataService;
        }

        public async Task<Models.Organization> FetchOrganizationAsync(FetchOrganizationInput input)
        {
            validationService.Validate(input);

            try
            {
                var output = await directoryDataService.SingleAsync<Models.Organization>(Sql.Format($"WHERE organization_id = {input.OrganizationId}"));
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
}
