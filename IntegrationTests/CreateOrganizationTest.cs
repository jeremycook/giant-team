using GiantTeam.Cluster.Directory.Services;
using GiantTeam.Organization.Etc.Models;
using GiantTeam.Organization.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using static GiantTeam.Authentication.Api.Controllers.LoginController;
using static GiantTeam.UserManagement.Services.JoinService;

namespace IntegrationTests;

public class CreateOrganizationTest : IClassFixture<WebApplicationFactory<WebApp.Program>>
{
    private readonly WebApplicationFactory<WebApp.Program> _factory;

    public CreateOrganizationTest(WebApplicationFactory<WebApp.Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Journey()
    {
        // Arrange
        var client = _factory.CreateClient();
        string organizationName = $"Test {DateTime.Now:MMdd HHmmss}";
        string databaseName = organizationName.ToLower().Replace(" ", "_");
        Guid organizationId;
        OrganizationDetails organizationDetails;

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
                Elevated = true,
            });
            if (!loginResponse.IsSuccessStatusCode) throw new Exception(loginResponse.StatusCode + ": " + await loginResponse.Content.ReadAsStringAsync());

            // Authenticate next request with cookie
            var setCookie = loginResponse.Headers.GetValues("Set-Cookie");
            client.DefaultRequestHeaders.Add("Cookie", setCookie);
        }

        // Create organization
        {
            using var response = await client.PostAsJsonAsync("/api/cluster/create-organization", new CreateOrganizationInput()
            {
                Name = organizationName,
                DatabaseName = databaseName,
            });
            if (!response.IsSuccessStatusCode) throw new Exception(response.StatusCode + ": " + await response.Content.ReadAsStringAsync());
            var data = await response.Content.ReadFromJsonAsync<CreateOrganizationResult>();

            Assert.NotNull(data);
            organizationId = data.OrganizationId;
        }

        // Get organizations
        {
            using var response = await client.PostAsJsonAsync("/api/cluster/fetch-organizations", new { });
            if (!response.IsSuccessStatusCode) throw new Exception(response.StatusCode + ": " + await response.Content.ReadAsStringAsync());
            var data = await response.Content.ReadFromJsonAsync<FetchOrganizationsOutput>();

            Assert.NotNull(data);

            var organization = data.Organizations.FirstOrDefault(o => o.OrganizationId == organizationId);
            Assert.NotNull(organization);
            Assert.Equal(organizationId, organization.OrganizationId);
            Assert.Equal(organizationName, organization.Name);
            Assert.Equal(databaseName, organization.DatabaseName);
        }

        // Get organization
        {
            using var response = await client.PostAsJsonAsync("/api/organization/fetch-organization-details", new FetchOrganizationDetailsInput { OrganizationId = organizationId });
            if (!response.IsSuccessStatusCode) throw new Exception(response.StatusCode + ": " + await response.Content.ReadAsStringAsync());
            var data = (await response.Content.ReadFromJsonAsync<OrganizationDetails>())!;

            Assert.NotNull(data);
            Assert.Equal(organizationId, data.OrganizationId);
            Assert.Equal(organizationName, data.RootInode.Name);
            Assert.Single(data.RootChildren);
            Assert.Equal(3, data.Roles.Count());

            organizationDetails = data;
        }

        // Create a table
        {
            var ownerRole = organizationDetails.Roles.Single(o => o.Name == "Owner");
            var adminRole = organizationDetails.Roles.Single(o => o.Name == "Admin");
            var memberRole = organizationDetails.Roles.Single(o => o.Name == "Member");
            var homeInode = organizationDetails.RootChildren.Single(o => o.UglyName == "home");
            using var response = await client.PostAsJsonAsync("/api/organization/create-table", new CreateTableInput
            {
                OrganizationId = organizationId,
                ParentInodeId = homeInode.InodeId,
                TableName = "Test Table",
                AccessControlList = new()
                {
                    new() { RoleId = ownerRole.RoleId, Permissions = new[] { PermissionId.m } },
                    new() { RoleId = adminRole.RoleId, Permissions = new[] { PermissionId.r, PermissionId.a, PermissionId.w, PermissionId.d } },
                    new() { RoleId = memberRole.RoleId, Permissions = new[] { PermissionId.r } },
                }
            });
            if (!response.IsSuccessStatusCode) throw new Exception(response.StatusCode + ": " + await response.Content.ReadAsStringAsync());
            var data = await response.Content.ReadFromJsonAsync<Inode>();

            Assert.NotNull(data);
            Assert.Equal("Test Table", data.Name);
            Assert.Equal("test_table", data.UglyName);
            Assert.Equal(InodeTypeId.Table, data.InodeTypeId);
        }
    }
}