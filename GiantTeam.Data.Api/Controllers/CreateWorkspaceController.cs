using GiantTeam.Postgres;
using GiantTeam.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Data.Api.Controllers;

public class CreateWorkspaceController : ControllerBase
{
    public class CreateWorkspaceInput
    {
        [Required]
        [PgLaxIdentifier]
        public string? WorkspaceName { get; set; }
    }

    public class CreateWorkspaceOutput
    {
        public CreateWorkspaceOutput(CreateWorkspaceStatus status)
        {
            Status = status;
        }

        public CreateWorkspaceStatus Status { get; }

        public string? ErrorMessage { get; init; }

        public string? WorkspaceId { get; set; }
    }

    public enum CreateWorkspaceStatus
    {
        /// <summary>
        /// Creation was not successful.
        /// Clients should present the <see cref="CreateWorkspaceOutput.ErrorMessage"/>.
        /// </summary>
        Error = 400,

        /// <summary>
        /// Created the workspace.
        /// Clients may choose to open the workspace matching <see cref="CreateWorkspaceOutput.WorkspaceId"/>.
        /// </summary>
        Created = 201,
    }

    [HttpPost("/api/[Controller]")]
    public async Task<CreateWorkspaceOutput> Post(
        [FromServices] CreateWorkspaceService createWorkspaceService,
        CreateWorkspaceInput input)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await createWorkspaceService.CreateWorkspaceAsync(new()
                {
                    WorkspaceName = input.WorkspaceName!,
                });

                return new(CreateWorkspaceStatus.Created)
                {
                    WorkspaceId = input.WorkspaceName,
                };
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }

        return new(CreateWorkspaceStatus.Error)
        {
            ErrorMessage = string.Join(" ", ModelState.SelectMany(e => e.Value?.Errors ?? Enumerable.Empty<ModelError>()).Select(e => e.ErrorMessage)),
        };
    }
}
