using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Postgres;
using GiantTeam.Postgres.Models;
using GiantTeam.UserData.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace GiantTeam.Organization.Services;

public class QueryService
{
    private readonly ILogger<QueryService> logger;
    private readonly ValidationService validationService;
    private readonly UserDataServiceFactory userDataServiceFactory;

    public QueryService(
        ILogger<QueryService> logger,
        ValidationService validationService,
        UserDataServiceFactory userDataServiceFactory)
    {
        this.logger = logger;
        this.validationService = validationService;
        this.userDataServiceFactory = userDataServiceFactory;
    }

    public async Task<TabularData> QueryAsync(QueryInput input)
    {
        validationService.Validate(input);

        var dataService = userDataServiceFactory.NewDataService(input.OrganizationId);
        TabularData output = await dataService.TabularQueryAsync(Sql.Raw(input.Sql));
        return output;
    }
}

public class QueryInput
{
    [RequiredGuid]
    public Guid OrganizationId { get; set; }

    [Required, Regex("^\\s*SELECT\\s.+$", RegexOptions.IgnoreCase | RegexOptions.Multiline, ErrorMessage = "The {0} must start with \"SELECT\".")]
    public string Sql { get; set; } = null!;
}
