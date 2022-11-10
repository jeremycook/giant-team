using GiantTeam.Asp.Startup;
using GiantTeam.Data;
using GiantTeam.Data.Services;
using GiantTeam.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebApp;

namespace Tests
{
    [TestClass]
    public class WorkspaceTests : IAsyncDisposable
    {
        private readonly SessionUser sessionUser;
        private readonly string workspaceName;
        private readonly WebApplication application;

        [TestMethod]
        public async Task CreateWorkspace()
        {
            using var scope = application.Services.CreateScope();
            var createWorkspaceService = scope.ServiceProvider.GetRequiredService<CreateWorkspaceService>();
            var workspaceService = scope.ServiceProvider.GetRequiredService<WorkspaceService>();

            await createWorkspaceService.CreateWorkspaceAsync(new()
            {
                WorkspaceName = workspaceName,
            });
            var workspace = await workspaceService.GetWorkspaceAsync(workspaceName);

            Assert.IsNotNull(workspace);
            Assert.AreEqual(workspaceName, workspace.WorkspaceId);
        }

        #region Configuration

        public WorkspaceTests()
        {
            workspaceName = "Tests-" + Guid.NewGuid();

            sessionUser = new SessionUser(
                new User()
                {
                    UserId = Guid.NewGuid(),
                    Username = "Tests-" + Guid.NewGuid(),
                    Created = DateTimeOffset.UtcNow,
                    Email = "test.user@example.com",
                    EmailVerified = true,
                    Name = "Test User",
                    PasswordDigest = null!,
                },
                databaseSlot: 1,
                databasePassword: Guid.NewGuid().ToString(),
                databasePasswordValidUntil: DateTimeOffset.UtcNow.AddMinutes(1)
            );

            var builder = WebApplication
                    .CreateBuilder(new WebApplicationOptions()
                    {
                        Args = Array.Empty<string>(),
                        EnvironmentName = "Development",
                        ApplicationName = "WebApp",
                        ContentRootPath = Path.GetFullPath("../../../../WebApp"),
                        WebRootPath = Path.GetFullPath("../../../../WebApp/wwwroot"),
                    });

            builder.Configuration
                .AddJsonFile(Path.GetFullPath("../../../appsettings.json"), true, true)
                .AddJsonFile(Path.GetFullPath($"../../../appsettings.{builder.Environment.EnvironmentName}.json"), true, true)
                .AddUserSecrets(GetType().Assembly, true, true);

            builder
                .ConfigureServicesWithServiceBuilders<WebAppServiceBuilder>();

            builder.Services
                .RemoveAll<SessionService>()
                .AddScoped<SessionService>(s => new StubSessionService(sessionUser));

            application = builder.Build();

            using var scope = application.Services.CreateScope();
            var joinService = scope.ServiceProvider.GetRequiredService<JoinService>();
            var mainDbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<GiantTeamDbContext>>();

            joinService.JoinAsync(new()
            {
                Email = sessionUser.Email,
                Name = sessionUser.Name,
                Password = Guid.NewGuid().ToString(),
                Username = sessionUser.Username,
            }).GetAwaiter().GetResult();
            using var mainDbContext = mainDbContextFactory.CreateDbContextAsync().GetAwaiter().GetResult();
            mainDbContext.SetDatabaseUserPasswordsAsync(sessionUser.DatabaseUsername, sessionUser.DatabaseSlot, sessionUser.DatabasePassword, sessionUser.DatabasePasswordValidUntil).GetAwaiter().GetResult();
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            // TODO: Clean up database objects that were created by tests

            await application.DisposeAsync();

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}