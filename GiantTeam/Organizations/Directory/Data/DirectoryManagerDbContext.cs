using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Organizations.Directory.Data;

public class DirectoryManagerDbContext : DirectoryDbContext<DirectoryManagerDbContext>
{
    public DirectoryManagerDbContext(DbContextOptions<DirectoryManagerDbContext> options)
        : base(options) { }
}
