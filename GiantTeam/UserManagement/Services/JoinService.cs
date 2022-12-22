using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Crypto;
using GiantTeam.RecordsManagement.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.UserManagement.Services
{
    public class JoinService
    {
        public class JoinInput
        {
            [Required, StringLength(100)]
            public string Name { get; set; } = default!;

            [Required, StringLength(200), EmailAddress]
            public string Email { get; set; } = default!;

            [Required, StringLength(50), PgIdentifier]
            public string Username { get; set; } = default!;

            [Required, StringLength(100, MinimumLength = 10), DataType(DataType.Password)]
            public string Password { get; set; } = default!;
        }

        public class JoinOutput
        {
            public Guid UserId { get; set; }
        }

        private readonly ILogger<JoinService> logger;
        private readonly RecordsManagementDbContext recordsManagementDbContext;
        private readonly DatabaseSecurityService security;
        private readonly ValidationService validationService;

        public JoinService(
            ILogger<JoinService> logger,
            RecordsManagementDbContext recordsManagementDbContext,
            DatabaseSecurityService databaseSecurityService,
            ValidationService validationService)
        {
            this.logger = logger;
            this.recordsManagementDbContext = recordsManagementDbContext;
            this.security = databaseSecurityService;
            this.validationService = validationService;
        }

        public async Task<JoinOutput> JoinAsync(JoinInput input)
        {
            validationService.Validate(input);

            DbRole dbRole = new()
            {
                RoleId = input.Username,
                Created = DateTimeOffset.UtcNow,
            };
            User user = new()
            {
                UserId = Guid.NewGuid(),
                Name = input.Name,
                Email = input.Email,
                Username = input.Username,
                Created = DateTimeOffset.UtcNow,
                DbRoleId = dbRole.RoleId,
            };
            UserPassword userPassword = new()
            {
                UserId = user.UserId,
                PasswordDigest = PasswordHelper.HashPlaintext(input.Password),
            };

            validationService.ValidateAll(dbRole, user, userPassword);
            recordsManagementDbContext.DbRoles.Add(dbRole);
            recordsManagementDbContext.Users.Add(user);
            recordsManagementDbContext.UserPasswords.Add(userPassword);

            // Create user record
            using var recordsManagementTx = await recordsManagementDbContext.Database.BeginTransactionAsync();
            try
            {
                await recordsManagementDbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                logger.LogWarning(ex, "Suppressed exception creating application user named \"{Username}\" {ExceptionType}: {ExceptionMessage}", user.Username, ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                throw new ValidationException($"The username \"{user.Username}\" already exists.", ex);
            }

            // Create database user
            try
            {
                await security.CreateUserAsync(user.DbRoleId);
            }
            catch (PostgresException ex) when (ex.SqlState == "42710")
            {
                logger.LogWarning(ex, "Suppressed exception creating database user named \"{DbRole}\" {ExceptionType}: {ExceptionMessage}", user.DbRoleId, ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                throw new ValidationException($"The username \"{user.Username}\" already exists.", ex);
            }

            await recordsManagementTx.CommitAsync();

            logger.LogInformation("Registered {Username} with {UserId}.", input.Username, user.UserId);

            return new()
            {
                UserId = user.UserId,
            };
        }
    }
}
