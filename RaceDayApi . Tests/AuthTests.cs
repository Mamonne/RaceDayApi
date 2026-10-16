using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using RaceDayApi.Data;
using RaceDayApi.Models;

namespace RaceDayApi.Tests
{
    public class AuthTests : TestBase
    {
        [Fact]
        public async Task Register_WithValidDetails_Returns201()
        {
            HttpResponseMessage response = await this._client.PostAsJsonAsync(
                "/api/auth/register", this.MakeRegisterBody("new@test.com", "Participant"));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Register_PasswordIsStoredHashed()
        {
            await this._client.PostAsJsonAsync(
                "/api/auth/register", this.MakeRegisterBody("hash@test.com", "Participant", "Pass123"));

            using (IServiceScope scope = this._factory.Services.CreateScope())
            {
                AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                User? user = context.Users.FirstOrDefault(u => u.Email == "hash@test.com");

                Assert.NotNull(user);
                Assert.NotEqual("Pass123", user.PasswordHash);
                Assert.StartsWith("$2", user.PasswordHash);
            }
        }

        [Fact]
        public async Task Register_DuplicateEmail_Returns400()
        {
            await this._client.PostAsJsonAsync(
                "/api/auth/register", this.MakeRegisterBody("same@test.com", "Participant"));

            HttpResponseMessage second = await this._client.PostAsJsonAsync(
                "/api/auth/register", this.MakeRegisterBody("same@test.com", "Participant"));

            Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
        }

        [Fact]
        public async Task Register_InvalidRole_Returns400()
        {
            HttpResponseMessage response = await this._client.PostAsJsonAsync(
                "/api/auth/register", this.MakeRegisterBody("admin@test.com", "Admin"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_ShortPassword_Returns400()
        {
            HttpResponseMessage response = await this._client.PostAsJsonAsync(
                "/api/auth/register", this.MakeRegisterBody("short@test.com", "Participant", "123"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithCorrectPassword_Returns200()
        {
            await this._client.PostAsJsonAsync(
                "/api/auth/register", this.MakeRegisterBody("login@test.com", "Participant"));

            var loginBody = new { email = "login@test.com", password = "Pass123" };
            HttpResponseMessage response = await this._client.PostAsJsonAsync("/api/auth/login", loginBody);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithWrongPassword_Returns401()
        {
            await this._client.PostAsJsonAsync(
                "/api/auth/register", this.MakeRegisterBody("wrong@test.com", "Participant"));

            var loginBody = new { email = "wrong@test.com", password = "WrongPassword" };
            HttpResponseMessage response = await this._client.PostAsJsonAsync("/api/auth/login", loginBody);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithUnknownEmail_Returns401()
        {
            var loginBody = new { email = "nobody@test.com", password = "Pass123" };
            HttpResponseMessage response = await this._client.PostAsJsonAsync("/api/auth/login", loginBody);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Profile_WhenNotLoggedIn_Returns401()
        {
            HttpResponseMessage response = await this._client.GetAsync("/api/profile");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Profile_WhenLoggedIn_Returns200_ThenAfterLogoutReturns401()
        {
            HttpClient participant = await this.CreateLoggedInClient("session@test.com", "Participant");

            HttpResponseMessage whileLoggedIn = await participant.GetAsync("/api/profile");
            Assert.Equal(HttpStatusCode.OK, whileLoggedIn.StatusCode);

            await participant.PostAsync("/api/auth/logout", null);

            HttpResponseMessage afterLogout = await participant.GetAsync("/api/profile");
            Assert.Equal(HttpStatusCode.Unauthorized, afterLogout.StatusCode);
        }
    }
}