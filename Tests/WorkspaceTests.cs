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
        private readonly WebApplication app;

        public WorkspaceTests()
        {
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
                .AddScoped<SessionService>(s => new StubSessionService(SessionUser));

            app = builder.Build();
        }

        public SessionUser SessionUser { get; } = new SessionUser(
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
        public string WorkspaceName { get; } = "Tests-" + Guid.NewGuid();

        [TestMethod]
        public async Task CreateWorkspace()
        {
            using var scope = app.Services.CreateScope();
            var joinService = scope.ServiceProvider.GetRequiredService<JoinService>();
            var mainDbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<GiantTeamDbContext>>();
            var createWorkspaceService = scope.ServiceProvider.GetRequiredService<CreateWorkspaceService>();
            var workspaceService = scope.ServiceProvider.GetRequiredService<WorkspaceService>();

            await joinService.JoinAsync(new()
            {
                Email = SessionUser.Email,
                Name = SessionUser.Name,
                Password = Guid.NewGuid().ToString(),
                Username = SessionUser.Username,
            });
            using var mainDbContext = await mainDbContextFactory.CreateDbContextAsync();
            await mainDbContext.SetDatabaseUserPasswordsAsync(SessionUser.DatabaseUsername, SessionUser.DatabaseSlot, SessionUser.DatabasePassword, SessionUser.DatabasePasswordValidUntil);

            await createWorkspaceService.CreateWorkspaceAsync(new()
            {
                WorkspaceName = WorkspaceName,
            });
            var workspace = await workspaceService.GetWorkspaceAsync(WorkspaceName);

            Assert.IsNotNull(workspace);
            Assert.AreEqual(WorkspaceName, workspace.WorkspaceId);
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            // TODO: Clean up database objects

            await app.DisposeAsync();

            GC.SuppressFinalize(this);
        }
    }
}