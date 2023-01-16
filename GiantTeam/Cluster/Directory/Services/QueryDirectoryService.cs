using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Postgres;
using GiantTeam.Postgres.Models;
using GiantTeam.UserData.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace GiantTeam.Cluster.Directory.Services;

public class QueryDirectoryService
{
    private readonly ILogger<QueryDirectoryService> logger;
    private readonly ValidationService validationService;
    private readonly UserDirectoryDataServiceFactory userDirectoryDataServiceFactory;

    public QueryDirectoryService(
        ILogger<QueryDirectoryService> logger,
        ValidationService validationService,
        UserDirectoryDataServiceFactory userDirectoryDataServiceFactory)
    {
        this.logger = logger;
        this.validationService = validationService;
        this.userDirectoryDataServiceFactory = userDirectoryDataServiceFactory;
    }

    public async Task<QueryTable> QueryDirectoryAsync(QueryDirectoryInput input)
    {
        validationService.Validate(input);

        var dataService = userDirectoryDataServiceFactory.NewDataService();
        var output = await dataService.QueryTableAsync(Sql.Raw(input.Sql));

        return output;
    }
}

public class QueryDirectoryInput
{
    [Required, Regex("^\\s*SELECT\\s.+$", RegexOptions.IgnoreCase | RegexOptions.Multiline, ErrorMessage = "The {0} must start with \"SELECT\".")]
    public string Sql { get; set; } = null!;
}
