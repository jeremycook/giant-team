using GiantTeam.ComponentModel.Services;
using GiantTeam.Postgres;
using GiantTeam.RecordsManagement.Data;
using GiantTeam.WorkspaceAdministration.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.UserManagement.Services
{
    public class CreateTeamService
    {
        private readonly SessionService sessionService;
        private readonly RecordsManagementDbContext rm;
        private readonly WorkspaceAdministrationDbContext wa;
        private readonly ValidationService validationService;

        public class CreateTeamInput
        {
            [Required]
            [PgLaxIdentifier]
            [StringLength(50, MinimumLength = 3)]
            public string? TeamName { get; set; }
        }

        public class CreateTeamOutput
        {
            public CreateTeamOutput(CreateTeamStatus status)
            {
                Status = status;
            }

            public CreateTeamStatus Status { get; }

            public string? Message { get; init; }

            public Guid? TeamId { get; set; }
        }

        public enum CreateTeamStatus
        {
            /// <summary>
            /// Problem creating the team.
            /// Check <see cref="CreateTeamOutput.Message"/>.
            /// </summary>
            Problem = 400,

            /// <summary>
            /// Team created.
            /// Check <see cref="CreateTeamOutput.TeamId"/>.
            /// </summary>
            Success = 200,
        }

        public CreateTeamService(
            SessionService sessionService,
            RecordsManagementDbContext rm,
            WorkspaceAdministrationDbContext wa,
            ValidationService validationService)
        {
            this.sessionService = sessionService;
            this.rm = rm;
            this.wa = wa;
            this.validationService = validationService;
        }

        public async Task<CreateTeamOutput> CreateAsync(CreateTeamInput input)
        {
            try
            {
                return await ProcessAsync(input);
            }
            catch (ValidationException ex)
            {
                return new(CreateTeamStatus.Problem)
                {
                    Message = ex.Message,
                };
            }
            catch (Exception)
            {
                // TODO: Handle key constraint violations gracefully
                throw;
            }
        }

        private async Task<CreateTeamOutput> ProcessAsync(CreateTeamInput input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            validationService.Validate(input);

            var sessionUser = sessionService.User;
            var teamName = input.TeamName!;

            var dbRole = new DbRole()
            {
                RoleId = teamName,
                Created = DateTimeOffset.UtcNow,
            };
            var team = new Team()
            {
                TeamId = Guid.NewGuid(),
                Name = dbRole.RoleId,
                DbRoleId = dbRole.RoleId,
                Created = DateTimeOffset.UtcNow,
                Users = new()
                {
                    new() { UserId = sessionUser.UserId },
                },
            };

            validationService.ValidateAll(dbRole, team);
            rm.DbRoles.Add(dbRole);
            rm.Teams.Add(team);

            using var tx = await rm.Database.BeginTransactionAsync();
            await rm.SaveChangesAsync();

            string quotedSessionUserRole = PgQuote.Identifier(sessionUser.DbRole);
            string quotedTeamRole = PgQuote.Identifier(team.DbRoleId);

            // Create the team's database role
            // with the session user an ADMIN of it.
            await wa.Database.ExecuteSqlRawAsync($"""
CREATE ROLE {quotedTeamRole} ADMIN {quotedSessionUserRole};
""");

            await tx.CommitAsync();

            return new(CreateTeamStatus.Success)
            {
                TeamId = team.TeamId,
            };
        }
    }
}
