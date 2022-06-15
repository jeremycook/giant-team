using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Postgres;

namespace WebApp.Services
{
    public class CreateDatabaseService
    {
        private readonly IDbContextFactory<GiantTeamDbContext> dbContextFactory;
        private readonly ValidationService validationService;
        private readonly IdentityService identityService;

        public CreateDatabaseService(IDbContextFactory<GiantTeamDbContext> dbContextFactory, ValidationService validationService, IdentityService identityService)
        {
            this.dbContextFactory = dbContextFactory;
            this.validationService = validationService;
            this.identityService = identityService;
        }

        public async Task<CreateDatabaseOutput> CreateDatabase(CreateDatabaseInput model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            validationService.Validate(model);

            using var db = dbContextFactory.CreateDbContext();

            string dbname = PgQuote.Identifier(model.DatabaseName);
            string dbowner = PgQuote.Identifier(model.DatabaseName + "_owner");
            string dbadmin = PgQuote.Identifier(model.DatabaseName + "_admin");

            await db.Database.ExecuteSqlRawAsync(
$@"CREATE ROLE {dbowner};");

            await db.Database.ExecuteSqlRawAsync(
$@"CREATE DATABASE {dbname};");

            await db.Database.ExecuteSqlRawAsync(
$@"GRANT ALL ON DATABASE {dbname} TO {dbowner};
REVOKE ALL ON DATABASE {dbname} FROM PUBLIC;");

            // Create role

            // Create database roles _admin, _inserter, _updater, _deleter, _viewer

            CreateDatabaseOutput output = new()
            {
                DatabaseName = model.DatabaseName
            };
            validationService.Validate(output);
            return output;

            // Create the user's home database
            // Revoke PUBLIC privileges
            //NpgsqlConnectionStringBuilder builder = new(connectionString)
            //{
            //    Username = user.UsernameLowercase,
            //    Password = dbPassword,
            //    Database = user.DbName(),
            //};
            //using (NpgsqlConnection conn = new(connectionString))
            //{
            //    await conn.OpenAsync();

            //    using NpgsqlBatch batch = new(conn)
            //    {
            //        BatchCommands =
            //        {
            //            new($"CREATE DATABASE {model.DatabaseName}"),
            //            new("ALTER SCHEMA public OWNER TO CURRENT_USER;"),
            //            new("DROP SCHEMA public;"),
            //        }
            //    };
            //    await batch.ExecuteNonQueryAsync();
            //}
        }
    }
}
