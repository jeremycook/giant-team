using GiantTeam.RecordsManagement.Data;
using GiantTeam.WorkspaceAdministration.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Services
{
    public class JoinService
    {
        private readonly ILogger<JoinService> logger;
        private readonly RecordsManagementDbContext recordsManagementDbContext;
        private readonly WorkspaceAdministrationDbContext databaseAdministrationDbContext;
        private readonly ValidationService validationService;

        public JoinService(
            ILogger<JoinService> logger,
            RecordsManagementDbContext recordsManagementDbContext,
            WorkspaceAdministrationDbContext databaseAdministrationDbContext,
            ValidationService validationService)
        {
            this.logger = logger;
            this.recordsManagementDbContext = recordsManagementDbContext;
            this.databaseAdministrationDbContext = databaseAdministrationDbContext;
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

            // Create user record
            using var recordsManagementTx = await recordsManagementDbContext.Database.BeginTransactionAsync();
            try
            {
                recordsManagementDbContext.Users.Add(user);
                await recordsManagementDbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                logger.LogWarning(ex, "Suppressed exception creating application user named \"{DisplayUsername}\" {ExceptionType}: {ExceptionMessage}", user.Username, ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                throw new ValidationException($"The username \"{user.Username}\" already exists.", ex);
            }

            // Create database user
            try
            {
                await databaseAdministrationDbContext.CreateDatabaseUserRolesAsync(user);
            }
            catch (PostgresException ex) when (ex.SqlState == "42710")
            {
                logger.LogWarning(ex, "Suppressed exception creating database users named \"{Username}\" {ExceptionType}: {ExceptionMessage}", user.UsernameNormalized, ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                throw new ValidationException($"The username \"{user.Username}\" already exists.", ex);
            }

            await recordsManagementTx.CommitAsync();
        }
    }
}
