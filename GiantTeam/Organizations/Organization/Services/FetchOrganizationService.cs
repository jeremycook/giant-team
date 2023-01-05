using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Organizations.Organization.Services
{
    public class FetchOrganizationService
    {
        private readonly ILogger<FetchOrganizationService> logger;
        private readonly ValidationService validationService;
        private readonly UserDataService dataService;

        public FetchOrganizationService(
            ILogger<FetchOrganizationService> logger,
            ValidationService validationService,
            UserDataService dataService)
        {
            this.logger = logger;
            this.validationService = validationService;
            this.dataService = dataService;
        }

        public async Task<Models.Organization> FetchOrganizationAsync(FetchOrganizationInput input)
        {
            validationService.Validate(input);

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

    public class FetchOrganizationInput
    {
        [Required]
        public string OrganizationId { get; set; } = null!;
    }
}
