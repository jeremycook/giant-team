using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Postgres;
using GiantTeam.Postgres.Models;
using GiantTeam.UserData.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace GiantTeam.Organization.Services;

public class QueryOrganizationService
{
    private readonly ILogger<QueryOrganizationService> logger;
    private readonly ValidationService validationService;
    private readonly UserDataServiceFactory userDataServiceFactory;

    public QueryOrganizationService(
        ILogger<QueryOrganizationService> logger,
        ValidationService validationService,
        UserDataServiceFactory userDataServiceFactory)
    {
        this.logger = logger;
        this.validationService = validationService;
        this.userDataServiceFactory = userDataServiceFactory;
    }

    public async Task<QueryTable> QueryOrganizationAsync(QueryOrganizationInput input)
    {
        validationService.Validate(input);

        try
        {
            var dataService = userDataServiceFactory.NewDataService(input.OrganizationId);
            QueryTable output = await dataService.QueryTableAsync(Sql.Raw(input.Sql));
            return output;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
            throw new NotFoundException("Database not found.");
        }
    }
}

public class QueryOrganizationInput
{
    [RequiredGuid]
    public Guid OrganizationId { get; set; }

    [Required, Regex("^\\s*SELECT\\s.+$", RegexOptions.IgnoreCase | RegexOptions.Multiline, ErrorMessage = "The {0} must start with \"SELECT\".")]
    public string Sql { get; set; } = null!;
}
