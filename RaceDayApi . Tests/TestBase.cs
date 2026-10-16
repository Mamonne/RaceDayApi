using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDayApi.Tests
{
    // Shared helper methods used by all test classes
    public class TestBase : IDisposable
    {
        protected readonly ApiFactory _factory;
        protected readonly HttpClient _client;   // a client that is NOT logged in

        public TestBase()
        {
            this._factory = new ApiFactory();
            this._client = this._factory.CreateClient();
        }

        public void Dispose()
        {
            this._client.Dispose();
            this._factory.Dispose();
        }

        protected object MakeRegisterBody(string email, string role, string password = "Pass123")
        {
            return new
            {
                fullName = "Test User",
                email = email,
                password = password,
                role = role,
                phoneNumber = "0821234567",
                dateOfBirth = "2000-01-01T00:00:00"
            };
        }

        protected object MakeEventBody(string eventType = "Running")
        {
            return new
            {
                eventName = "Test Fun Run",
                eventDate = DateTime.Today.AddDays(30),
                location = "Bloemfontein",
                description = "A test event",
                eventType = eventType
            };
        }

        protected object MakeCategoryBody(int maxParticipants = 100)
        {
            return new
            {
                categoryName = "10km Run",
                distance = 10,
                entryFee = 150,
                maxParticipants = maxParticipants
            };
        }

        // Registers a user, logs in, and returns a client that stays logged in
        protected async Task<HttpClient> CreateLoggedInClient(string email, string role)
        {
            HttpClient client = this._factory.CreateClient();

            HttpResponseMessage registerResponse = await client.PostAsJsonAsync(
                "/api/auth/register", this.MakeRegisterBody(email, role));
            Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

            var loginBody = new { email = email, password = "Pass123" };
            HttpResponseMessage loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginBody);
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            return client;
        }

        // Creates an event as the given Organiser and returns its id
        protected async Task<int> CreateEvent(HttpClient organiserClient)
        {
            HttpResponseMessage response = await organiserClient.PostAsJsonAsync(
                "/api/events", this.MakeEventBody());
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            JsonElement json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("eventId").GetInt32();
        }

        // Creates a category for an event and returns its id
        protected async Task<int> CreateCategory(HttpClient organiserClient, int eventId, int maxParticipants = 100)
        {
            HttpResponseMessage response = await organiserClient.PostAsJsonAsync(
                "/api/events/" + eventId + "/categories", this.MakeCategoryBody(maxParticipants));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            JsonElement json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("categoryId").GetInt32();
        }

        // Enrols a participant and returns the response
        protected async Task<HttpResponseMessage> Enrol(HttpClient participantClient, int eventId, int categoryId)
        {
            var body = new { categoryId = categoryId };
            return await participantClient.PostAsJsonAsync("/api/events/" + eventId + "/enrolments", body);
        }
    }
}