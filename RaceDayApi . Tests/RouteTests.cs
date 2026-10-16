using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDayApi.Tests
{
    public class RouteTests : TestBase
    {
        private object MakeRouteBody()
        {
            return new
            {
                routeName = "City Loop",
                distanceKm = 10,
                elevationGainM = 85
            };
        }

        [Fact]
        public async Task GetRoute_WhenNoRouteAdded_Returns404()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            int eventId = await this.CreateEvent(organiser);

            HttpResponseMessage response = await this._client.GetAsync("/api/events/" + eventId + "/route");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetRoute_ForUnknownEvent_Returns404()
        {
            HttpResponseMessage response = await this._client.GetAsync("/api/events/9999/route");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddRoute_AsOwner_Returns201()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            int eventId = await this.CreateEvent(organiser);

            HttpResponseMessage response = await organiser.PostAsJsonAsync(
                "/api/events/" + eventId + "/route", this.MakeRouteBody());

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task AddRoute_ThenGetRoute_Returns200_WithTheRoute()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            int eventId = await this.CreateEvent(organiser);

            await organiser.PostAsJsonAsync("/api/events/" + eventId + "/route", this.MakeRouteBody());

            HttpResponseMessage response = await this._client.GetAsync("/api/events/" + eventId + "/route");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JsonElement list = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(1, list.GetArrayLength());
            Assert.Equal("City Loop", list[0].GetProperty("routeName").GetString());
        }

        [Fact]
        public async Task AddRoute_WhenNotLoggedIn_Returns401()
        {
            HttpResponseMessage response = await this._client.PostAsJsonAsync(
                "/api/events/1/route", this.MakeRouteBody());

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AddRoute_AsParticipant_Returns403()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            HttpClient participant = await this.CreateLoggedInClient("part@test.com", "Participant");
            int eventId = await this.CreateEvent(organiser);

            HttpResponseMessage response = await participant.PostAsJsonAsync(
                "/api/events/" + eventId + "/route", this.MakeRouteBody());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task AddRoute_AsDifferentOrganiser_Returns403()
        {
            HttpClient owner = await this.CreateLoggedInClient("owner@test.com", "Organiser");
            HttpClient other = await this.CreateLoggedInClient("other@test.com", "Organiser");
            int eventId = await this.CreateEvent(owner);

            HttpResponseMessage response = await other.PostAsJsonAsync(
                "/api/events/" + eventId + "/route", this.MakeRouteBody());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task AddRoute_ForUnknownEvent_Returns404()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");

            HttpResponseMessage response = await organiser.PostAsJsonAsync(
                "/api/events/9999/route", this.MakeRouteBody());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddRoute_WithZeroDistance_Returns400()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            int eventId = await this.CreateEvent(organiser);

            var body = new { routeName = "Bad Route", distanceKm = 0, elevationGainM = 10 };
            HttpResponseMessage response = await organiser.PostAsJsonAsync(
                "/api/events/" + eventId + "/route", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AddRoute_WithEmptyName_Returns400()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            int eventId = await this.CreateEvent(organiser);

            var body = new { routeName = "", distanceKm = 10, elevationGainM = 10 };
            HttpResponseMessage response = await organiser.PostAsJsonAsync(
                "/api/events/" + eventId + "/route", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AddRoute_WithNegativeElevation_Returns400()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            int eventId = await this.CreateEvent(organiser);

            var body = new { routeName = "Downhill", distanceKm = 10, elevationGainM = -5 };
            HttpResponseMessage response = await organiser.PostAsJsonAsync(
                "/api/events/" + eventId + "/route", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}