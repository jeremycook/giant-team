using Microsoft.AspNetCore.Mvc.Testing;
using static GiantTeam.Authentication.Api.Controllers.LoginController;
using static GiantTeam.UserManagement.Services.CreateTeamService;
using static GiantTeam.UserManagement.Services.JoinService;
using static GiantTeam.WorkspaceAdministration.Services.CreateWorkspaceService;
using static GiantTeam.WorkspaceAdministration.Services.FetchWorkspaceService;

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
        string workspaceOwner = workspaceName + " Team";

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

        // Create team
        {
            using var createTeamResponse = await client.PostAsJsonAsync("/api/create-team", new CreateTeamInput()
            {
                TeamName = "My " + workspaceName + " Team",
            });
            createTeamResponse.EnsureSuccessStatusCode();
            var createTeamOutput = await createTeamResponse.Content.ReadFromJsonAsync<CreateTeamOutput>();

            Assert.NotNull(createTeamOutput);
            Assert.NotNull(createTeamOutput.TeamId);

            // Save for later
            workspaceOwner = createTeamOutput.TeamId;
        }

        // Create workspace
        {
            using var createWorkspaceResponse = await client.PostAsJsonAsync("/api/create-workspace", new CreateWorkspaceInput()
            {
                WorkspaceName = workspaceName,
                WorkspaceOwner = workspaceOwner,
            });
            createWorkspaceResponse.EnsureSuccessStatusCode();
            var createWorkspaceOutput = await createWorkspaceResponse.Content.ReadFromJsonAsync<CreateWorkspaceOutput>();

            Assert.NotNull(createWorkspaceOutput);
            Assert.Equal(workspaceName, createWorkspaceOutput.WorkspaceName);
        }

        // Get workspace
        {
            using var fetchWorkspaceResponse = await client.PostAsJsonAsync("/api/fetch-workspace", new FetchWorkspaceInput()
            {
                WorkspaceName = workspaceName,
            });
            fetchWorkspaceResponse.EnsureSuccessStatusCode();
            var fetchWorkspaceOutput = await fetchWorkspaceResponse.Content.ReadFromJsonAsync<FetchWorkspaceOutput>();

            Assert.NotNull(fetchWorkspaceOutput);
            Assert.Equal(workspaceName, fetchWorkspaceOutput.WorkspaceName);
            Assert.Equal(workspaceOwner, fetchWorkspaceOutput.WorkspaceOwner);
        }
    }
}