using GiantTeam.Startup;
using GiantTeam.UserManagement.Services;
using Microsoft.Extensions.Options;

namespace GiantTeam.Organizations.Organization.Services
{
    [Service]
    public class UserDataServiceFactory
    {
        private readonly ILogger<UserDataService> logger;
        private readonly IOptions<GiantTeamOptions> options;
        private readonly SessionService sessionService;

        public UserDataServiceFactory(ILogger<UserDataService> logger,
            IOptions<GiantTeamOptions> options,
            SessionService sessionService)
        {
            this.logger = logger;
            this.options = options;
            this.sessionService = sessionService;
        }

        /// <summary>
        /// Create a new <see cref="UserDataService"/> that connects to <paramref name="databaseName"/>.
        /// </summary>
        /// <param name="databaseName"></param>
        public UserDataService CreateDataService(string databaseName, string defaultSchema = "spaces")
        {
            return new(logger, options, sessionService, databaseName, defaultSchema);
        }
    }
}
