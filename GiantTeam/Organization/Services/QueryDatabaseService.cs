using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Postgres;
using GiantTeam.Postgres.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace GiantTeam.Organization.Services;

public class QueryDatabaseService
{
    private readonly ILogger<QueryDatabaseService> logger;
    private readonly ValidationService validationService;
    private readonly UserDataServiceFactory organizationDataFactory;

    public QueryDatabaseService(
        ILogger<QueryDatabaseService> logger,
        ValidationService validationService,
        UserDataServiceFactory organizationDataFactory)
    {
        this.logger = logger;
        this.validationService = validationService;
        this.organizationDataFactory = organizationDataFactory;
    }

    public async Task<QueryTable> QueryDatabaseAsync(QueryDatabaseInput input)
    {
        validationService.Validate(input);

        try
        {
            var dataService = organizationDataFactory.NewDataService(input.DatabaseName);
            QueryTable output = await dataService.QueryTableAsync(Sql.Raw(input.Sql));
            return output;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
        }

        throw new NotFoundException("Database not found.");
    }
}

public class QueryDatabaseInput
{
    [Required, DatabaseName]
    public string DatabaseName { get; set; } = null!;

    [Required, Regex("^\\s*SELECT\\s.+$", RegexOptions.IgnoreCase | RegexOptions.Multiline, ErrorMessage = "The {0} must start with \"SELECT\".")]
    public string Sql { get; set; } = null!;
}
