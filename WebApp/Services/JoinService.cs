using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.ComponentModel.DataAnnotations;
using WebApp.Data;
using WebApp.Postgres;

namespace WebApp.Services
{
    public class JoinService
    {
        private readonly ILogger<JoinService> logger;
        private readonly IDbContextFactory<GiantTeamDbContext> dbContextFactory;
        private readonly HashingService hashingService;
        private readonly ValidationService validationService;

        public JoinService(
            ILogger<JoinService> logger,
            IDbContextFactory<GiantTeamDbContext> dbContextFactory,
            HashingService hashingService,
            ValidationService validationService)
        {
            this.logger = logger;
            this.dbContextFactory = dbContextFactory;
            this.hashingService = hashingService;
            this.validationService = validationService;
        }

        public async Task JoinAsync(JoinDataModel joinDataModel)
        {
            if (joinDataModel is null)
            {
                throw new ArgumentNullException(nameof(joinDataModel));
            }

            validationService.Validate(joinDataModel);

            User user = new()
            {
                DisplayName = joinDataModel.DisplayName,
                Email = joinDataModel.Email,
                Username = joinDataModel.Username,
                PasswordDigest = hashingService.HashPlaintext(joinDataModel.Password),
                DatabaseUsername = "u:" + hashingService.RandomPassword(),
                Created = DateTimeOffset.UtcNow,
            };

            using (var db = await dbContextFactory.CreateDbContextAsync())
            using (var tx = await db.Database.BeginTransactionAsync())
            {
                // Create database user
                try
                {
                    await db.Database.ExecuteSqlRawAsync($@"CREATE USER {PgQuote.Identifier(user.DatabaseUsername)} WITH
LOGIN
NOSUPERUSER
NOCREATEDB
NOCREATEROLE
NOREPLICATION
ADMIN CURRENT_USER;");
                }
                catch (PostgresException ex) when (ex.SqlState == "42710")
                {
                    logger.LogWarning(ex, "Suppressed exception creating database user {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                    throw new ValidationException($"The username \"{user.DatabaseUsername}\" already exists.", ex);
                }

                try
                {
                    // Create application user
                    db.Users.Add(user);
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, "Suppressed exception creating application user {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                    throw new ValidationException($"The username \"{user.Username}\" already exists.", ex);
                }

                await tx.CommitAsync();
            }
        }
    }
}
