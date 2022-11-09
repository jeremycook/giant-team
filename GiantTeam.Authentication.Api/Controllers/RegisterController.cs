using GiantTeam.Postgres;
using GiantTeam.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Authentication.Api.Controllers;

[AllowAnonymous]
public class RegisterController : ControllerBase
{
    public class RegisterInput
    {
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = default!;

        [EmailAddress]
        [StringLength(200, MinimumLength = 3)]
        public string Email { get; set; } = default!;

        [PgLaxIdentifier]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = default!;

        [StringLength(100, MinimumLength = 10)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        [StringLength(100, MinimumLength = 10)]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string PasswordConfirmation { get; set; } = default!;
    }

    public class RegisterOutput
    {
        public RegisterOutput(RegisterStatus status)
        {
            Status = status;
        }

        public RegisterStatus Status { get; }

        public string? Message { get; init; }
    }

    public enum RegisterStatus
    {
        /// <summary>
        /// Something about the input is invalid.
        /// Clients should present the <see cref="RegisterOutput.Message"/>.
        /// </summary>
        Error = 400,

        /// <summary>
        /// A user was created using the supplied information.
        /// Clients should ask the user to login.
        /// </summary>
        Success = 200,
    }

    [HttpPost("/api/[Controller]")]
    public async Task<RegisterOutput> Post(
        [FromServices] JoinService joinService,
        RegisterInput input)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await joinService.JoinAsync(new JoinDataModel
                {
                    Name = input.Name,
                    Email = input.Email,
                    Username = input.Username,
                    Password = input.Password,
                });

                return new(RegisterStatus.Success);
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }

        return new(RegisterStatus.Error)
        {
            Message = string.Join(" ", ModelState.SelectMany(e => e.Value?.Errors ?? Enumerable.Empty<ModelError>()).Select(e => e.ErrorMessage)),
        };
    }
}
