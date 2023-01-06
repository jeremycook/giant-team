using GiantTeam.ComponentModel;
using GiantTeam.ComponentModel.Services;
using GiantTeam.Crypto;
using GiantTeam.Organizations.Directory.Data;
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

            [Required, StringLength(50), Username]
            public string Username { get; set; } = default!;

            [Required, StringLength(100, MinimumLength = 10), DataType(DataType.Password)]
            public string Password { get; set; } = default!;
        }

        public class JoinOutput
        {
            public Guid UserId { get; set; }
        }

        private readonly ILogger<JoinService> logger;
        private readonly ManagerDirectoryDbContext directoryManagerDb;
        private readonly ClusterSecurityService security;
        private readonly ValidationService validationService;

        public JoinService(
            ILogger<JoinService> logger,
            ManagerDirectoryDbContext directoryManagerDb,
            ClusterSecurityService databaseSecurityService,
            ValidationService validationService)
        {
            this.logger = logger;
            this.directoryManagerDb = directoryManagerDb;
            this.security = databaseSecurityService;
            this.validationService = validationService;
        }

        public async Task<JoinOutput> JoinAsync(JoinInput input)
        {
            validationService.Validate(input);

            User user = new()
            {
                UserId = Guid.NewGuid(),
                Name = input.Name,
                Username = input.Username,
                DbUser = "u:" + input.Username,
                Email = input.Email,
                EmailVerified = false,
                Created = DateTimeOffset.UtcNow,
            };
            UserPassword userPassword = new()
            {
                UserId = user.UserId,
                PasswordDigest = PasswordHelper.HashPlaintext(input.Password),
            };

            validationService.ValidateAll(user, userPassword);
            directoryManagerDb.Users.Add(user);
            directoryManagerDb.UserPasswords.Add(userPassword);

            // Create user record
            using var dmtx = await directoryManagerDb.Database.BeginTransactionAsync();
            try
            {
                await directoryManagerDb.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                logger.LogWarning(ex,
                    "Suppressed exception creating application user named \"{Username}\" {ExceptionType}: {ExceptionMessage}",
                    user.Username, ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                throw new ValidationException($"The username \"{user.Username}\" already exists.", ex);
            }

            // Create database user
            try
            {
                await security.CreateUserRolesAsync(user.DbUser);
            }
            catch (PostgresException ex) when (ex.SqlState == "42710")
            {
                logger.LogWarning(ex, "Suppressed exception creating database user roles \"{DbUser}\" {ExceptionType}: {ExceptionMessage}", user.DbUser, ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                throw new ValidationException($"The username \"{user.Username}\" already exists.", ex);
            }
            catch
            {
                throw;
            }

            await dmtx.CommitAsync();

            logger.LogInformation("Registered {Username} with {UserId}.", input.Username, user.UserId);

            return new()
            {
                UserId = user.UserId,
            };
        }
    }
}
