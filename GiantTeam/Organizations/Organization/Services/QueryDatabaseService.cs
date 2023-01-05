using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Organizations.Services;
using GiantTeam.Postgres;
using GiantTeam.Postgres.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace GiantTeam.Databases.Database.Services;

public class QueryDatabaseService
{
    private readonly ILogger<QueryDatabaseService> logger;
    private readonly ValidationService validationService;
    private readonly DirectoryDataService directoryDataService;

    public QueryDatabaseService(
        ILogger<QueryDatabaseService> logger,
        ValidationService validationService,
        DirectoryDataService directoryDataService)
    {
        this.logger = logger;
        this.validationService = validationService;
        this.directoryDataService = directoryDataService;
    }

    public async Task<QueryTable> QueryDatabaseAsync(QueryDatabaseProps props)
    {
        validationService.Validate(props);

        try
        {
            var dataService = directoryDataService.CloneDataService(props.DatabaseName);
            QueryTable output = await directoryDataService.QueryTableAsync(Sql.Raw(props.Sql));
            return output;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
        }

        throw new NotFoundException("Database not found.");
    }
}

public class QueryDatabaseProps
{
    [Required, DatabaseName]
    public string DatabaseName { get; set; } = null!;

    [Required, Regex("^\\s*SELECT\\s.+$", RegexOptions.IgnoreCase | RegexOptions.Multiline, ErrorMessage = "The {0} must start with \"SELECT\".")]
    public string Sql { get; set; } = null!;
}
