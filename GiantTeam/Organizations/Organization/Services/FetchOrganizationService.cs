using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organizations.Organization.Services
{
    public class FetchOrganizationService
    {
        private readonly ILogger<FetchOrganizationService> logger;
        private readonly ValidationService validationService;
        private readonly OrganizationDataService dataService;

        public FetchOrganizationService(
            ILogger<FetchOrganizationService> logger,
            ValidationService validationService,
            OrganizationDataService dataService)
        {
            this.logger = logger;
            this.validationService = validationService;
            this.dataService = dataService;
        }

        public async Task<Models.Organization> FetchOrganizationAsync(FetchOrganizationProps props)
        {
            validationService.Validate(props);

            try
            {
                var output = await dataService.SingleAsync<Models.Organization>();
                return output;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
            }

            throw new NotFoundException("Organization not found.");
        }
    }

    public class FetchOrganizationProps
    {
        [Required]
        public string OrganizationId { get; set; } = null!;
    }
}
