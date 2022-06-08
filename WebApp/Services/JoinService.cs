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
        private readonly EncryptionService encryptionService;
        private readonly ValidationService validationService;

        public JoinService(
            ILogger<JoinService> logger,
            IDbContextFactory<GiantTeamDbContext> dbContextFactory,
            EncryptionService encryptionService,
            ValidationService validationService)
        {
            this.logger = logger;
            this.dbContextFactory = dbContextFactory;
            this.encryptionService = encryptionService;
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
                PasswordDigest = encryptionService.HashPlaintext(joinDataModel.Password),
                Created = DateTimeOffset.UtcNow,
            };

            using (var db = await dbContextFactory.CreateDbContextAsync())
            using (var tx = await db.Database.BeginTransactionAsync())
            {
                // Create database user
                try
                {
                    await db.Database.ExecuteSqlRawAsync($@"CREATE USER {PgQuote.Identifier(user.UsernameLowercase)} WITH
LOGIN
NOSUPERUSER
NOCREATEDB
NOCREATEROLE
NOREPLICATION
ADMIN CURRENT_USER;");
                }
                catch (PostgresException ex) when (ex.SqlState == "42710")
                {
                    logger.LogWarning(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                    throw new ValidationException($"The username \"{user.Username}\" already exists.", ex);
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, "Suppressed {ExceptionType}: {ExceptionMessage}", ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                    throw new ValidationException($"The username \"{user.Username}\" already exists.", ex);
                }

                // Add application user
                db.Users.Add(user);
                await db.SaveChangesAsync();

                await tx.CommitAsync();
            }
        }
    }
}
