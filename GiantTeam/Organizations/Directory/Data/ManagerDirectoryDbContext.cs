using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Organizations.Directory.Data;

public class ManagerDirectoryDbContext : DirectoryDbContext<ManagerDirectoryDbContext>
{
    public ManagerDirectoryDbContext(DbContextOptions<ManagerDirectoryDbContext> options)
        : base(options) { }
}
