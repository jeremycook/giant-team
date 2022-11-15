using GiantTeam.Asp.Startup;
using GiantTeam.Postgres;
using GiantTeam.RecordsManagement.Data;
using GiantTeam.RecordsManagement.Services;
using GiantTeam.Services;
using GiantTeam.WorkspaceAdministration.Data;
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
        private static SessionUser sessionUser = null!;
        private static string workspaceName = null!;
        private static WebApplication application = null!;

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
            Assert.AreEqual(workspaceName, workspace.WorkspaceName);
        }

        #region Configuration

        [ClassInitialize]
        public static void ClassInit(TestContext _)
        {
            workspaceName = "workspace-tests-workspace";

            sessionUser = null!;

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
                .AddUserSecrets(typeof(WebApp.Program).Assembly, true, true)
                .AddUserSecrets(typeof(WorkspaceTests).Assembly, true, true);

            builder
                .ConfigureServicesWithServiceBuilders<WebAppServiceBuilder>();

            builder.Services
                .RemoveAll<SessionService>()
                .AddScoped<SessionService>(s => new StubSessionService(sessionUser));

            application = builder.Build();

            using var scope = application.Services.CreateScope();
            var joinService = scope.ServiceProvider.GetRequiredService<JoinService>();
            var databaseAdministrationDbContext = scope.ServiceProvider.GetRequiredService<WorkspaceAdministrationDbContext>();
            var recordsManagementDbContext = scope.ServiceProvider.GetRequiredService<RecordsManagementDbContext>();

            var joinOutput = joinService.JoinAsync(new()
            {
                Email = "test.user@example.com",
                Name = "workspace-tests-user",
                Password = Guid.NewGuid().ToString(),
                Username = "workspace-tests-user",
            })
                .GetAwaiter().GetResult();

            User user = recordsManagementDbContext.Users.Find(joinOutput.UserId)!;

            string dbLogin = databaseAdministrationDbContext.CreateDatabaseLoginAsync(user.UsernameNormalized)
                .GetAwaiter().GetResult();
            string dbPassword = Guid.NewGuid().ToString();
            DateTimeOffset validUntil = DateTimeOffset.UtcNow.AddMinutes(1);

            databaseAdministrationDbContext.SetDatabasePasswordsAsync(dbLogin, dbPassword, validUntil)
                .GetAwaiter().GetResult();

            sessionUser = new(
                sub: user.UserId.ToString(),
                username: user.Username,
                name: user.Name,
                email: user.Email,
                emailVerified: user.EmailVerified,
                dbLogin: dbLogin,
                dbPassword: dbPassword,
                dbRole: user.UsernameNormalized
            );
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // TODO: Clean up database objects that were created by tests
            using var scope = application.Services.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<WorkspaceAdministrationDbContext>();
            using var rm = scope.ServiceProvider.GetRequiredService<RecordsManagementDbContext>();

            // Cleanup database
            db.Database.ExecuteSqlRaw($"""
DROP DATABASE IF EXISTS {PgQuote.Identifier(workspaceName)};
""");

            // Then roles that depend on it
            var dropRoles = db.Database
                .SqlQuery<string>($"""
SELECT format('DROP ROLE IF EXISTS %I;', rolname)
FROM pg_roles
WHERE rolname ILIKE '%{workspaceName}%' OR rolname ILIKE '%{sessionUser.DbRole}%';
""")
                .ToList();
            db.Database.ExecuteSqlRaw(string.Join("\n", dropRoles));

            rm.Workspaces.Where(o => o.WorkspaceId == workspaceName).ExecuteDelete();
            rm.Users.Where(o => o.Username == sessionUser.Username).ExecuteDelete();
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await application.DisposeAsync();

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}