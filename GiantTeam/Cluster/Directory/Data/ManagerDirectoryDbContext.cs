using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Cluster.Directory.Data;

public class ManagerDirectoryDbContext : DirectoryDbContext<ManagerDirectoryDbContext>
{
    public ManagerDirectoryDbContext(DbContextOptions<ManagerDirectoryDbContext> options)
        : base(options) { }
}
