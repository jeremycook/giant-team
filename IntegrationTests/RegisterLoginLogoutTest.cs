using GiantTeam.Crypto;
using Microsoft.AspNetCore.Mvc.Testing;
using static GiantTeam.Authentication.Api.Controllers.LoginController;
using static GiantTeam.Authentication.Api.Controllers.SessionController;
using static GiantTeam.UserManagement.Services.JoinService;

namespace IntegrationTests;

public class RegisterLoginLogoutTest : IClassFixture<WebApplicationFactory<WebApp.Program>>
{
    private readonly WebApplicationFactory<WebApp.Program> _factory;

    public RegisterLoginLogoutTest(WebApplicationFactory<WebApp.Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Journey()
    {
        // Arrange
        var client = _factory.CreateClient();
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        string username = $"test_{utcNow:yyMMdd}_{utcNow:HHmmssfff}";
        string password = PasswordHelper.GeneratePassword();
        Guid userId = Guid.Empty;

        try
        {
            // Register
            {
                using var registerResponse = await client.PostAsJsonAsync("/api/register", new JoinInput()
                {
                    Name = $"Test {utcNow:yyMMdd} {utcNow:HHmmssfff}",
                    Email = username + "@example.com",
                    Username = username,
                    Password = password,
                });
                if (!registerResponse.IsSuccessStatusCode) throw new Exception(registerResponse.StatusCode + ": " + await registerResponse.Content.ReadAsStringAsync());

                var registerOutput = await registerResponse.Content.ReadFromJsonAsync<JoinOutput>();
                Assert.NotNull(registerOutput);
                Assert.NotEqual(Guid.Empty, registerOutput.UserId);

                // Save for later
                userId = registerOutput.UserId;
            }

            // Login
            {
                using var loginResponse = await client.PostAsJsonAsync("/api/login", new LoginInput()
                {
                    Username = username,
                    Password = password,
                });
                if (!loginResponse.IsSuccessStatusCode) throw new Exception(loginResponse.StatusCode + ": " + await loginResponse.Content.ReadAsStringAsync());
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
                if (!sessionResponse.IsSuccessStatusCode) throw new Exception(sessionResponse.StatusCode + ": " + await sessionResponse.Content.ReadAsStringAsync());
                var sessionOutput = await sessionResponse.Content.ReadFromJsonAsync<SessionOutput>();
                Assert.NotNull(sessionOutput);
                Assert.Equal(SessionStatus.Authenticated, sessionOutput.Status);
                Assert.Equal(userId, sessionOutput.UserId);
                Assert.Equal(username, sessionOutput.Username);
            }

            // Logout
            {
                using var logoutResponse = await client.PostAsJsonAsync("/api/logout", new { });
                if (!logoutResponse.IsSuccessStatusCode) throw new Exception(logoutResponse.StatusCode + ": " + await logoutResponse.Content.ReadAsStringAsync());
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