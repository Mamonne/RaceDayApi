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
    }
}