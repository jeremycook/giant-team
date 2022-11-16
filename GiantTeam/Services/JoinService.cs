﻿using GiantTeam.RecordsManagement.Data;
using GiantTeam.WorkspaceAdministration.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Services
{
    public class JoinService
    {
        public class JoinInput
        {
            [Required]
            [StringLength(100, MinimumLength = 3)]
            public string Name { get; set; } = default!;

            [Required]
            [EmailAddress]
            [StringLength(200, MinimumLength = 3)]
            public string Email { get; set; } = default!;

            [Required]
            [RegularExpression("^[A-Za-z][A-Za-z0-9-_]*$")]
            [StringLength(50, MinimumLength = 3)]
            public string Username { get; set; } = default!;

            [Required]
            [StringLength(100, MinimumLength = 10)]
            [DataType(DataType.Password)]
            public string Password { get; set; } = default!;
        }

        public class JoinOutput
        {
            public Guid UserId { get; set; }
        }

        private readonly ILogger<JoinService> logger;
        private readonly RecordsManagementDbContext recordsManagementDbContext;
        private readonly WorkspaceAdministrationDbContext workspaceAdministrationDbContext;
        private readonly ValidationService validationService;

        public JoinService(
            ILogger<JoinService> logger,
            RecordsManagementDbContext recordsManagementDbContext,
            WorkspaceAdministrationDbContext workspaceAdministrationDbContext,
            ValidationService validationService)
        {
            this.logger = logger;
            this.recordsManagementDbContext = recordsManagementDbContext;
            this.workspaceAdministrationDbContext = workspaceAdministrationDbContext;
            this.validationService = validationService;
        }

        public async Task<JoinOutput> JoinAsync(JoinInput joinInputModel)
        {
            if (joinInputModel is null)
            {
                throw new ArgumentNullException(nameof(joinInputModel));
            }

            validationService.Validate(joinInputModel);

            DbRole dbRole = new()
            {
                RoleId = joinInputModel.Username,
                Created = DateTimeOffset.UtcNow,
            };
            User user = new()
            {
                Name = joinInputModel.Name,
                Email = joinInputModel.Email,
                Username = joinInputModel.Username,
                PasswordDigest = HashingHelper.HashPlaintext(joinInputModel.Password),
                Created = DateTimeOffset.UtcNow,
                DbRoleId = dbRole.RoleId,
            };

            // Create user record
            using var recordsManagementTx = await recordsManagementDbContext.Database.BeginTransactionAsync();
            try
            {
                recordsManagementDbContext.DbRoles.Add(dbRole);
                recordsManagementDbContext.Users.Add(user);

                // Validate and then save changes
                validationService.ValidateAll(dbRole, user);
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
                await workspaceAdministrationDbContext.CreateDatabaseUserAsync(user.DbRoleId);
            }
            catch (PostgresException ex) when (ex.SqlState == "42710")
            {
                logger.LogWarning(ex, "Suppressed exception creating database user named \"{DbRole}\" {ExceptionType}: {ExceptionMessage}", user.DbRoleId, ex.GetBaseException().GetType(), ex.GetBaseException().Message);
                throw new ValidationException($"The username \"{user.Username}\" already exists.", ex);
            }

            await recordsManagementTx.CommitAsync();

            return new()
            {
                UserId = user.UserId,
            };
        }
    }
}
