using GiantTeam.Authentication.Api.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using static GiantTeam.Authentication.Api.Controllers.RegisterController;
using static GiantTeam.Authentication.Api.Controllers.LoginController;
using static GiantTeam.Authentication.Api.Controllers.SessionController;
using static GiantTeam.Authentication.Api.Controllers.LogoutController;

namespace IntegrationTests;

public class Authentication : IClassFixture<WebApplicationFactory<WebApp.Program>>
{
    private readonly WebApplicationFactory<WebApp.Program> _factory;

    public Authentication(WebApplicationFactory<WebApp.Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Journey()
    {
        // Arrange
        var client = _factory.CreateClient();
        var registerInput = new RegisterInput()
        {
            Name = "Test User",
            Email = "testuser@example.com",
            Username = "test-user",
            Password = "test-password",
            PasswordConfirmation = "test-password",
        };

        try
        {
            // Register with fixed credentials
            // Ignore response, it may already exist and that's fine
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
                var loginOutput = await loginResponse.Content.ReadFromJsonAsync<LoginOutput>();
                Assert.Equal(LoginStatus.Authenticated, loginOutput?.Status);
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
                Assert.Equal(SessionStatus.Authenticated, sessionOutput?.Status);
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