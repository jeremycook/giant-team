using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.ComponentModel.DataAnnotations;
using WebApp.Data;

namespace WebApp.Services
{
    public class JoinService
    {
        private readonly ILogger<JoinService> logger;
        private readonly IDbContextFactory<GiantTeamDbContext> dbContextFactory;
        private readonly ValidationService validationService;

        public JoinService(
            ILogger<JoinService> logger,
            IDbContextFactory<GiantTeamDbContext> dbContextFactory,
            ValidationService validationService)
        {
            this.logger = logger;
            this.dbContextFactory = dbContextFactory;
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
                Name = joinDataModel.Name,
                Email = joinDataModel.Email,
                Username = joinDataModel.Username,
                PasswordDigest = HashingHelper.HashPlaintext(joinDataModel.Password),
                Created = DateTimeOffset.UtcNow,
            };

            using (var db = await dbContextFactory.CreateDbContextAsync())
            using (var tx = await db.Database.BeginTransactionAsync())
            {
                // Create database user
                try
                {
                    await db.CreateDatabaseUserRolesAsync(user);
                }
                catch (PostgresException ex) when (ex.SqlState == "42710")
                {
                    logger.LogWarning(ex, "Suppressed exception creating database users named \"{Username}\" {ExceptionType}: {ExceptionMessage}", user.UsernameNormalized, ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                    throw new ValidationException($"The username \"{user.Username}\" already exists.", ex);
                }

                // Create application user
                try
                {
                    db.Users.Add(user);
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, "Suppressed exception creating application user named \"{DisplayUsername}\" {ExceptionType}: {ExceptionMessage}", user.Username, ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                    throw new ValidationException($"The username \"{user.Username}\" already exists.", ex);
                }

                await tx.CommitAsync();
            }
        }
    }
}
