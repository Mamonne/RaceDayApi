using System.Net;
using System.Net.Http.Json;

namespace RaceDayApi.Tests
{
    public class EventTests : TestBase
    {
        [Fact]
        public async Task GetEvents_WhenNotLoggedIn_Returns200()
        {
            HttpResponseMessage response = await this._client.GetAsync("/api/events");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_WhenNotLoggedIn_Returns401()
        {
            HttpResponseMessage response = await this._client.PostAsJsonAsync(
                "/api/events", this.MakeEventBody());

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_AsParticipant_Returns403()
        {
            HttpClient participant = await this.CreateLoggedInClient("part@test.com", "Participant");

            HttpResponseMessage response = await participant.PostAsJsonAsync(
                "/api/events", this.MakeEventBody());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_AsOrganiser_Returns201()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");

            HttpResponseMessage response = await organiser.PostAsJsonAsync(
                "/api/events", this.MakeEventBody());

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_WithInvalidType_Returns400()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");

            HttpResponseMessage response = await organiser.PostAsJsonAsync(
                "/api/events", this.MakeEventBody("Swimming"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetEvent_WithUnknownId_Returns404()
        {
            HttpResponseMessage response = await this._client.GetAsync("/api/events/9999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateEvent_AsOwner_Returns200()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            int eventId = await this.CreateEvent(organiser);

            HttpResponseMessage response = await organiser.PutAsJsonAsync(
                "/api/events/" + eventId, this.MakeEventBody("Cycling"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task UpdateEvent_AsDifferentOrganiser_Returns403()
        {
            HttpClient owner = await this.CreateLoggedInClient("owner@test.com", "Organiser");
            HttpClient other = await this.CreateLoggedInClient("other@test.com", "Organiser");
            int eventId = await this.CreateEvent(owner);

            HttpResponseMessage response = await other.PutAsJsonAsync(
                "/api/events/" + eventId, this.MakeEventBody());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DeleteEvent_AsDifferentOrganiser_Returns403()
        {
            HttpClient owner = await this.CreateLoggedInClient("owner@test.com", "Organiser");
            HttpClient other = await this.CreateLoggedInClient("other@test.com", "Organiser");
            int eventId = await this.CreateEvent(owner);

            HttpResponseMessage response = await other.DeleteAsync("/api/events/" + eventId);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DeleteEvent_AsOwner_Returns200_ThenEventIsGone()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            int eventId = await this.CreateEvent(organiser);

            HttpResponseMessage deleteResponse = await organiser.DeleteAsync("/api/events/" + eventId);
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            HttpResponseMessage getResponse = await this._client.GetAsync("/api/events/" + eventId);
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_WithZeroDistance_Returns400()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");

            var body = new
            {
                eventName = "Test Run",
                eventDate = DateTime.Today.AddDays(30),
                location = "Bloemfontein",
                description = "No distance",
                distance = 0,
                eventType = "Running"
            };
            HttpResponseMessage response = await organiser.PostAsJsonAsync("/api/events", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        [Fact]
        public async Task GetMyEvents_AsOrganiser_ReturnsOnlyTheirOwnEvents()
        {
            HttpClient first = await this.CreateLoggedInClient("first@test.com", "Organiser");
            HttpClient second = await this.CreateLoggedInClient("second@test.com", "Organiser");
            await this.CreateEvent(first);
            await this.CreateEvent(second);

            HttpResponseMessage response = await first.GetAsync("/api/events/mine");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            System.Text.Json.JsonElement list = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            Assert.Equal(1, list.GetArrayLength());
        }

        [Fact]
        public async Task GetMyEvents_AsParticipant_Returns403()
        {
            HttpClient participant = await this.CreateLoggedInClient("part@test.com", "Participant");

            HttpResponseMessage response = await participant.GetAsync("/api/events/mine");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        private object MakeCustomEventBody(string name, decimal distance, string eventType)
        {
            return new
            {
                eventName = name,
                eventDate = DateTime.Today.AddDays(30),
                location = "Bloemfontein",
                description = "Filter test",
                distance = distance,
                eventType = eventType
            };
        }

        [Fact]
        public async Task GetEvents_FilterByEventType_ReturnsOnlyThatType()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            await organiser.PostAsJsonAsync("/api/events", this.MakeCustomEventBody("Run One", 10, "Running"));
            await organiser.PostAsJsonAsync("/api/events", this.MakeCustomEventBody("Cycle One", 50, "Cycling"));

            HttpResponseMessage response = await this._client.GetAsync("/api/events?eventType=Cycling");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            System.Text.Json.JsonElement list = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            Assert.Equal(1, list.GetArrayLength());
            Assert.Equal("Cycle One", list[0].GetProperty("eventName").GetString());
        }

        [Fact]
        public async Task GetEvents_Search_FindsEventByName()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            await organiser.PostAsJsonAsync("/api/events", this.MakeCustomEventBody("Bloem Marathon", 42, "Running"));
            await organiser.PostAsJsonAsync("/api/events", this.MakeCustomEventBody("Park Walk", 5, "Walking"));

            HttpResponseMessage response = await this._client.GetAsync("/api/events?search=marathon");

            System.Text.Json.JsonElement list = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            Assert.Equal(1, list.GetArrayLength());
            Assert.Equal("Bloem Marathon", list[0].GetProperty("eventName").GetString());
        }

        [Fact]
        public async Task GetEvents_FilterByMinimumDistance_ReturnsOnlyLongerEvents()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            await organiser.PostAsJsonAsync("/api/events", this.MakeCustomEventBody("Short Run", 5, "Running"));
            await organiser.PostAsJsonAsync("/api/events", this.MakeCustomEventBody("Long Run", 42, "Running"));

            HttpResponseMessage response = await this._client.GetAsync("/api/events?minDistance=20");

            System.Text.Json.JsonElement list = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            Assert.Equal(1, list.GetArrayLength());
            Assert.Equal("Long Run", list[0].GetProperty("eventName").GetString());
        }

        [Fact]
        public async Task GetEvents_WithInvalidEventTypeFilter_Returns400()
        {
            HttpResponseMessage response = await this._client.GetAsync("/api/events?eventType=Swimming");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}