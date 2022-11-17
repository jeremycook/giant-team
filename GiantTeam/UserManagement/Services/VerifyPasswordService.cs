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
            public VerifyPasswordOutput(VerifyPasswordStatus status)
            {
                Status = status;
            }

            public VerifyPasswordStatus Status { get; }

            public string? Message { get; init; }

            public Guid? UserId { get; set; }
        }

        public enum VerifyPasswordStatus
        {
            /// <summary>
            /// Problem authenticating.
            /// Check <see cref="VerifyPasswordOutput.Message"/>.
            /// </summary>
            Problem = 400,

            /// <summary>
            /// User authenticated.
            /// Check <see cref="VerifyPasswordOutput.UserId"/>.
            /// </summary>
            Success = 200,
        }

        private readonly RecordsManagementDbContext db;

        public VerifyPasswordService(
            RecordsManagementDbContext db)
        {
            this.db = db;
        }

        public async Task<VerifyPasswordOutput> VerifyAsync(VerifyPasswordInput input)
        {
            var userPassword = await (
                from u in db.Users
                join up in db.UserPasswords on u.UserId equals up.UserId
                where u.InvariantUsername == input.Username.ToLowerInvariant()
                select up
            ).SingleOrDefaultAsync();

            if (userPassword is not null &&
                !string.IsNullOrEmpty(userPassword.PasswordDigest) &&
                HashingHelper.VerifyHashedPlaintext(userPassword.PasswordDigest, input.Password))
            {
                return new(VerifyPasswordStatus.Success)
                {
                    UserId = userPassword.UserId,
                };
            }
            else
            {
                // TODO: Log verification failure
                return new(VerifyPasswordStatus.Problem)
                {
                    Message = $"The username or password is incorrect.",
                };
            }
        }
    }
}
