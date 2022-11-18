using GiantTeam.Crypto;
using Microsoft.AspNetCore.Mvc.Testing;
using static GiantTeam.Authentication.Api.Controllers.LoginController;
using static GiantTeam.Authentication.Api.Controllers.RegisterController;
using static GiantTeam.Authentication.Api.Controllers.SessionController;

namespace IntegrationTests;

public class Register_login_logout : IClassFixture<WebApplicationFactory<WebApp.Program>>
{
    private readonly WebApplicationFactory<WebApp.Program> _factory;

    public Register_login_logout(WebApplicationFactory<WebApp.Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Journey()
    {
        // Arrange
        var client = _factory.CreateClient();
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        string username = $"Test {GetType().Name} {DateTime.Now:ddHHmmss}";
        string password = PasswordHelper.GeneratePassword();
        Guid userId = Guid.Empty;

        try
        {
            // Register
            {
                using var registerResponse = await client.PostAsJsonAsync("/api/register", new RegisterInput()
                {
                    Name = $"Test User {utcNow:yy-MM-dd HHmmss}",
                    Email = $"test.user+{utcNow:yyMMddHHmmss}@example.com",
                    Username = username,
                    Password = password,
                    PasswordConfirmation = password,
                });
                registerResponse.EnsureSuccessStatusCode();
                var registerOutput = await registerResponse.Content.ReadFromJsonAsync<RegisterOutput>();
                Assert.NotNull(registerOutput);
                Assert.Null(registerOutput.Message);
                Assert.Equal(RegisterStatus.Success, registerOutput.Status);
                Assert.NotNull(registerOutput.UserId);

                // Save for later
                userId = registerOutput.UserId.Value;
            }

            // Login
            {
                using var loginResponse = await client.PostAsJsonAsync("/api/login", new LoginInput()
                {
                    Username = username,
                    Password = password,
                });
                loginResponse.EnsureSuccessStatusCode();
                Assert.True(
                    loginResponse.Headers.TryGetValues("Set-Cookie", out var cookies) &&
                    cookies.Any(c => c.StartsWith(".AspNetCore.Cookies=") && c.EndsWith("; path=/; secure; samesite=lax; httponly")),
                    userMessage: "Missing Set-Cookie .AspNetCore.Cookies authentication header"
                );

                // Use the authentication cookie to make authenticated requests
                client.DefaultRequestHeaders.Add("Cookie", cookies.Where(c => c.StartsWith(".AspNetCore.Cookies=")));
            }

            // Check session
            {
                using var sessionResponse = await client.PostAsJsonAsync("/api/session", new { });
                sessionResponse.EnsureSuccessStatusCode();
                var sessionOutput = await sessionResponse.Content.ReadFromJsonAsync<SessionOutput>();
                Assert.NotNull(sessionOutput);
                Assert.Equal(SessionStatus.Authenticated, sessionOutput.Status);
                Assert.Equal(userId, sessionOutput.UserId);
                Assert.Equal(username, sessionOutput.Username);
            }

            // Logout
            {
                using var logoutResponse = await client.PostAsJsonAsync("/api/logout", new { });
                logoutResponse.EnsureSuccessStatusCode();
                Assert.True(
                    logoutResponse.Headers.TryGetValues("Set-Cookie", out var logoutSetCookies) &&
                    logoutSetCookies.Any(c => c == ".AspNetCore.Cookies=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/; secure; samesite=lax; httponly"),
                    userMessage: "Missing Set-Cookie .AspNetCore.Cookies expire authentication header"
                );
            }
        }
        finally
        {
            // TODO: Cleanup
            //using var scope = _factory.Services.CreateScope();
        }
    }
}