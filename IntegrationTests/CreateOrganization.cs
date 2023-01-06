using GiantTeam.Cluster.Directory.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using static GiantTeam.Authentication.Api.Controllers.LoginController;
using static GiantTeam.UserManagement.Services.JoinService;

namespace IntegrationTests;

public class CreateOrganization : IClassFixture<WebApplicationFactory<WebApp.Program>>
{
    private readonly WebApplicationFactory<WebApp.Program> _factory;

    public CreateOrganization(WebApplicationFactory<WebApp.Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Journey()
    {
        // Arrange
        var client = _factory.CreateClient();
        string organizationName = $"Test {DateTime.Now:MMdd HHmmss}";
        string organizationId = organizationName.ToLower().Replace(" ", "_");
        string databaseName = organizationId;

        // Register and login with fixed credentials that may already exist
        {
            // Register
            using var registerResponse = await client.PostAsJsonAsync("/api/register", new JoinInput()
            {
                Name = "Test User",
                Email = Constants.Username + "@example.com",
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
            if (!loginResponse.IsSuccessStatusCode) throw new Exception(loginResponse.StatusCode + ": " + await loginResponse.Content.ReadAsStringAsync());

            // Authenticate next request with cookie
            var setCookie = loginResponse.Headers.GetValues("Set-Cookie");
            client.DefaultRequestHeaders.Add("Cookie", setCookie);
        }

        // Create organization
        {
            using var response = await client.PostAsJsonAsync("/api/create-organization", new CreateOrganizationInput()
            {
                Name = organizationName,
                DatabaseName = databaseName,
            });
            if (!response.IsSuccessStatusCode) throw new Exception(response.StatusCode + ": " + await response.Content.ReadAsStringAsync());
            var data = await response.Content.ReadFromJsonAsync<CreateOrganizationResult>();

            Assert.NotNull(data);
            Assert.Equal(organizationId, data.OrganizationId);
        }

        // Get organization
        {
            using var response = await client.PostAsJsonAsync("/api/fetch-organization", new FetchOrganizationInput()
            {
                OrganizationId = organizationId,
            });
            if (!response.IsSuccessStatusCode) throw new Exception(response.StatusCode + ": " + await response.Content.ReadAsStringAsync());
            var data = await response.Content.ReadFromJsonAsync<FetchOrganizationOutput>();

            Assert.NotNull(data);
            Assert.Equal(organizationId, data.OrganizationId);

            Assert.Equal(organizationName, data.Name);
            Assert.Equal(databaseName, data.DatabaseName);
        }
    }
}