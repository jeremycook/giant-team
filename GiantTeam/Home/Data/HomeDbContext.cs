using GiantTeam.Postgres;
using GiantTeam.WorkspaceAdministration.Services;
using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Home.Data
{
    public class HomeDbContext : DbContext
    {
        private readonly UserConnectionService userConnectionService;

        public HomeDbContext(UserConnectionService userConnectionService)
        {
            My = new(this);
            this.userConnectionService = userConnectionService;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var connection = userConnectionService.CreateConnection("Home");

            options.UseNpgsql(connection);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            My.OnModelCreating(modelBuilder);
        }

        public MySchema My { get; }
    }
}
