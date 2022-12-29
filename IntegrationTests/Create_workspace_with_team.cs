using GiantTeam.WorkspaceAdministration.Services;
using GiantTeam.Workspaces.Models;
using GiantTeam.Workspaces.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using static GiantTeam.Authentication.Api.Controllers.LoginController;
using static GiantTeam.UserManagement.Services.JoinService;

namespace IntegrationTests;

public class Create_workspace_with_team : IClassFixture<WebApplicationFactory<WebApp.Program>>
{
    private readonly WebApplicationFactory<WebApp.Program> _factory;

    public Create_workspace_with_team(WebApplicationFactory<WebApp.Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Journey()
    {
        // Arrange
        var client = _factory.CreateClient();
        string workspaceName = $"Test {GetType().Name} {DateTime.Now:ddHHmmss}";
        string workspaceOwner = workspaceName + ":Owner";

        // Register and login with fixed credentials that may already exist
        {
            // Register
            using var registerResponse = await client.PostAsJsonAsync("/api/register", new JoinInput()
            {
                Name = "Test User",
                Email = "testuser@example.com",
                Username = Constants.Username,
                Password = Constants.Password,
            });
            // Ignore registration response

            // Login
            using var loginResponse = await client.PostAsJsonAsync("/api/login", new LoginInput()
            {
                Username = Constants.Username,
                Password = Constants.Password,
            });
            loginResponse.EnsureSuccessStatusCode();

            // Authenticate next request with cookie
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
        }

        // Get workspace
        {
            using var fetchWorkspaceResponse = await client.PostAsJsonAsync("/api/fetch-workspace", new FetchWorkspaceInput()
            {
                WorkspaceName = workspaceName,
            });
            fetchWorkspaceResponse.EnsureSuccessStatusCode();
            var fetchWorkspaceOutput = await fetchWorkspaceResponse.Content.ReadFromJsonAsync<Workspace>();

            Assert.NotNull(fetchWorkspaceOutput);
            Assert.Equal(workspaceName, fetchWorkspaceOutput.Name);
            Assert.Equal(workspaceOwner, fetchWorkspaceOutput.Owner);
        }
    }
}