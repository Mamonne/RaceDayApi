using System.Net;
using System.Net.Http.Json;

namespace RaceDayApi.Tests
{
    // Checks that each role is blocked from the other role's functions
    public class RoleTests : TestBase
    {
        [Fact]
        public async Task CreateCategory_AsParticipant_Returns403()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            HttpClient participant = await this.CreateLoggedInClient("part@test.com", "Participant");
            int eventId = await this.CreateEvent(organiser);

            HttpResponseMessage response = await participant.PostAsJsonAsync(
                "/api/events/" + eventId + "/categories", this.MakeCategoryBody());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Enrol_AsOrganiser_Returns403()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            int eventId = await this.CreateEvent(organiser);
            int categoryId = await this.CreateCategory(organiser, eventId);

            HttpResponseMessage response = await this.Enrol(organiser, eventId, categoryId);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ViewEventEnrolments_AsParticipant_Returns403()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            HttpClient participant = await this.CreateLoggedInClient("part@test.com", "Participant");
            int eventId = await this.CreateEvent(organiser);

            HttpResponseMessage response = await participant.GetAsync("/api/events/" + eventId + "/enrolments");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CaptureResult_AsParticipant_Returns403()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            HttpClient participant = await this.CreateLoggedInClient("part@test.com", "Participant");
            int eventId = await this.CreateEvent(organiser);

            var body = new { enrolmentId = 1, finishTime = "00:50:00", overallPosition = 1, status = "Finished" };
            HttpResponseMessage response = await participant.PostAsJsonAsync(
                "/api/events/" + eventId + "/results", body);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ViewMyResults_AsOrganiser_Returns403()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");

            HttpResponseMessage response = await organiser.GetAsync("/api/users/me/results");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ViewMyResults_WhenNotLoggedIn_Returns401()
        {
            HttpResponseMessage response = await this._client.GetAsync("/api/users/me/results");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ViewMyResults_AsParticipant_Returns200()
        {
            HttpClient participant = await this.CreateLoggedInClient("part@test.com", "Participant");

            HttpResponseMessage response = await participant.GetAsync("/api/users/me/results");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}