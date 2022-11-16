using GiantTeam.WorkspaceInteraction.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using static GiantTeam.Authentication.Api.Controllers.LoginController;
using static GiantTeam.Authentication.Api.Controllers.RegisterController;
using static GiantTeam.Data.Api.Controllers.CreateWorkspaceController;
using static GiantTeam.Data.Api.Controllers.GetWorkspaceController;

namespace IntegrationTests;

public class Create_workspace_without_team : IClassFixture<WebApplicationFactory<WebApp.Program>>
{
    private readonly WebApplicationFactory<WebApp.Program> _factory;

    public Create_workspace_without_team(WebApplicationFactory<WebApp.Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Journey()
    {
        // Arrange
        var client = _factory.CreateClient();
        string workspaceName = $"Test Workspace {Random.Shared.Next()}";
        string workspaceId = workspaceName;

        try
        {
            // Register with fixed credentials
            // Ignore response, it may already exist and that's fine
            var registerInput = new RegisterInput()
            {
                Name = "Test User",
                Email = "testuser@example.com",
                Username = "test-user",
                Password = "test-password",
                PasswordConfirmation = "test-password",
            };
            {
                using var registerResponse = await client.PostAsJsonAsync("/api/register", registerInput);
                registerResponse.EnsureSuccessStatusCode();
            }

            // Login
            {
                using var loginResponse = await client.PostAsJsonAsync("/api/login", new LoginInput()
                {
                    Username = registerInput.Username,
                    Password = registerInput.Password,
                });
                loginResponse.EnsureSuccessStatusCode();

                // Use the authentication cookie to make authenticated requests
                var setCookie = loginResponse.Headers.GetValues("Set-Cookie");
                client.DefaultRequestHeaders.Add("Cookie", setCookie);
            }

            // Create workspace
            {
                using var createWorkspaceResponse = await client.PostAsJsonAsync("/api/create-workspace", new CreateWorkspaceInput()
                {
                    WorkspaceName = workspaceName,
                });
                createWorkspaceResponse.EnsureSuccessStatusCode();
                var createWorkspaceOutput = await createWorkspaceResponse.Content.ReadFromJsonAsync<CreateWorkspaceOutput>();

                Assert.NotNull(createWorkspaceOutput);
                Assert.Null(createWorkspaceOutput.Message);
                Assert.Equal(CreateWorkspaceStatus.Success, createWorkspaceOutput?.Status);
                Assert.Equal(workspaceId, createWorkspaceOutput?.WorkspaceId);
            }

            // Get workspace
            {
                using var getWorkspaceResponse = await client.PostAsJsonAsync("/api/get-workspace", new GetWorkspaceInput()
                {
                    WorkspaceId = workspaceId,
                });
                getWorkspaceResponse.EnsureSuccessStatusCode();
                var getWorkspaceOutput = await getWorkspaceResponse.Content.ReadFromJsonAsync<GetWorkspaceOutput>();

                Assert.NotNull(getWorkspaceOutput);
                Assert.Null(getWorkspaceOutput.Message);
                Assert.Equal(GetWorkspaceStatus.Success, getWorkspaceOutput.Status);
                Assert.NotNull(getWorkspaceOutput.Workspace);
                Assert.Equal(workspaceId, getWorkspaceOutput.Workspace.WorkspaceId);
                Assert.Equal(workspaceName, getWorkspaceOutput.Workspace.WorkspaceName);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
        finally
        {
            // Cleanup
            using var loginResponse = await client.PostAsJsonAsync("/api/recycle-workspace", new RecycleWorkspaceService.RecycleWorkspaceInput()
            {
                WorkspaceId = workspaceId,
            });
            loginResponse.EnsureSuccessStatusCode();
        }
    }
}