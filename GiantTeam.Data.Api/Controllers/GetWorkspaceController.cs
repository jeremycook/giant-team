using GiantTeam.RecordsManagement.Data;
using GiantTeam.RecordsManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Data.Api.Controllers;

public class GetWorkspaceController : ControllerBase
{
    public class GetWorkspaceInput
    {
        [Required]
        public string? WorkspaceId { get; set; }
    }

    public class GetWorkspaceOutput
    {
        public GetWorkspaceOutput(GetWorkspaceStatus status)
        {
            Status = status;
        }

        public GetWorkspaceStatus Status { get; }

        public string? Message { get; init; }

        public Workspace? Workspace { get; set; }
    }

    public enum GetWorkspaceStatus
    {
        /// <summary>
        /// Workspace not found.
        /// Clients should present the <see cref="GetWorkspaceOutput.Message"/>.
        /// </summary>
        Problem = 400,

        /// <summary>
        /// Workspace not found.
        /// Clients should present the <see cref="GetWorkspaceOutput.Message"/>.
        /// </summary>
        NotFound = 404,

        /// <summary>
        /// Workspace found.
        /// Clients may choose to open the workspace for the user.
        /// </summary>
        Found = 200,
    }

    [HttpPost("/api/[Controller]")]
    public async Task<GetWorkspaceOutput> Post(
        [FromServices] WorkspaceService workspaceService,
        GetWorkspaceInput input)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var workspace = await workspaceService
                    .GetWorkspaceAsync(input.WorkspaceId!);

                if (workspace is null)
                {
                    return new(GetWorkspaceStatus.NotFound)
                    {
                        Message = "A workspace with that ID was not found.",
                    };
                }
                else
                {
                    return new(GetWorkspaceStatus.Found)
                    {
                        Workspace = workspace,
                    };
                }
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }

        return new(GetWorkspaceStatus.Problem)
        {
            Message = string.Join(" ", ModelState.SelectMany(e => e.Value?.Errors ?? Enumerable.Empty<ModelError>()).Select(e => e.ErrorMessage)),
        };
    }
}
