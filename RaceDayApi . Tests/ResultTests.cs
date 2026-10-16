using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDayApi.Tests
{
    public class ResultTests : TestBase
    {
        // Enrols a participant and returns the new enrolment id
        private async Task<int> EnrolAndGetId(HttpClient participant, int eventId, int categoryId)
        {
            HttpResponseMessage response = await this.Enrol(participant, eventId, categoryId);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            JsonElement json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("enrolmentId").GetInt32();
        }

        private object MakeResultBody(int enrolmentId)
        {
            return new
            {
                enrolmentId = enrolmentId,
                finishTime = "00:52:30",
                overallPosition = 12,
                categoryPosition = 1,
                status = "Finished"
            };
        }

        [Fact]
        public async Task CaptureResult_AsOwner_Returns201()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            HttpClient participant = await this.CreateLoggedInClient("part@test.com", "Participant");
            int eventId = await this.CreateEvent(organiser);
            int categoryId = await this.CreateCategory(organiser, eventId);
            int enrolmentId = await this.EnrolAndGetId(participant, eventId, categoryId);

            HttpResponseMessage response = await organiser.PostAsJsonAsync(
                "/api/events/" + eventId + "/results", this.MakeResultBody(enrolmentId));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task CaptureResult_WhenNotLoggedIn_Returns401()
        {
            HttpResponseMessage response = await this._client.PostAsJsonAsync(
                "/api/events/1/results", this.MakeResultBody(1));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CaptureResult_AsDifferentOrganiser_Returns403()
        {
            HttpClient owner = await this.CreateLoggedInClient("owner@test.com", "Organiser");
            HttpClient other = await this.CreateLoggedInClient("other@test.com", "Organiser");
            HttpClient participant = await this.CreateLoggedInClient("part@test.com", "Participant");
            int eventId = await this.CreateEvent(owner);
            int categoryId = await this.CreateCategory(owner, eventId);
            int enrolmentId = await this.EnrolAndGetId(participant, eventId, categoryId);

            HttpResponseMessage response = await other.PostAsJsonAsync(
                "/api/events/" + eventId + "/results", this.MakeResultBody(enrolmentId));

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CaptureResult_ForUnknownEvent_Returns404()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");

            HttpResponseMessage response = await organiser.PostAsJsonAsync(
                "/api/events/9999/results", this.MakeResultBody(1));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CaptureResult_Twice_Returns400()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            HttpClient participant = await this.CreateLoggedInClient("part@test.com", "Participant");
            int eventId = await this.CreateEvent(organiser);
            int categoryId = await this.CreateCategory(organiser, eventId);
            int enrolmentId = await this.EnrolAndGetId(participant, eventId, categoryId);

            await organiser.PostAsJsonAsync("/api/events/" + eventId + "/results", this.MakeResultBody(enrolmentId));
            HttpResponseMessage second = await organiser.PostAsJsonAsync(
                "/api/events/" + eventId + "/results", this.MakeResultBody(enrolmentId));

            Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
        }

        [Fact]
        public async Task CaptureResult_FinishedWithoutTime_Returns400()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            HttpClient participant = await this.CreateLoggedInClient("part@test.com", "Participant");
            int eventId = await this.CreateEvent(organiser);
            int categoryId = await this.CreateCategory(organiser, eventId);
            int enrolmentId = await this.EnrolAndGetId(participant, eventId, categoryId);

            var body = new { enrolmentId = enrolmentId, overallPosition = 5, status = "Finished" };
            HttpResponseMessage response = await organiser.PostAsJsonAsync(
                "/api/events/" + eventId + "/results", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CaptureResult_WithInvalidStatus_Returns400()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            HttpClient participant = await this.CreateLoggedInClient("part@test.com", "Participant");
            int eventId = await this.CreateEvent(organiser);
            int categoryId = await this.CreateCategory(organiser, eventId);
            int enrolmentId = await this.EnrolAndGetId(participant, eventId, categoryId);

            var body = new { enrolmentId = enrolmentId, finishTime = "01:00:00", overallPosition = 5, status = "Disqualified" };
            HttpResponseMessage response = await organiser.PostAsJsonAsync(
                "/api/events/" + eventId + "/results", body);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CaptureResult_WithUnknownEnrolment_Returns400()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            int eventId = await this.CreateEvent(organiser);

            HttpResponseMessage response = await organiser.PostAsJsonAsync(
                "/api/events/" + eventId + "/results", this.MakeResultBody(9999));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CaptureResult_DNFWithoutTime_Returns201()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            HttpClient participant = await this.CreateLoggedInClient("part@test.com", "Participant");
            int eventId = await this.CreateEvent(organiser);
            int categoryId = await this.CreateCategory(organiser, eventId);
            int enrolmentId = await this.EnrolAndGetId(participant, eventId, categoryId);

            var body = new { enrolmentId = enrolmentId, status = "DNF" };
            HttpResponseMessage response = await organiser.PostAsJsonAsync(
                "/api/events/" + eventId + "/results", body);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task GetEventResults_ReturnsTheCapturedResult()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            HttpClient participant = await this.CreateLoggedInClient("part@test.com", "Participant");
            int eventId = await this.CreateEvent(organiser);
            int categoryId = await this.CreateCategory(organiser, eventId);
            int enrolmentId = await this.EnrolAndGetId(participant, eventId, categoryId);

            await organiser.PostAsJsonAsync("/api/events/" + eventId + "/results", this.MakeResultBody(enrolmentId));

            HttpResponseMessage response = await this._client.GetAsync("/api/events/" + eventId + "/results");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JsonElement list = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(1, list.GetArrayLength());
            Assert.Equal(12, list[0].GetProperty("overallPosition").GetInt32());
        }

        [Fact]
        public async Task GetEventResults_ForUnknownEvent_Returns404()
        {
            HttpResponseMessage response = await this._client.GetAsync("/api/events/9999/results");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetMyResults_ShowsOnlyTheParticipantsOwnResult()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            HttpClient first = await this.CreateLoggedInClient("part1@test.com", "Participant");
            HttpClient second = await this.CreateLoggedInClient("part2@test.com", "Participant");
            int eventId = await this.CreateEvent(organiser);
            int categoryId = await this.CreateCategory(organiser, eventId);

            int firstEnrolment = await this.EnrolAndGetId(first, eventId, categoryId);
            int secondEnrolment = await this.EnrolAndGetId(second, eventId, categoryId);

            await organiser.PostAsJsonAsync("/api/events/" + eventId + "/results", this.MakeResultBody(firstEnrolment));
            await organiser.PostAsJsonAsync("/api/events/" + eventId + "/results", this.MakeResultBody(secondEnrolment));

            HttpResponseMessage response = await first.GetAsync("/api/users/me/results");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JsonElement list = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(1, list.GetArrayLength());
        }

        [Fact]
        public async Task CancelEnrolment_AfterResultCaptured_Returns400()
        {
            HttpClient organiser = await this.CreateLoggedInClient("org@test.com", "Organiser");
            HttpClient participant = await this.CreateLoggedInClient("part@test.com", "Participant");
            int eventId = await this.CreateEvent(organiser);
            int categoryId = await this.CreateCategory(organiser, eventId);
            int enrolmentId = await this.EnrolAndGetId(participant, eventId, categoryId);

            await organiser.PostAsJsonAsync("/api/events/" + eventId + "/results", this.MakeResultBody(enrolmentId));

            HttpResponseMessage response = await participant.DeleteAsync("/api/enrolments/" + enrolmentId);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}