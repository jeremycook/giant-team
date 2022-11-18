using GiantTeam.Crypto;
using GiantTeam.RecordsManagement.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.UserManagement.Services
{
    public class VerifyPasswordService
    {
        public class VerifyPasswordInput
        {
            [StringLength(50, MinimumLength = 3)]
            public string Username { get; set; } = default!;

            [StringLength(100, MinimumLength = 10)]
            public string Password { get; set; } = default!;
        }

        public class VerifyPasswordOutput
        {
            public Guid UserId { get; set; }
        }

        private readonly RecordsManagementDbContext db;

        public VerifyPasswordService(
            RecordsManagementDbContext db)
        {
            this.db = db;
        }

        /// <summary>
        /// Returns a <see cref="VerifyPasswordOutput"/> if the credentials are valid.
        /// Throws <see cref="ValidationException"/> if they are not.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="ValidationException"></exception>
        public async Task<VerifyPasswordOutput> VerifyUsernameAndPasswordAsync(VerifyPasswordInput input)
        {
            var userPassword = await (
                from u in db.Users
                join up in db.UserPasswords on u.UserId equals up.UserId
                where u.InvariantUsername == input.Username.ToLowerInvariant()
                select up
            ).SingleOrDefaultAsync();

            if (userPassword is not null &&
                !string.IsNullOrEmpty(userPassword.PasswordDigest) &&
                PasswordHelper.VerifyHashedPlaintext(userPassword.PasswordDigest, input.Password))
            {
                // TODO: Log verification success
                return new()
                {
                    UserId = userPassword.UserId,
                };
            }
            else
            {
                // TODO: Log verification failure
                throw new ValidationException($"The username or password is incorrect.");
            }
        }
    }
}
