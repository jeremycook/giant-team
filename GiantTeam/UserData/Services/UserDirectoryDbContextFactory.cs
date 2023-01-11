using GiantTeam.Cluster.Directory.Data;
using GiantTeam.Cluster.Directory.Helpers;
using GiantTeam.Startup;

namespace GiantTeam.UserData.Services
{
    [Service]
    public class UserDirectoryDbContextFactory
    {
        private readonly UserDbContextFactory userDbContextFactory;

        public UserDirectoryDbContextFactory(
            UserDbContextFactory userDbContextFactory)
        {
            this.userDbContextFactory = userDbContextFactory;
        }

        public UserDirectoryDbContext NewDbContext()
        {
            var directoryDb = userDbContextFactory.NewDbContext<UserDirectoryDbContext>(DirectoryHelpers.Database, DirectoryHelpers.Schema);
            return directoryDb;
        }
    }
}
